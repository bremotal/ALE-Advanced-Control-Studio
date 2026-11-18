using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Controllers.Common.ViewModels;

namespace ALE.ACS.Presentation.Controllers.Identification.ViewModels;

/// <summary>
/// Editor de Identificação de processo. Seções: Método, Dados de entrada,
/// Dados de saída, Modelo estimado, Validação.
/// </summary>
public sealed partial class IdentificationEditorViewModel : ControllerEditorViewModelBase
{
    public IdentificationEditorViewModel()
        : base(CreateDefault())
    {
    }

    private static ControllerEditorModel CreateDefault()
    {
        return new ControllerEditorModel(
            name: "Identification",
            description: "Identificação de modelo de processo a partir de dados de teste (degrau/PRBS).",
            controllerKind: "Identification",
            sections:
            [
                new ControllerSection(
                    title: "Método",
                    icon: "▶",
                    hint: "Algoritmo de identificação (mínimos quadrados, RLS, ERA, N4SID).",
                    items: ["Método: Least Squares", "Modelo: FOPDT"]),
                new ControllerSection(
                    title: "Dados de entrada",
                    icon: "↘",
                    hint: "Sinal de excitação aplicado ao processo (degrau, PRBS, seno).",
                    items: ["Tipo: Step", "Amplitude: 10%", "Duração: 600 s"]),
                new ControllerSection(
                    title: "Dados de saída",
                    icon: "↗",
                    hint: "Resposta medidas (PV) sincronizadas com a entrada.",
                    items: ["PV: Temperature", "Sample time: 1 s"]),
                new ControllerSection(
                    title: "Modelo estimado",
                    icon: "∿",
                    hint: "Parâmetros identificados do modelo FOPDT/SOPDT.",
                    items: ["K = 1.05", "τ = 12.3 s", "θ = 1.8 s"]),
                new ControllerSection(
                    title: "Validação",
                    icon: "✓",
                    hint: "Métricas de ajuste (FIT%, RMSE) e análise de resíduos.",
                    items: ["FIT = 96.2%", "RMSE = 0.31"])
            ]);
    }
}
