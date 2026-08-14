using ALE.ACS.Engine.Fuzzy.Expressions;
using ALE.ACS.Engine.Fuzzy.Rules;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Expressions;

public sealed class FuzzyRuleExpressionTests
{
    [Fact]
    public void Parser_Should_Parse_Parenthesized_Expressions()
    {
        var parser = new FuzzyRuleParser();

        var expression = parser.Parse("Temperature IS High AND (Pressure IS Low OR NOT Humidity IS Medium)");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<OperatorNode>();

        var root = (OperatorNode)expression;
        root.Operator.Should().Be("AND");
        root.Children.Should().HaveCount(2);
        root.Children[1].Should().BeOfType<OperatorNode>();

        var nested = (OperatorNode)root.Children[1];
        nested.Operator.Should().Be("OR");
        nested.Children.Should().HaveCount(2);
    }

    [Fact]
    public void Compiler_Should_Evaluate_Logical_Expressions_Using_Clause_Degrees()
    {
        var parser = new FuzzyRuleParser();
        var compiler = new FuzzyRuleCompiler();

        var expression = parser.Parse("Temperature IS High AND (Pressure IS Low OR NOT Humidity IS Medium)");
        var compiled = compiler.Compile(expression);

        var inputs = new Dictionary<string, double>
        {
            ["Temperature:High"] = 0.8,
            ["Pressure:Low"] = 0.2,
            ["Humidity:Medium"] = 0.6
        };

        var result = compiled.Evaluate(inputs);

        result.Should().BeApproximately(0.2, 1e-9);
    }

    [Fact]
    public void Parser_Should_Throw_ValidationException_For_Invalid_Syntax()
    {
        var parser = new FuzzyRuleParser();

        Action act = () => parser.Parse("Temperature IS High AND");

        act.Should().Throw<ValidationException>();
    }
}
