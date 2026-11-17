using ALE.ACS.Engine.Fuzzy;
using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.Inference;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;
using Xunit;

namespace ALE.ACS.Engine.Tests.Fuzzy;

public sealed class FuzzyControllerTests
{
    [Fact]
    public void Evaluate_Should_Return_Crisp_Result_For_Sugeno_Controller()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(20.0, 30.0, 40.0));
        temperature.AddTerm(low);
        temperature.AddTerm(high);

        var output = new FuzzyVariable("Output");
        var rules = new List<FuzzyRule>
        {
            new([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low)),
            new([new FuzzyClause(temperature, high)], FuzzyOperator.And, new FuzzyClause(temperature, high))
        };

        var controller = new FuzzyController(
            "TemperatureToOutput",
            [temperature],
            output,
            rules,
            new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.Constant, 10.0)));

        var result = controller.Evaluate(new Dictionary<string, double> { ["Temperature"] = 15.0 });

        Assert.Equal(10.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Crisp_Result_For_Mamdani_Controller()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(20.0, 30.0, 40.0));
        temperature.AddTerm(low);
        temperature.AddTerm(high);

        var output = new FuzzyVariable("Output");
        var rules = new List<FuzzyRule>
        {
            new([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low)),
            new([new FuzzyClause(temperature, high)], FuzzyOperator.And, new FuzzyClause(temperature, high))
        };

        var controller = new FuzzyController(
            "TemperatureToOutput",
            [temperature],
            output,
            rules,
            new MamdaniInferenceEngine(),
            new CentroidDefuzzifier());

        var result = controller.Evaluate(new Dictionary<string, double> { ["Temperature"] = 15.0 });

        // Antes do fix o resultado era 0.0 por BUG: o defuzzifier recebia array
        // vazio e o range fixo [0,100] ignorava o suporte real dos termos.
        // Agora o Mamdani calcula: Temperature=15 ativa só a regra "Low" (MF
        // triangular 0-10-20, μ(15)=0.5). O CentroidDefuzzifier faz a discretização
        // sobre o range real [0,20] (suporte do termo Low) e retorna o centróide
        // do conjunto triangular simétrico — exatamente 10.0.
        Assert.Equal(10.0, result, 6);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mamdani_Uses_No_Defuzzifier()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        temperature.AddTerm(low);

        var output = new FuzzyVariable("Output");
        var rules = new List<FuzzyRule>
        {
            new([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low))
        };

        Assert.Throws<ArgumentException>(() => new FuzzyController("X", [temperature], output, rules, new MamdaniInferenceEngine()));
    }

    [Fact]
    public void Evaluate_Should_Throw_When_Inputs_Are_Missing()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        temperature.AddTerm(low);

        var output = new FuzzyVariable("Output");
        var rules = new List<FuzzyRule>
        {
            new([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low))
        };

        var controller = new FuzzyController("X", [temperature], output, rules, new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.Constant, 10.0)));

        Assert.Throws<ArgumentException>(() => controller.Evaluate(new Dictionary<string, double>()));
    }
}
