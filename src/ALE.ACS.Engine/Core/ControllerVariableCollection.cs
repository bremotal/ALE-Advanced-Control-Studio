using System.Collections;
using System.Collections.Generic;

namespace ALE.ACS.Engine.Core;

public sealed class ControllerVariableCollection : IReadOnlyCollection<ControllerVariable>, IEnumerable<ControllerVariable>
{
    private readonly List<ControllerVariable> _variables;

    public ControllerVariableCollection()
    {
        _variables = [];
    }

    public ControllerVariableCollection(IEnumerable<ControllerVariable> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);
        _variables = [.. variables];
    }

    public int Count => _variables.Count;

    public IEnumerator<ControllerVariable> GetEnumerator()
    {
        return _variables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ControllerVariable variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        _variables.Add(variable);
    }

    public void AddRange(IEnumerable<ControllerVariable> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);
        _variables.AddRange(variables);
    }

    public bool TryGet(string name, out ControllerVariable? variable)
    {
        variable = _variables.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        return variable is not null;
    }

    public IReadOnlyList<ControllerVariable> AsReadOnlyList()
    {
        return _variables;
    }
}