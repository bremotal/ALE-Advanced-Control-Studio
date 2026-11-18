using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Controllers.Common.ViewModels;

namespace ALE.ACS.Presentation.Controllers.Neural.ViewModels;

/// <summary>
/// Editor de rede neural para modelagem/controle. Seções: Topologia, Camadas,
/// Funções de ativação, Treino, Aplicação.
/// </summary>
public sealed partial class NeuralEditorViewModel : ControllerEditorViewModelBase
{
    public NeuralEditorViewModel()
        : base(CreateDefault())
    {
    }

    private static ControllerEditorModel CreateDefault()
    {
        return new ControllerEditorModel(
            name: "Neural Network",
            description: "Rede neural feedforward para modelagem de processo ou controle por inversão.",
            controllerKind: "Neural",
            sections:
            [
                new ControllerSection(
                    title: "Topologia",
                    icon: "▦",
                    hint: "Arquitetura da rede (entradas, saídas, camadas ocultas).",
                    items: ["Inputs: 3", "Outputs: 1", "Hidden layers: 2"]),
                new ControllerSection(
                    title: "Camadas",
                    icon: "▯",
                    hint: "Neurônios por camada oculta.",
                    items: ["L1: 16 neurons", "L2: 8 neurons"]),
                new ControllerSection(
                    title: "Funções de ativação",
                    icon: "∿",
                    hint: "Função de ativação por camada (ReLU, Tanh, Sigmoid).",
                    items: ["L1: ReLU", "L2: Tanh", "Output: Linear"]),
                new ControllerSection(
                    title: "Treino",
                    icon: "⏱",
                    hint: "Algoritmo de treinamento (Adam, SGD), épocas, learning rate.",
                    items: ["Optimizer: Adam", "Epochs: 1000", "LR = 0.001", "Batch = 32"]),
                new ControllerSection(
                    title: "Aplicação",
                    icon: "⇒",
                    hint: "Modo de uso (modelo direto, inversão, controle por rede).",
                    items: ["Mode: Forward model", "Input: [SP, PV, MV_prev]"])
            ]);
    }
}
