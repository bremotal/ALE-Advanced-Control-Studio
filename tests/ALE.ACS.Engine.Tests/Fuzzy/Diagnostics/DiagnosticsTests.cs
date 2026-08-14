using ALE.ACS.Engine.Fuzzy.Diagnostics;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Diagnostics;

public sealed class DiagnosticsTests
{
    [Fact]
    public void ValidationResult_Should_Store_Diagnostic_Details()
    {
        var result = new ValidationResult(DiagnosticCode.MissingVariable, DiagnosticSeverity.Error, "Variable missing");

        result.Code.Should().Be(DiagnosticCode.MissingVariable);
        result.Severity.Should().Be(DiagnosticSeverity.Error);
        result.Message.Should().Be("Variable missing");
    }

    [Fact]
    public void ConsistencyChecker_Should_Report_MissingVariable_And_MissingTerm()
    {
        var checker = new ConsistencyChecker();
        var variable = new FuzzyVariable("Temperature");
        var missingTerm = new FuzzyTerm("High", new TriangularMembershipFunction(0.0, 1.0, 2.0));
        variable.AddTerm(missingTerm);

        var missingVariable = new FuzzyVariable("Pressure");
        var rule = new FuzzyRule(
            [new FuzzyClause(variable, missingTerm)],
            FuzzyOperator.And,
            new FuzzyClause(variable, missingTerm));

        var report = checker.Validate([rule], [variable, missingVariable]);

        report.Results.Should().Contain(r => r.Code == DiagnosticCode.MissingTerm);
        report.Results.Should().Contain(r => r.Code == DiagnosticCode.MissingVariable);
    }

    [Fact]
    public void ConsistencyChecker_Should_Produce_Empty_Report_When_Valid()
    {
        var checker = new ConsistencyChecker();
        var variable = new FuzzyVariable("Temperature");
        var high = new FuzzyTerm("High", new TriangularMembershipFunction(0.0, 1.0, 2.0));
        variable.AddTerm(high);

        var rule = new FuzzyRule(
            [new FuzzyClause(variable, high)],
            FuzzyOperator.And,
            new FuzzyClause(variable, high));

        var report = checker.Validate([rule], [variable]);

        report.Results.Should().BeEmpty();
    }
}
