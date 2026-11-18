using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Optimization;

/// <summary>
/// Controlador que resolve o QP/LP on-line. Existe para validação e debug no .NET;
/// em produção, o PLC recebe a solução pré-computada offline (gain matrix para MPC,
/// ou a solução direta para LP estático).
/// </summary>
public sealed class OptimizationController : ControllerBase
{
    private readonly OptimizationSpec _spec;

    public OptimizationController(OptimizationSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    public OptimizationSpec Spec => _spec;

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        try
        {
            double[] x;
            bool optimal;
            double obj;

            if (_spec.H is not null)
            {
                var qp = new QpSolver();
                var sol = qp.Solve(_spec.H, _spec.C, _spec.LowerBounds, _spec.UpperBounds);
                x = sol.X;
                optimal = sol.Optimal;
                obj = sol.Objective;
            }
            else if (_spec.A is not null && _spec.B is not null)
            {
                var lp = new SimplexSolver();
                var sol = lp.Solve(_spec.C, _spec.A, _spec.B);
                x = sol.X;
                optimal = sol.Optimal;
                obj = sol.Objective;
            }
            else
            {
                return new ControllerResult(
                    new ControllerVariableCollection(),
                    ControllerStatus.InvalidInput,
                    [new ControllerMessage(ControllerMessageSeverity.Error, "OPT_NO_FORMULATION",
                        "Spec precisa de H (QP) ou A/B (LP).", DateTime.UtcNow)],
                    TimeSpan.Zero, false);
            }

            var outputs = new ControllerVariableCollection();
            for (int i = 0; i < x.Length; i++)
                outputs.Set($"x{i}", x[i]);
            outputs.Set(_spec.Manipulated.Name, x.Length > 0 ? x[0] : 0.0);

            return new ControllerResult(
                outputs,
                optimal ? ControllerStatus.Success : ControllerStatus.Warning,
                [new ControllerMessage(
                    ControllerMessageSeverity.Info, "OPT_OK",
                    $"obj={obj:F6}, optimal={optimal}, n={x.Length}", DateTime.UtcNow)],
                TimeSpan.Zero, true);
        }
        catch (Exception ex)
        {
            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [new ControllerMessage(ControllerMessageSeverity.Error, "OPT_FAIL", ex.Message, DateTime.UtcNow)],
                TimeSpan.Zero, false);
        }
    }
}
