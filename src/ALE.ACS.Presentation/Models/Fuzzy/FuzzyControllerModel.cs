using System.Collections.ObjectModel;

namespace ALE.ACS.Presentation.Models.Fuzzy;

public sealed class FuzzyControllerModel
{
    public FuzzyControllerModel(
        string name,
        string description,
        IEnumerable<string>? inputs = null,
        IEnumerable<string>? outputs = null,
        IEnumerable<string>? membershipFunctions = null,
        IEnumerable<string>? ruleBase = null)
    {
        Name = name;
        Description = description;
        Inputs = new ObservableCollection<string>(inputs ?? []);
        Outputs = new ObservableCollection<string>(outputs ?? []);
        MembershipFunctions = new ObservableCollection<string>(membershipFunctions ?? []);
        RuleBase = new ObservableCollection<string>(ruleBase ?? []);
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public ObservableCollection<string> Inputs { get; }

    public ObservableCollection<string> Outputs { get; }

    public ObservableCollection<string> MembershipFunctions { get; }

    public ObservableCollection<string> RuleBase { get; }
}
