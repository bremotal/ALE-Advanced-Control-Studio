using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.Inference;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;

namespace ALE.ACS.Application.Fuzzy;

/// <summary>
/// DTO containing the fuzzy controller configuration and current crisp inputs.
/// </summary>
public sealed class EvaluateFuzzyControllerCommand
{
    public required string Name { get; init; }

    public required List<FuzzyVariableDto> InputVariables { get; init; }

    public required FuzzyVariableDto OutputVariable { get; init; }

    public required List<FuzzyRuleDto> Rules { get; init; }

    public required Dictionary<string, double> Inputs { get; init; }

    public required InferenceEngineType EngineType { get; init; }

    public required SugenoOutputType? SugenoOutputType { get; init; }

    public double? ConstantValue { get; init; }

    public double? LinearA { get; init; }

    public double? LinearB { get; init; }

    public List<SugenoOutputPointDto>? PiecewisePoints { get; init; }

    public DefuzzificationMethod? DefuzzificationMethod { get; init; }
}

public sealed class FuzzyVariableDto
{
    public required string Name { get; init; }

    public required List<FuzzyTermDto> Terms { get; init; }
}

public sealed class FuzzyTermDto
{
    public required string Name { get; init; }

    public required MembershipFunctionDto MembershipFunction { get; init; }
}

public sealed class MembershipFunctionDto
{
    public required string Type { get; init; }

    public double A { get; init; }

    public double B { get; init; }

    public double C { get; init; }

    public double D { get; init; }
}

public sealed class FuzzyRuleDto
{
    public required List<FuzzyClauseDto> Antecedents { get; init; }

    public required FuzzyOperator Operator { get; init; }

    public required FuzzyClauseDto Consequent { get; init; }
}

public sealed class FuzzyClauseDto
{
    public required string VariableName { get; init; }

    public required string TermName { get; init; }
}

public sealed class SugenoOutputPointDto
{
    public double X { get; init; }

    public double Y { get; init; }
}
