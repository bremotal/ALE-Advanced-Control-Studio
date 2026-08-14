using ALE.ACS.Engine.Fuzzy.Inference;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;
using Xunit;

namespace ALE.ACS.Engine.Tests.Fuzzy.Inference;

public sealed class SugenoInferenceEngineTests
{
    [Fact]
    public void Evaluate_Should_Return_Weighted_Average_For_Constant_Output()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(20.0, 30.0, 40.0));
        temperature.AddTerm(low);
        temperature.AddTerm(high);

        var rule1 = new FuzzyRule([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low));
        var rule2 = new FuzzyRule([new FuzzyClause(temperature, high)], FuzzyOperator.And, new FuzzyClause(temperature, high));

        var engine = new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.Constant, 10.0));

        var result = engine.Evaluate(new Dictionary<string, double> { ["Temperature"] = 15.0 }, [rule1, rule2]);

        Assert.Equal(10.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Weighted_Average_For_Linear_Output()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(20.0, 30.0, 40.0));
        temperature.AddTerm(low);
        temperature.AddTerm(high);

        var rule1 = new FuzzyRule([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low));
        var rule2 = new FuzzyRule([new FuzzyClause(temperature, high)], FuzzyOperator.And, new FuzzyClause(temperature, high));

        var engine = new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.Linear, 2.0, 3.0));

        var result = engine.Evaluate(new Dictionary<string, double> { ["Temperature"] = 15.0 }, [rule1, rule2]);

        Assert.Equal(33.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Interpolated_Value_For_PiecewiseLinear_Output()
    {
        var temperature = new FuzzyVariable("Temperature");
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(0.0, 10.0, 20.0));
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(20.0, 30.0, 40.0));
        temperature.AddTerm(low);
        temperature.AddTerm(high);

        var rule1 = new FuzzyRule([new FuzzyClause(temperature, low)], FuzzyOperator.And, new FuzzyClause(temperature, low));
        var rule2 = new FuzzyRule([new FuzzyClause(temperature, high)], FuzzyOperator.And, new FuzzyClause(temperature, high));

        var engine = new SugenoInferenceEngine(new SugenoOutputFunction(SugenoOutputType.PiecewiseLinear, [
            new SugenoOutputPoint(0.0, 0.0),
            new SugenoOutputPoint(10.0, 100.0)
        ]));

        var result = engine.Evaluate(new Dictionary<string, double> { ["Temperature"] = 15.0 }, [rule1, rule2]);

        Assert.Equal(150.0, result, 6);
    }
}
