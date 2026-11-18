using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Controllers.Common.ViewModels;

namespace ALE.ACS.Presentation.Controllers.MPC.ViewModels;

/// <summary>
/// Editor do controlador MPC. Seções: Variáveis (CV/MV/DV), Horizontes,
/// Pesos, Restrições, Modelo dinâmico.
/// </summary>
public sealed partial class MpcEditorViewModel : ControllerEditorViewModelBase
{
    public MpcEditorViewModel()
        : base(CreateDefault())
    {
    }

    private static ControllerEditorModel CreateDefault()
    {
        return new ControllerEditorModel(
            name: "MPC Controller",
            description: "Model Predictive Control com QP (gradient-projection), matriz de predição S e ganho K offline.",
            controllerKind: "MPC",
            sections:
            [
                new ControllerSection(
                    title: "Variáveis (CV/MV/DV)",
                    icon: "▶",
                    hint: "Variáveis controladas, manipuladas e distúrbios medidos.",
                    items: ["CV: Temperature", "MV: Valve", "DV: FeedFlow"]),
                new ControllerSection(
                    title: "Horizontes",
                    icon: "⏱",
                    hint: "P = horizonte de predição, M = horizonte de controle (amostras).",
                    items: ["P = 20", "M = 5"]),
                new ControllerSection(
                    title: "Pesos",
                    icon: "⚖",
                    hint: "Peso no erro (Q), peso na ação de controle (R), peso na variação (Δu).",
                    items: ["Q = 1.0", "R = 0.1", "Δu weight = 0.5"]),
                new ControllerSection(
                    title: "Restrições",
                    icon: "■",
                    hint: "Limites em MV, ΔMV e CV (caixa do QP).",
                    items: ["MV ∈ [0, 100]", "ΔMV ∈ [-5, 5]", "CV ∈ [10, 90]"]),
                new ControllerSection(
                    title: "Modelo dinâmico",
                    icon: "∿",
                    hint: "Modelo FOPDT/SOPDT discretizado; matriz de ganho K pré-resolvida offline.",
                    items: ["FOPDT: K=1.0, τ=10s, θ=2s", "K matrix: 20×5"])
            ]);
    }
}
