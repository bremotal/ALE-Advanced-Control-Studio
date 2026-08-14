using ALE.ACS.Engine.Core.Applications;
using ALE.ACS.Engine.Core.Validation;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class ValidationPipelineTests
{
    [Fact]
    public void Validate_ShouldReturnValid_WhenAllRulesPass()
    {
        var variable = new EngineeringVariable(Guid.NewGuid(), "Flow", "L/s", "Flow", 0, 100, true);
        var pipeline = new ValidationPipeline([new RequiredUnitRule()]);

        var result = pipeline.Validate(variable);

        result.IsValid.Should().BeTrue();
        result.ExecutionAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenRuleFails()
    {
        var variable = new EngineeringVariable(Guid.NewGuid(), "Flow", null, "Flow", 0, 100, true);
        var pipeline = new ValidationPipeline([new RequiredUnitRule()]);

        var result = pipeline.Validate(variable);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(m => m.Code == "VALIDATION_MISSING_UNIT");
    }

    private sealed class RequiredUnitRule : ValidationRuleBase
    {
        public override ValidationResult Validate(object target, ValidationContext context)
        {
            if (target is EngineeringVariable variable && string.IsNullOrWhiteSpace(variable.Unit))
            {
                return CreateFailure(
                    ValidationSeverity.Error,
                    ValidationCategory.Engineering,
                    "VALIDATION_MISSING_UNIT",
                    "The variable must declare a unit.");
            }

            return CreateSuccess();
        }
    }
}
