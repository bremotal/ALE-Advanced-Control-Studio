using ALE.ACS.Application.Fuzzy;
using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.Inference;
using Xunit;

namespace ALE.ACS.Application.Tests.Fuzzy;

public sealed class EvaluateFuzzyControllerHandlerTests
{
    [Fact]
    public void Handle_Should_Return_Success_For_Simple_Sugeno_Controller()
    {
        var command = new EvaluateFuzzyControllerCommand
        {
            Name = "SimpleSugeno",
            InputVariables =
            [
                new FuzzyVariableDto
                {
                    Name = "Temperature",
                    Terms =
                    [
                        new FuzzyTermDto
                        {
                            Name = "Low",
                            MembershipFunction = new MembershipFunctionDto { Type = "Triangular", A = 0.0, B = 10.0, C = 20.0 }
                        }
                    ]
                }
            ],
            OutputVariable = new FuzzyVariableDto
            {
                Name = "Output",
                Terms = []
            },
            Rules =
            [
                new FuzzyRuleDto
                {
                    Antecedents = [new FuzzyClauseDto { VariableName = "Temperature", TermName = "Low" }],
                    Operator = ALE.ACS.Engine.Fuzzy.Rules.FuzzyOperator.And,
                    Consequent = new FuzzyClauseDto { VariableName = "Output", TermName = "Low" }
                }
            ],
            Inputs = new Dictionary<string, double> { ["Temperature"] = 15.0 },
            EngineType = InferenceEngineType.Sugeno,
            SugenoOutputType = SugenoOutputType.Constant,
            ConstantValue = 10.0,
            DefuzzificationMethod = DefuzzificationMethod.Centroid
        };

        var handler = new EvaluateFuzzyControllerHandler();
        var result = handler.Handle(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(10.0, result.Value, 6);
    }

    [Fact]
    public void Handle_Should_Return_Failure_When_Rules_Are_Empty()
    {
        var command = new EvaluateFuzzyControllerCommand
        {
            Name = "NoRules",
            InputVariables =
            [
                new FuzzyVariableDto
                {
                    Name = "Temperature",
                    Terms = []
                }
            ],
            OutputVariable = new FuzzyVariableDto { Name = "Output", Terms = [] },
            Rules = [],
            Inputs = new Dictionary<string, double> { ["Temperature"] = 15.0 },
            EngineType = InferenceEngineType.Sugeno,
            SugenoOutputType = SugenoOutputType.Constant,
            ConstantValue = 10.0,
            DefuzzificationMethod = DefuzzificationMethod.Centroid
        };

        var handler = new EvaluateFuzzyControllerHandler();
        var result = handler.Handle(command);

        Assert.False(result.IsSuccess);
        Assert.Contains("At least one fuzzy rule is required", result.Error);
    }
}
