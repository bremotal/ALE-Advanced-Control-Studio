using ALE.ACS.Domain.Common.Base;
using ALE.ACS.Domain.Common.Results;

namespace ALE.ACS.Domain.Controllers;

/// <summary>
/// Enumera os tipos de controlador suportados pelo ALE ACS.
/// Espelha <see cref="ALE.ACS.Engine.Abstractions.ControllerKind"/>
/// para que o Domain não dependa do Engine.
/// </summary>
public enum ControllerType
{
    /// <summary>Lógica fuzzy (Mamdani/Sugeno).</summary>
    Fuzzy,

    /// <summary>Model Predictive Control com QP.</summary>
    Mpc,

    /// <summary>MPC adaptativo com identificação RLS.</summary>
    AdaptiveMpc,

    /// <summary>Cascade master/slave.</summary>
    Cascade,

    /// <summary>Smith predictor para processos com tempo morto.</summary>
    SmithPredictor,

    /// <summary>Linearização por realimentação.</summary>
    Linearization,

    /// <summary>Otimização LP/QP standalone.</summary>
    Optimization,

    /// <summary>PID clássico (single loop).</summary>
    Pid,

    /// <summary>Identificação de processo.</summary>
    Identification,

    /// <summary>Rede neural para modelagem/controle.</summary>
    Neural
}

/// <summary>
/// Estado do ciclo de vida de um controlador dentro de um projeto.
/// </summary>
public enum ControllerLifecycle
{
    /// <summary>Recém-criado, sem design computado.</summary>
    Draft,

    /// <summary>Design em andamento.</summary>
    Designing,

    /// <summary>Design concluído e validado, pronto para simular/exportar.</summary>
    Ready,

    /// <summary>Exportado (parâmetros/PLC gerados).</summary>
    Exported
}

/// <summary>
/// Aggregate root representando um controlador dentro de um projeto.
/// </summary>
/// <remarks>
/// Carrega a especificação de design (tipo + parâmetros em formato serializável)
/// sem depender diretamente do Engine. O Application layer faz a ponte entre
/// este aggregate e o <c>IControllerSpec</c> concreto do Engine.
/// </remarks>
public sealed class Controller : AggregateRoot
{
    private Controller()
    {
    }

    /// <summary>
    /// Tipo do controlador (Fuzzy, MPC, PID, ...).
    /// </summary>
    public ControllerType Type { get; private set; }

    /// <summary>
    /// Estado do ciclo de vida.
    /// </summary>
    public ControllerLifecycle Lifecycle { get; private set; }

    /// <summary>
    /// Setpoint principal (nome da variável alvo).
    /// </summary>
    public string Setpoint { get; private set; } = string.Empty;

    /// <summary>
    /// Variável manipulada (saída do controlador).
    /// </summary>
    public string Manipulated { get; private set; } = string.Empty;

    /// <summary>
    /// Variável controlada (medição).
    /// </summary>
    public string Controlled { get; private set; } = string.Empty;

    /// <summary>
    /// Período de varredura em segundos.
    /// </summary>
    public TimeSpan ScanTime { get; private set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Especificação serializada do controlador (JSON).
    /// O formato é definido pelo Engine via <c>IControllerSpec</c>; o Domain
    /// trata apenas como string opaca para manter isolamento de camadas.
    /// </summary>
    public string SerializedSpec { get; private set; } = string.Empty;

    /// <summary>
    /// Indica se o design offline foi computado e validado.
    /// </summary>
    public bool IsDesignValid { get; private set; }

    /// <summary>
    /// Resumo legível do design (parâmetros pré-computados).
    /// </summary>
    public string DesignSummary { get; private set; } = string.Empty;

    /// <summary>
    /// Cria um novo controlador do tipo especificado.
    /// </summary>
    /// <param name="name">Nome do controlador.</param>
    /// <param name="type">Tipo do controlador.</param>
    /// <returns>Resultado da criação.</returns>
    public static Result<Controller> Create(string name, ControllerType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Controller>.Failure(
                "O nome do controlador é obrigatório.");
        }

        var controller = new Controller
        {
            Type = type,
            Lifecycle = ControllerLifecycle.Draft
        };

        controller.Rename(name);

        return Result<Controller>.Success(controller);
    }

    /// <summary>
    /// Define as variáveis principais do controlador.
    /// </summary>
    public void SetVariables(string setpoint, string manipulated, string controlled)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(setpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(manipulated);
        ArgumentException.ThrowIfNullOrWhiteSpace(controlled);

        Setpoint = setpoint.Trim();
        Manipulated = manipulated.Trim();
        Controlled = controlled.Trim();

        Touch();
    }

    /// <summary>
    /// Define o período de varredura.
    /// </summary>
    public void SetScanTime(TimeSpan scanTime)
    {
        if (scanTime <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "O período de varredura deve ser positivo.",
                nameof(scanTime));
        }

        ScanTime = scanTime;

        Touch();
    }

    /// <summary>
    /// Armazena a especificação serializada do controlador.
    /// </summary>
    public void SetSerializedSpec(string serializedSpec)
    {
        SerializedSpec = serializedSpec ?? string.Empty;

        Touch();
    }

    /// <summary>
    /// Marca o design como computado e válido, armazenando o resumo.
    /// </summary>
    public void MarkDesignValid(string designSummary)
    {
        IsDesignValid = true;
        DesignSummary = designSummary ?? string.Empty;
        Lifecycle = ControllerLifecycle.Ready;

        Touch();
    }

    /// <summary>
    /// Marca o controlador como exportado.
    /// </summary>
    public void MarkExported()
    {
        Lifecycle = ControllerLifecycle.Exported;

        Touch();
    }

    /// <summary>
    /// Transiciona o ciclo de vida para "Designing".
    /// </summary>
    public void BeginDesign()
    {
        Lifecycle = ControllerLifecycle.Designing;

        Touch();
    }
}
