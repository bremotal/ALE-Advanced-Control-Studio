using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Registro que associa cada <see cref="EngineeringBlock"/> a um
/// <see cref="IController"/> concreto e mantém o <see cref="VariableStore"/>
/// compartilhado entre os blocos do pipeline.
/// </summary>
/// <remarks>
/// A associação block→controller é feita por id do block. O <see cref="VariableStore"/>
/// carrega os valores correntes das variáveis de processo (PV, MV, SP, DV) por nome,
/// permitindo que os controllers se comuniquem via variáveis nomeadas.
/// </remarks>
public sealed class ControllerExecutionHost
{
    private readonly Dictionary<Guid, IController> _controllersByBlockId = new();
    private readonly Dictionary<string, double> _variables = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Associa um controller a um block pelo id do block.</summary>
    public void Register(Guid blockId, IController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        _controllersByBlockId[blockId] = controller;
    }

    /// <summary>Remove a associação de um block.</summary>
    public bool Unregister(Guid blockId)
        => _controllersByBlockId.Remove(blockId);

    /// <summary>Tenta obter o controller associado a um block.</summary>
    public bool TryGetController(Guid blockId, out IController? controller)
        => _controllersByBlockId.TryGetValue(blockId, out controller);

    /// <summary>Define o valor de uma variável nomeada.</summary>
    public void SetVariable(string name, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _variables[name] = value;
    }

    /// <summary>Obtém o valor de uma variável nomeada (0.0 se não existir).</summary>
    public double GetVariable(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _variables.TryGetValue(name, out var value) ? value : 0.0;
    }

    /// <summary>Tenta obter o valor de uma variável nomeada.</summary>
    public bool TryGetVariable(string name, out double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _variables.TryGetValue(name, out value);
    }

    /// <summary>Enumera todas as variáveis correntes (cópia).</summary>
    public IReadOnlyDictionary<string, double> SnapshotVariables()
        => new Dictionary<string, double>(_variables, StringComparer.OrdinalIgnoreCase);

    /// <summary>Limpa todos os controllers e variáveis.</summary>
    public void Clear()
    {
        _controllersByBlockId.Clear();
        _variables.Clear();
    }
}

/// <summary>
/// Loja de variáveis de processo (atalho para o <see cref="ControllerExecutionHost"/>).
/// </summary>
public sealed class VariableStore
{
    private readonly ControllerExecutionHost _host;

    public VariableStore(ControllerExecutionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public double this[string name]
    {
        get => _host.GetVariable(name);
        set => _host.SetVariable(name, value);
    }

    public bool TryGetValue(string name, out double value)
        => _host.TryGetVariable(name, out value);

    public IReadOnlyDictionary<string, double> Snapshot()
        => _host.SnapshotVariables();
}
