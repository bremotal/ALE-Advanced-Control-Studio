using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Tests;

public class GaussianMembershipFunctionTests
{
    [Fact]
    public void DeveRetornarUmNoCentro()
    {
        var mf = new GaussianMembershipFunction("Media", 50, 10);

        Assert.Equal(1.0, mf.GetMembership(50), 10);
    }

    [Fact]
    public void DeveSerSimetrica()
    {
        var mf = new GaussianMembershipFunction("Media", 50, 10);

        var esquerda = mf.GetMembership(40);
        var direita = mf.GetMembership(60);

        Assert.Equal(esquerda, direita, 10);
    }

    [Fact]
    public void DeveRetornarValorMenorQueUmForaDoCentro()
    {
        var mf = new GaussianMembershipFunction("Media", 50, 10);

        Assert.True(mf.GetMembership(40) < 1.0);
        Assert.True(mf.GetMembership(60) < 1.0);
    }

    [Fact]
    public void DeveAproximarZeroMuitoLongeDoCentro()
    {
        var mf = new GaussianMembershipFunction("Media", 50, 10);

        Assert.True(mf.GetMembership(-100) < 0.000001);
        Assert.True(mf.GetMembership(200) < 0.000001);
    }

    [Fact]
    public void DeveLancarExcecaoQuandoSigmaForMenorOuIgualAZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GaussianMembershipFunction("Erro", 50, 0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GaussianMembershipFunction("Erro", 50, -5));
    }
}