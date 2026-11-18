using ALE.ACS.Engine.Core.Execution;

namespace ALE.ACS.Engine.Core.Runtime;

/// <summary>
/// Estado de uma sessão de runtime.
/// </summary>
public enum RuntimeState
{
    /// <summary>Sessão criada mas não iniciada.</summary>
    Created,

    /// <summary>Runtime em execução (scan loop ativo).</summary>
    Running,

    /// <summary>Runtime parado.</summary>
    Stopped,

    /// <summary>Runtime abortado por erro.</summary>
    Faulted
}

/// <summary>
/// Configuração de uma sessão de runtime.
/// </summary>
public sealed class RuntimeConfiguration
{
    public RuntimeConfiguration(TimeSpan scanTime, int maxScans, bool autoStart = false)
    {
        if (scanTime <= TimeSpan.Zero)
        {
            throw new ArgumentException("ScanTime deve ser positivo.", nameof(scanTime));
        }

        ScanTime = scanTime;
        MaxScans = maxScans;
        AutoStart = autoStart;
    }

    /// <summary>Período de varredura (tempo entre scans).</summary>
    public TimeSpan ScanTime { get; }

    /// <summary>Número máximo de scans (0 = ilimitado).</summary>
    public int MaxScans { get; }

    /// <summary>Indica se a sessão inicia automaticamente após Configure.</summary>
    public bool AutoStart { get; }
}

/// <summary>
/// Estatísticas acumuladas de uma sessão de runtime.
/// </summary>
public sealed class RuntimeStatistics
{
    public long ScanCount { get; set; }
    public int ComponentCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan LastScanDuration { get; set; }
    public double AverageScanDurationMs { get; set; }
}

/// <summary>
/// Componente que participa do scan loop de runtime (ex.: um controlador).
/// </summary>
public interface IRuntimeComponent
{
    /// <summary>Nome do componente.</summary>
    string Name { get; }

    /// <summary>Chamado uma vez no início da sessão.</summary>
    void Initialize(ControllerExecutionHost host);

    /// <summary>Chamado a cada scan.</summary>
    void Execute(ControllerExecutionHost host);

    /// <summary>Chamado ao final da sessão.</summary>
    void Shutdown();
}

/// <summary>
/// Exceção lançada por falhas no runtime.
/// </summary>
public sealed class RuntimeException : Exception
{
    public RuntimeException(string message) : base(message) { }
    public RuntimeException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Scheduler que executa os componentes em ordem de registro a cada scan.
/// </summary>
public sealed class RuntimeScheduler
{
    private readonly List<IRuntimeComponent> _components = [];

    public void Add(IRuntimeComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        _components.Add(component);
    }

    public IReadOnlyList<IRuntimeComponent> Components => _components;

    public void InitializeAll(ControllerExecutionHost host)
    {
        foreach (var c in _components)
        {
            c.Initialize(host);
        }
    }

    public void ExecuteScan(ControllerExecutionHost host)
    {
        foreach (var c in _components)
        {
            c.Execute(host);
        }
    }

    public void ShutdownAll()
    {
        foreach (var c in _components)
        {
            c.Shutdown();
        }
    }
}

/// <summary>
/// Sessão de runtime — agrupa configuração, scheduler e estado.
/// </summary>
public sealed class RuntimeSession
{
    public RuntimeSession(RuntimeConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Scheduler = new RuntimeScheduler();
        Host = new ControllerExecutionHost();
        State = RuntimeState.Created;
    }

    public RuntimeConfiguration Configuration { get; }
    public RuntimeScheduler Scheduler { get; }
    public ControllerExecutionHost Host { get; }
    public RuntimeState State { get; internal set; }
    public RuntimeStatistics? Statistics { get; internal set; }
}
