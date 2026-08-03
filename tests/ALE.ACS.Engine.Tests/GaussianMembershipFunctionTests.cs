using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Tests.Fuzzy.MembershipFunctions;

public class GaussianMembershipFunctionTests
{
    private const double Tolerance = 1e-6;

    private static GaussianMembershipFunction CreateDefault()
    {
        return new("Gaussian", 0.0, 1.0);
    }

    #region Construction

    [Fact]
    public void Should_Create_Valid_Gaussian_Membership_Function()
    {
        var mf = CreateDefault();

        Assert.NotNull(mf);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_Invalid(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new GaussianMembershipFunction(name, 0.0, 1.0));
    }

    [Fact]
    public void Should_Throw_When_Name_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GaussianMembershipFunction(null!, 0.0, 1.0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_Throw_When_Sigma_Is_Not_Positive(double sigma)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GaussianMembershipFunction(
                "Gaussian",
                0.0,
                sigma));
    }

    #endregion

    #region Properties

    [Fact]
    public void Should_Expose_Name()
    {
        var mf = CreateDefault();

        Assert.Equal("Gaussian", mf.Name);
    }

    [Fact]
    public void Should_Expose_Mean()
    {
        var mf = CreateDefault();

        Assert.Equal(0.0, mf.Mean);
    }

    [Fact]
    public void Should_Expose_Sigma()
    {
        var mf = CreateDefault();

        Assert.Equal(1.0, mf.Sigma);
    }

    #endregion

    #region Evaluation

    [Fact]
    public void Should_Return_One_At_Mean()
    {
        var mf = CreateDefault();

        Assert.Equal(1.0, mf.Evaluate(0.0), 6);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.0)]
    public void Should_Be_Symmetric_Around_Mean(double delta)
    {
        var mf = CreateDefault();

        var left = mf.Evaluate(-delta);
        var right = mf.Evaluate(delta);

        Assert.Equal(left, right, 6);
    }

    [Theory]
    [InlineData(5.0)]
    [InlineData(10.0)]
    [InlineData(20.0)]
    public void Should_Approach_Zero_Far_From_Mean(double x)
    {
        var mf = CreateDefault();

        Assert.True(mf.Evaluate(x) < 1e-5);
    }

    #endregion

    #region Boundary Conditions

    [Theory]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void Should_Return_Zero_For_Extreme_Values(double input)
    {
        var mf = CreateDefault();

        Assert.Equal(
            0.0,
            mf.Evaluate(input),
            Tolerance);
    }

    #endregion
}