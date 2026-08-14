using ALE.ACS.Engine.Fuzzy;
using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.Inference;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;

namespace ALE.ACS.Application.Fuzzy;

/// <summary>
/// Application handler that maps fuzzy DTOs into engine objects and evaluates the controller.
/// </summary>
public sealed class EvaluateFuzzyControllerHandler
{
    public Result<double> Handle(EvaluateFuzzyControllerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var inputVariables = command.InputVariables.Select(MapVariable).ToList();
            var outputVariable = MapVariable(command.OutputVariable);
            var rules = command.Rules.Select(rule => MapRule(rule, inputVariables, outputVariable)).ToList();

            if (rules.Count == 0)
            {
                return Result<double>.Failure("At least one fuzzy rule is required.");
            }

            IFuzzyInferenceEngine inferenceEngine = command.EngineType switch
            {
                InferenceEngineType.Sugeno => CreateSugenoEngine(command),
                InferenceEngineType.Mamdani => CreateMamdaniEngine(command),
                _ => throw new ArgumentOutOfRangeException(nameof(command.EngineType))
            };

            var controller = new FuzzyController(
                command.Name,
                inputVariables,
                outputVariable,
                rules,
                inferenceEngine,
                command.EngineType == InferenceEngineType.Mamdani ? CreateDefuzzifier(command) : null);

            var result = controller.Evaluate(command.Inputs);
            return Result<double>.Success(result);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result<double>.Failure(ex.Message);
        }
    }

    private static FuzzyVariable MapVariable(FuzzyVariableDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var variable = new FuzzyVariable(dto.Name);

        foreach (var termDto in dto.Terms)
        {
            var term = new FuzzyTerm(termDto.Name, MapMembershipFunction(termDto.MembershipFunction));
            variable.AddTerm(term);
        }

        return variable;
    }

    private static IMembershipFunction MapMembershipFunction(MembershipFunctionDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.Type switch
        {
            "Triangular" => new TriangularMembershipFunction(dto.A, dto.B, dto.C),
            "Trapezoidal" => new TrapezoidalMembershipFunction(dto.A, dto.B, dto.C, dto.D),
            _ => throw new ArgumentOutOfRangeException(nameof(dto.Type))
        };
    }

    private static FuzzyRule MapRule(FuzzyRuleDto dto, IReadOnlyList<FuzzyVariable> inputVariables, FuzzyVariable outputVariable)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var antecedents = dto.Antecedents.Select(antecedent => MapClause(antecedent, inputVariables)).ToList();
        var consequent = MapClause(dto.Consequent, outputVariable);
        return new FuzzyRule(antecedents, dto.Operator, consequent);
    }

    private static FuzzyClause MapClause(FuzzyClauseDto dto, IReadOnlyList<FuzzyVariable> variables)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var variable = variables.FirstOrDefault(v => string.Equals(v.Name, dto.VariableName, StringComparison.OrdinalIgnoreCase));
        if (variable is null)
        {
            throw new ArgumentException($"Variable '{dto.VariableName}' was not found.", nameof(dto));
        }

        var term = variable.Terms.FirstOrDefault(t => string.Equals(t.Name, dto.TermName, StringComparison.OrdinalIgnoreCase));
        if (term is null)
        {
            throw new ArgumentException($"Term '{dto.TermName}' was not found on variable '{dto.VariableName}'.", nameof(dto));
        }

        return new FuzzyClause(variable, term);
    }

    private static FuzzyClause MapClause(FuzzyClauseDto dto, FuzzyVariable outputVariable)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(outputVariable);

        var term = outputVariable.Terms.FirstOrDefault(t => string.Equals(t.Name, dto.TermName, StringComparison.OrdinalIgnoreCase));
        if (term is not null)
        {
            return new FuzzyClause(outputVariable, term);
        }

        var placeholderTerm = new FuzzyTerm(dto.TermName, new TriangularMembershipFunction(0.0, 1.0, 2.0));
        outputVariable.AddTerm(placeholderTerm);
        return new FuzzyClause(outputVariable, placeholderTerm);
    }

    private static IFuzzyInferenceEngine CreateSugenoEngine(EvaluateFuzzyControllerCommand command)
    {
        if (command.SugenoOutputType is null)
        {
            throw new ArgumentException("Sugeno output type must be provided.", nameof(command));
        }

        return command.SugenoOutputType switch
        {
            SugenoOutputType.Constant => new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.Constant, command.ConstantValue ?? 0.0)),
            SugenoOutputType.Linear => new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.Linear, command.LinearA ?? 0.0, command.LinearB ?? 0.0)),
            SugenoOutputType.PiecewiseLinear => new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.PiecewiseLinear, command.PiecewisePoints?.Select(p => new SugenoOutputPoint(p.X, p.Y)) ?? [])),
            _ => throw new ArgumentOutOfRangeException(nameof(command.SugenoOutputType))
        };
    }

    private static IFuzzyInferenceEngine CreateMamdaniEngine(EvaluateFuzzyControllerCommand command)
    {
        return new MamdaniInferenceEngine();
    }

    private static IDefuzzifier CreateDefuzzifier(EvaluateFuzzyControllerCommand command)
    {
        return command.DefuzzificationMethod switch
        {
            DefuzzificationMethod.Centroid => new CentroidDefuzzifier(),
            DefuzzificationMethod.MeanOfMaximum => new MeanOfMaximumDefuzzifier(),
            _ => new CentroidDefuzzifier()
        };
    }
}

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public string? Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string error) => new(false, default, error);
}
