using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;

namespace ALE.ACS.Engine.Fuzzy.Diagnostics;

/// <summary>
/// Validates a fuzzy rule base and variable set for common consistency issues.
/// </summary>
public sealed class ConsistencyChecker
{
    /// <summary>
    /// Validates the supplied rules and variables.
    /// </summary>
    /// <param name="rules">The rules to validate.</param>
    /// <param name="variables">The variables to validate.</param>
    /// <returns>A validation report containing any diagnostics.</returns>
    public ValidationReport Validate(IEnumerable<FuzzyRule> rules, IEnumerable<FuzzyVariable> variables)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(variables);

        var report = new ValidationReport();
        var variableList = variables.ToList();
        var ruleList = rules.ToList();

        foreach (var variable in variableList)
        {
            if (variable.Terms.Count == 0)
            {
                report.Add(new ValidationResult(DiagnosticCode.MissingVariable, DiagnosticSeverity.Warning, $"Variable '{variable.Name}' is not fully defined."));
                report.Add(new ValidationResult(DiagnosticCode.MissingTerm, DiagnosticSeverity.Warning, $"Variable '{variable.Name}' has no terms."));
            }
        }

        foreach (var rule in ruleList)
        {
            foreach (var antecedent in rule.Antecedents)
            {
                if (!variableList.Any(v => string.Equals(v.Name, antecedent.Variable.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    report.Add(new ValidationResult(DiagnosticCode.MissingVariable, DiagnosticSeverity.Error, $"Variable '{antecedent.Variable.Name}' is not defined."));
                }

                if (!antecedent.Variable.Terms.Contains(antecedent.Term))
                {
                    report.Add(new ValidationResult(DiagnosticCode.MissingTerm, DiagnosticSeverity.Error, $"Term '{antecedent.Term.Name}' is not defined for variable '{antecedent.Variable.Name}'."));
                }
            }

            if (!variableList.Any(v => string.Equals(v.Name, rule.Consequent.Variable.Name, StringComparison.OrdinalIgnoreCase)))
            {
                report.Add(new ValidationResult(DiagnosticCode.MissingVariable, DiagnosticSeverity.Error, $"Variable '{rule.Consequent.Variable.Name}' is not defined."));
            }

            if (!rule.Consequent.Variable.Terms.Contains(rule.Consequent.Term))
            {
                report.Add(new ValidationResult(DiagnosticCode.MissingTerm, DiagnosticSeverity.Error, $"Term '{rule.Consequent.Term.Name}' is not defined for variable '{rule.Consequent.Variable.Name}'."));
            }
        }

        return report;
    }
}
