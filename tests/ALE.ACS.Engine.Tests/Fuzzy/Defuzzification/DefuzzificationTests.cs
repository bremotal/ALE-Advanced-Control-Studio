using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using Xunit;

namespace ALE.ACS.Engine.Tests.Fuzzy.Defuzzification;

public sealed class DefuzzificationTests
{
    [Fact]
    public void Centroid_Should_Return_Center_Of_A_Single_Triangular_Function()
    {
        var triangular = new TriangularMembershipFunction(0.0, 10.0, 20.0);
        var defuzzifier = new CentroidDefuzzifier();

        var result = defuzzifier.Defuzzify([(triangular, 1.0)], 0.0, 20.0);

        Assert.Equal(10.0, result, 2);
    }

    [Fact]
    public void Centroid_Should_Return_Weighted_Center_For_Overlapping_Functions()
    {
        var left = new TriangularMembershipFunction(0.0, 5.0, 10.0);
        var right = new TriangularMembershipFunction(5.0, 10.0, 15.0);
        var defuzzifier = new CentroidDefuzzifier();

        var result = defuzzifier.Defuzzify([(left, 1.0), (right, 1.0)], 0.0, 15.0);

        Assert.Equal(7.5, result, 2);
    }

    [Fact]
    public void MeanOfMaximum_Should_Return_The_Maximum_Point_For_A_Trapezoidal_Function()
    {
        var trapezoid = new TrapezoidalMembershipFunction(0.0, 5.0, 10.0, 15.0);
        var defuzzifier = new MeanOfMaximumDefuzzifier();

        var result = defuzzifier.Defuzzify([(trapezoid, 1.0)], 0.0, 15.0);

        Assert.Equal(7.5, result, 2);
    }
}
