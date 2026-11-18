using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Controllers.Common.ViewModels;

namespace ALE.ACS.Presentation.Controllers.PID.ViewModels;

/// <summary>
/// Editor do controlador PID. Seções: Variáveis do laço, Sintonia,
/// Modos de operação, Limites de saída.
/// </summary>
public sealed partial class PidEditorViewModel : ControllerEditorViewModelBase
{
    public PidEditorViewModel()
        : base(CreateDefault())
    {
    }

    private static ControllerEditorModel CreateDefault()
    {
        return new ControllerEditorModel(
            name: "PID Controller",
            description: "Controlador PID clássico (ISA) com anti-windup por back-calculation e derivação sobre PV.",
            controllerKind: "PID",
            sections:
            [
                new ControllerSection(
                    title: "Variáveis do laço",
                    icon: "▶",
                    hint: "Setpoint, variável controlada (PV) e variável manipulada (MV).",
                    items: ["Setpoint", "Controlled (PV)", "Manipulated (MV)"]),
                new ControllerSection(
                    title: "Sintonia",
                    icon: "∿",
                    hint: "Ganhos do PID: Kp (proporcional), Ti (integral, s), Td (derivativo, s), N (filtro derivativo).",
                    items: ["Kp = 1.0", "Ti = 10.0 s", "Td = 0.0 s", "N = 8.0"]),
                new ControllerSection(
                    title: "Modos de operação",
                    icon: "⇒",
                    hint: "Auto/Manual, ação direta/reversa, bumpless transfer.",
                    items: ["Auto", "DirectAction = false", "Bumpless = true"]),
                new ControllerSection(
                    title: "Limites de saída",
                    icon: "◀",
                    hint: "Saturação da MV (OutMin/OutMax) e estratégia de anti-windup.",
                    items: ["OutMin = 0.0", "OutMax = 100.0", "Anti-windup = back-calculation"])
            ]);
    }
}
