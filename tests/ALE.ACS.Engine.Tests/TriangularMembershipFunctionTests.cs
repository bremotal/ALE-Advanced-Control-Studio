using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Tests;

public class TriangularMembershipFunctionTests
{
    [Fact]
    public void DeveRetornarZeroNoLimiteInferior()
    {
        var mf = new TriangularMembershipFunction("Alta", 0, 50, 100);

        Assert.Equal(0.0, mf.GetMembership(0));
    }

    [Fact]
    public void DeveRetornarUmNoPico()
    {
        var mf = new TriangularMembershipFunction("Alta", 0, 50, 100);

        Assert.Equal(1.0, mf.GetMembership(50));
    }

    [Fact]
    public void DeveRetornarZeroNoLimiteSuperior()
    {
        var mf = new TriangularMembershipFunction("Alta", 0, 50, 100);

        Assert.Equal(0.0, mf.GetMembership(100));
    }

    [Fact]
    public void DeveRetornarZeroForaDaFaixa()
    {
        var mf = new TriangularMembershipFunction("Alta", 0, 50, 100);

        Assert.Equal(0.0, mf.GetMembership(-10));
        Assert.Equal(0.0, mf.GetMembership(150));
    }

    [Fact]
    public void DeveRetornarMeioNaSubida()
    {
        var mf = new TriangularMembershipFunction("Alta", 0, 50, 100);

        Assert.Equal(0.5, mf.GetMembership(25), 6);
    }

    [Fact]
    public void DeveRetornarMeioNaDescida()
    {
        var mf = new TriangularMembershipFunction("Alta", 0, 50, 100);

        Assert.Equal(0.5, mf.GetMembership(75), 6);
    }
}