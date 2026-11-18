using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Controllers.Common.ViewModels;

namespace ALE.ACS.Presentation.Controllers.Optimization.ViewModels;

/// <summary>
/// Editor do controlador de Otimização. Seções: Variáveis de decisão,
/// Função objetivo, Restrições, Solver, Resultado.
/// </summary>
public sealed partial class OptimizationEditorViewModel : ControllerEditorViewModelBase
{
    public OptimizationEditorViewModel()
        : base(CreateDefault())
    {
    }

    private static ControllerEditorModel CreateDefault()
    {
        return new ControllerEditorModel(
            name: "Optimization",
            description: "Otimização LP/QP standalone: maximizar produção respeitando limites operacionais.",
            controllerKind: "Optimization",
            sections:
            [
                new ControllerSection(
                    title: "Variáveis de decisão",
                    icon: "▶",
                    hint: "Variáveis que o otimizador ajusta (produção, consumo, qualidade).",
                    items: ["x1: Production", "x2: SteamFlow", "x3: Yield"]),
                new ControllerSection(
                    title: "Função objetivo",
                    icon: "⚖",
                    hint: "Critério a maximizar/minimizar (lucro, custo, consumo).",
                    items: ["maximize: 10·x1 - 2·x2 - 3·x3"]),
                new ControllerSection(
                    title: "Restrições",
                    icon: "■",
                    hint: "Limites operacionais e de qualidade (igualdade e desigualdade).",
                    items: ["x1 ≤ 100", "x2 ≥ 10", "x3 ∈ [0.85, 0.95]", "x1 + x2 ≤ 120"]),
                new ControllerSection(
                    title: "Solver",
                    icon: "⏱",
                    hint: "Algoritmo (Simplex para LP, gradient-projection + LM para QP).",
                    items: ["Type: LP", "Solver: Simplex", "Tolerance: 1e-6"]),
                new ControllerSection(
                    title: "Resultado",
                    icon: "✓",
                    hint: "Solução ótima encontrada (variáveis + valor objetivo).",
                    items: ["x1* = 95.0", "x2* = 12.5", "x3* = 0.92", "obj* = 875.0"])
            ]);
    }
}
