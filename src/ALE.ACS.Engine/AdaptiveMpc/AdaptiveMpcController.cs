using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Mpc;
using ALE.ACS.Engine.Optimization;

namespace ALE.ACS.Engine.AdaptiveMpc;

/// <summary>
/// Controlador MPC adaptativo. Combina:
///   1. Identificação online do modelo via RLS (a cada scan: observa (u, y), atualiza θ).
///   2. Detecção de mudança de parâmetro (norma relativa de Δθ > threshold).
///   3. Re-design do QP quando o modelo muda (recomputa matriz de ganho K).
///   4. Resolução do QP com o modelo atual (igual ao MPC fixo).
/// </summary>
public sealed class AdaptiveMpcController : ControllerBase
{
    private readonly AdaptiveMpcSpec _spec;
    private readonly RecursiveLeastSquares _rls;
    private readonly QpSolver _qp = new();
    private MpcSpec _currentMpcSpec;
    private MpcController? _currentMpc;
    private Matrix? _S;
    private Matrix? _H;
    private double[] _uPrev;
    private double[]? _thetaPrev;
    private double _yPrev;
    private bool _firstScan = true;
    private int _redesignCount;

    public AdaptiveMpcController(AdaptiveMpcSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        _rls = new RecursiveLeastSquares(spec.ArxOrder, spec.ForgettingFactor);
        _currentMpcSpec = spec.BuildMpcSpec(spec.InitialModel);
        _currentMpc = new MpcController(_currentMpcSpec);
        _uPrev = new double[spec.ControlHorizon];
        _yPrev = 0.0;
        RedesignQp();
    }

    public AdaptiveMpcSpec Spec => _spec;
    public double[] EstimatedParameters => _rls.Theta;
    public int RedesignCount => _redesignCount;
    public Matrix? CurrentGain => _S;

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : 0.0;
        var pv = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;
        var uPrevVal = _uPrev.Length > 0 ? _uPrev[0] : 0.0;

        var messages = new List<ControllerMessage>();

        // 1. Atualiza RLS com a observação mais recente: φ = [-y(k-1), u(k-1)] para ordem 2
        if (!_firstScan)
        {
            var phi = BuildRegressor(_yPrev, uPrevVal);
            _rls.Update(phi, pv);

            // 2. Verifica se os parâmetros mudaram significativamente
            if (_thetaPrev is not null && ShouldRedesign())
            {
                RebuildModel();
                RedesignQp();
                _redesignCount++;
                messages.Add(new ControllerMessage(
                    ControllerMessageSeverity.Warning, "AMPC_REDESIGN",
                    $"Parâmetros mudaram > {_spec.RedesignThreshold:P0}. Re-design #{_redesignCount}.",
                    DateTime.UtcNow));
            }
            _thetaPrev = (double[])_rls.Theta.Clone();
        }
        _yPrev = pv;
        _firstScan = false;

        // 3. Resolve o QP (igual ao MPC fixo, mas com o modelo atual)
        if (_S is null || _H is null || _currentMpc is null)
        {
            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [new ControllerMessage(ControllerMessageSeverity.Error, "AMPC_NO_QP",
                    "QP não foi inicializado.", DateTime.UtcNow)],
                TimeSpan.Zero, false);
        }

        var ypred = Enumerable.Repeat(pv, _spec.PredictionHorizon).ToArray();
        var refVec = Enumerable.Repeat(sp, _spec.PredictionHorizon).ToArray();
        var err = new double[_spec.PredictionHorizon];
        for (int i = 0; i < _spec.PredictionHorizon; i++)
            err[i] = refVec[i] - ypred[i];

        var g = new double[_spec.ControlHorizon];
        for (int j = 0; j < _spec.ControlHorizon; j++)
        {
            double s = 0;
            for (int i = 0; i < _spec.PredictionHorizon; i++)
                s += _S[i, j] * err[i];
            g[j] = -2.0 * _spec.OutputWeight * s;
        }

        var lb = new double[_spec.ControlHorizon];
        var ub = new double[_spec.ControlHorizon];
        for (int i = 0; i < _spec.ControlHorizon; i++)
        {
            lb[i] = Math.Max(_spec.InputRateMin, _spec.InputMin - _uPrev[i]);
            ub[i] = Math.Min(_spec.InputRateMax, _spec.InputMax - _uPrev[i]);
        }

        var sol = _qp.Solve(_H, g, lb, ub);
        var du = sol.X;
        var u = _uPrev[0] + du[0];
        for (int i = 0; i < _spec.ControlHorizon - 1; i++)
            _uPrev[i] = _uPrev[i + 1] + du[i + 1];
        _uPrev[^1] = u;
        u = Math.Max(_spec.InputMin, Math.Min(_spec.InputMax, u));

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, u);

        messages.Add(new ControllerMessage(
            sol.Optimal ? ControllerMessageSeverity.Info : ControllerMessageSeverity.Warning,
            "AMPC_STEP",
            $"SP={sp:F3} PV={pv:F3} MV={u:F3} Δu={du[0]:F4} opt={sol.Optimal} redesigns={_redesignCount}",
            DateTime.UtcNow));

        return new ControllerResult(
            outputs,
            sol.Optimal ? ControllerStatus.Success : ControllerStatus.Warning,
            messages,
            TimeSpan.Zero, true);
    }

    private double[] BuildRegressor(double yPrev, double uPrev)
    {
        // Para ordem 2: φ = [-y(k-1), u(k-1)]
        // Para ordem 4: φ = [-y(k-1), -y(k-2), u(k-1), u(k-2)]
        var phi = new double[_spec.ArxOrder];
        int half = _spec.ArxOrder / 2;
        for (int i = 0; i < half; i++)
            phi[i] = -_yPrev;  // simplificação: assume y(k-1) = y(k-2) = ... = yPrev
        for (int i = half; i < _spec.ArxOrder; i++)
            phi[i] = uPrev;
        return phi;
    }

    private bool ShouldRedesign()
    {
        if (_thetaPrev is null || _thetaPrev.Length != _rls.Theta.Length)
            return true;

        double num = 0, den = 0;
        for (int i = 0; i < _rls.Theta.Length; i++)
        {
            num += Math.Abs(_rls.Theta[i] - _thetaPrev[i]);
            den += Math.Abs(_thetaPrev[i]);
        }
        if (den < 1e-9) den = 1e-9;
        return (num / den) > _spec.RedesignThreshold;
    }

    private void RebuildModel()
    {
        // Reconstrói o modelo discreto a partir dos parâmetros ARX estimados
        // Para ordem 2 (FOPDT-like): y(k) = -a1 y(k-1) + b1 u(k-1)
        // → A = [a1], B = [b1], C = [1]
        var theta = _rls.Theta;
        if (_spec.ArxOrder == 2)
        {
            var a1 = -theta[0];  // theta[0] é -a1
            var b1 = theta[1];
            // Garante estabilidade: |a1| < 1
            a1 = Math.Clamp(a1, -0.999, 0.999);

            var A = new Matrix(1, 1); A[0, 0] = a1;
            var B = new Matrix(1, 1); B[0, 0] = b1;
            var C = new Matrix(1, 1); C[0, 0] = 1.0;

            var newModel = new ProcessModel($"{_spec.InitialModel.Name}_adaptive",
                _spec.InitialModel.Ts, A, B, C);
            _currentMpcSpec = _spec.BuildMpcSpec(newModel);
        }
        // Para ordens maiores, a reconstrução requer síntese de espaço de estados a partir de ARX
        // — fica como extensão futura.
    }

    private void RedesignQp()
    {
        var design = (MpcDesign)_currentMpcSpec.Design();
        if (!design.IsValid || design.S is null)
            throw new InvalidOperationException($"Re-design falhou: {design.Summary}");

        _S = design.S;
        var StS = _S.Transpose().Multiply(_S);
        _H = StS.Scale(2.0 * _spec.OutputWeight);
        for (int i = 0; i < _spec.ControlHorizon; i++)
            _H[i, i] += 2.0 * (_spec.InputWeight + _spec.InputRateWeight);

        _currentMpc = new MpcController(_currentMpcSpec);
    }
}
