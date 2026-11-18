using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Optimization;

namespace ALE.ACS.Engine.Mpc;

/// <summary>
/// Controlador MPC que resolve o QP a cada scan. Existe para validação offline e debug.
/// Em produção no PLC, usa-se a matriz de ganho K pré-computada em <see cref="MpcDesign"/>
/// (não resolve QP no PLC — apenas multiplica K pela diferença x̂ - x_ref).
/// </summary>
public sealed class MpcController : ControllerBase
{
    private readonly MpcSpec _spec;
    private readonly QpSolver _qp = new();
    private readonly Matrix? _S;
    private readonly Matrix? _H;
    private readonly double[]? _uPrev;

    public MpcController(MpcSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));

        var design = (MpcDesign)spec.Design();
        if (!design.IsValid)
            throw new InvalidOperationException($"MPC spec inválida: {design.Summary}");

        _S = design.S;

        // Pré-monta H = 2 * (S' Q S + R + ΔR)
        var Hp = spec.PredictionHorizon;
        var Hu = spec.ControlHorizon;
        var StS = _S!.Transpose().Multiply(_S);
        _H = StS.Scale(2.0 * spec.OutputWeight);
        for (int i = 0; i < Hu; i++)
            _H[i, i] += 2.0 * (spec.InputWeight + spec.InputRateWeight);
        _uPrev = new double[Hu];
    }

    public MpcSpec Spec => _spec;
    public Matrix? GainMatrix => ((MpcDesign)_spec.Design()).K;

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : 0.0;
        var pv = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;

        // Estado estimado (simplificado: assume que o modelo está sincronizado com a planta)
        // Em produção, usaria observador (Kalman/Luenberger).
        var ypred = new double[_spec.PredictionHorizon];
        for (int i = 0; i < _spec.PredictionHorizon; i++)
        {
            ypred[i] = pv;  // predição livre (sem dinâmica — simplificação para validação)
        }

        // Vetor de referência: SP constante durante todo o horizonte
        var refVec = Enumerable.Repeat(sp, _spec.PredictionHorizon).ToArray();

        // Erro de predição
        var err = new double[_spec.PredictionHorizon];
        for (int i = 0; i < _spec.PredictionHorizon; i++)
            err[i] = refVec[i] - ypred[i];

        // Gradiente: g = -2 S' Q err  (Q = q*I)
        var g = new double[_spec.ControlHorizon];
        for (int j = 0; j < _spec.ControlHorizon; j++)
        {
            double s = 0;
            for (int i = 0; i < _spec.PredictionHorizon; i++)
                s += _S![i, j] * err[i];
            g[j] = -2.0 * _spec.OutputWeight * s;
        }

        // Limites de Δu: InputMin/Max - u_prev (para converter em restrição de Δu)
        var lb = new double[_spec.ControlHorizon];
        var ub = new double[_spec.ControlHorizon];
        for (int i = 0; i < _spec.ControlHorizon; i++)
        {
            lb[i] = Math.Max(_spec.InputRateMin, _spec.InputMin - _uPrev![i]);
            ub[i] = Math.Min(_spec.InputRateMax, _spec.InputMax - _uPrev[i]);
        }

        // Resolve QP
        var sol = _qp.Solve(_H!, g, lb, ub);
        var du = sol.X;

        // Aplica apenas o primeiro Δu (receding horizon)
        var u = _uPrev![0] + du[0];
        // Atualiza histórico (shift)
        for (int i = 0; i < _spec.ControlHorizon - 1; i++)
            _uPrev[i] = _uPrev[i + 1] + du[i + 1];
        _uPrev[^1] = u;

        // Satura
        u = Math.Max(_spec.InputMin, Math.Min(_spec.InputMax, u));

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, u);

        var severity = sol.Optimal ? ControllerMessageSeverity.Info : ControllerMessageSeverity.Warning;
        return new ControllerResult(
            outputs,
            sol.Optimal ? ControllerStatus.Success : ControllerStatus.Warning,
            [new ControllerMessage(severity, "MPC_STEP",
                $"SP={sp:F3} PV={pv:F3} MV={u:F3} Δu={du[0]:F4} obj={sol.Objective:F4} opt={sol.Optimal}",
                DateTime.UtcNow)],
            TimeSpan.Zero, true);
    }
}
