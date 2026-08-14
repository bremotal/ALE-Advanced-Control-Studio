using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;
using Xunit;

namespace ALE.ACS.Engine.Tests.Fuzzy.Rules;

public sealed class FuzzyRuleTests
{
    [Fact]
    public void Constructor_Should_Create_Valid_Rule_With_And_Operator()
    {
        var temperature = new FuzzyVariable("Temperature");
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(70.0, 80.0, 90.0));
        var low = new FuzzyTerm("Low", new TriangularMembershipFunction(10.0, 20.0, 30.0));
        temperature.AddTerm(high);
        temperature.AddTerm(low);

        var clause1 = new FuzzyClause(temperature, high);
        var clause2 = new FuzzyClause(temperature, low);

        var rule = new FuzzyRule([clause1, clause2], FuzzyOperator.And, new FuzzyClause(temperature, high));

        Assert.Equal(FuzzyOperator.And, rule.Operator);
        Assert.Equal(2, rule.Antecedents.Count);
        Assert.NotNull(rule.Consequent);
    }

    [Fact]
    public void Constructor_Should_Create_Valid_Rule_With_Or_Operator()
    {
        var temperature = new FuzzyVariable("Temperature");
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(70.0, 80.0, 90.0));
        var medium = new FuzzyTerm("Medium", new TriangularMembershipFunction(40.0, 50.0, 60.0));
        temperature.AddTerm(high);
        temperature.AddTerm(medium);

        var antecedent = new FuzzyClause(temperature, high);
        var consequent = new FuzzyClause(temperature, medium);

        var rule = new FuzzyRule([antecedent], FuzzyOperator.Or, consequent);

        Assert.Equal(FuzzyOperator.Or, rule.Operator);
        Assert.Single(rule.Antecedents);
        Assert.Same(consequent, rule.Consequent);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Antecedents_Are_Empty()
    {
        var temperature = new FuzzyVariable("Temperature");
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(70.0, 80.0, 90.0));
        temperature.AddTerm(high);

        Assert.Throws<ArgumentException>(() => new FuzzyRule([], FuzzyOperator.And, new FuzzyClause(temperature, high)));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Consequent_Is_Null()
    {
        var temperature = new FuzzyVariable("Temperature");
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(70.0, 80.0, 90.0));
        temperature.AddTerm(high);

        Assert.Throws<ArgumentNullException>(() => new FuzzyRule([new FuzzyClause(temperature, high)], FuzzyOperator.And, null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Term_Does_Not_Belong_To_Variable()
    {
        var temperature = new FuzzyVariable("Temperature");
        var pressure = new FuzzyVariable("Pressure");
        var highTemperature = new FuzzyTerm("High", new TriangularMembershipFunction(70.0, 80.0, 90.0));
        var highPressure = new FuzzyTerm("High", new TriangularMembershipFunction(10.0, 20.0, 30.0));
        temperature.AddTerm(highTemperature);
        pressure.AddTerm(highPressure);

        Assert.Throws<ArgumentException>(() => new FuzzyClause(temperature, highPressure));
    }
}
