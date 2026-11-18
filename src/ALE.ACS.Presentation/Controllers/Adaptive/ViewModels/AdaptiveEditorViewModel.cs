using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Controllers.Common.ViewModels;

namespace ALE.ACS.Presentation.Controllers.Adaptive.ViewModels;

/// <summary>
/// Editor do controlador Adaptive MPC. Seções: Variáveis, Identificação RLS,
/// Detecção de mudança, Redesign QP, Limites.
/// </summary>
public sealed partial class AdaptiveEditorViewModel : ControllerEditorViewModelBase
{
    public AdaptiveEditorViewModel()
        : base(CreateDefault())
    {
    }

    private static ControllerEditorModel CreateDefault()
    {
        return new ControllerEditorModel(
            name: "Adaptive MPC",
            description: "MPC adaptativo com identificação RLS (forgetting factor) + detecção de mudança + QP redesign.",
            controllerKind: "Adaptive MPC",
            sections:
            [
                new ControllerSection(
                    title: "Variáveis",
                    icon: "▶",
                    hint: "Variáveis do laço adaptativo.",
                    items: ["CV: Temperature", "MV: Valve"]),
                new ControllerSection(
                    title: "Identificação RLS",
                    icon: "∿",
                    hint: "Recursive Least Squares com fator de esquecimento λ e matriz de covariância P.",
                    items: ["λ = 0.95", "P₀ = 1000·I", "ARX order = 2"]),
                new ControllerSection(
                    title: "Detecção de mudança",
                    icon: "⏱",
                    hint: "Threshold relativo na norma de θ para disparar redesign.",
                    items: ["‖Δθ‖/‖θ‖ > 0.10"]),
                new ControllerSection(
                    title: "Redesign QP",
                    icon: "⚖",
                    hint: "Reconstroi matriz de ganho K quando mudança é detectada.",
                    items: ["Rebuild on detection", "K matrix: 20×5"]),
                new ControllerSection(
                    title: "Limites",
                    icon: "■",
                    hint: "Caixa do QP após redesign.",
                    items: ["MV ∈ [0, 100]", "ΔMV ∈ [-5, 5]"])
            ]);
    }
}
