namespace ALE.ACS.Engine.Core;

public sealed class ControllerInput
{
    public ControllerInput()
    {
        Variables = new ControllerVariableCollection();
    }

    public ControllerInput(IEnumerable<ControllerVariable> variables)
    {
        Variables = new ControllerVariableCollection(variables);
    }

    public ControllerVariableCollection Variables { get; }

    public void AddVariable(ControllerVariable variable)
    {
        Variables.Add(variable);
    }

    public bool TryGetVariable(string name, out ControllerVariable? variable)
    {
        return Variables.TryGet(name, out variable);
    }
}