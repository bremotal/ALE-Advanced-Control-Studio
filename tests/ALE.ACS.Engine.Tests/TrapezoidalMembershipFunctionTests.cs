using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Tests;

public class TrapezoidalMembershipFunctionTests
{
    [Fact]
    public void DeveRetornarZeroNoLimiteInferior()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(0.0, mf.GetMembership(0));
    }

    [Fact]
    public void DeveRetornarUmNoInicioDoPlato()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(1.0, mf.GetMembership(25));
    }

    [Fact]
    public void DeveRetornarUmNoFinalDoPlato()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(1.0, mf.GetMembership(75));
    }

    [Fact]
    public void DeveRetornarZeroNoLimiteSuperior()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(0.0, mf.GetMembership(100));
    }

    [Fact]
    public void DeveRetornarZeroForaDaFaixa()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(0.0, mf.GetMembership(-10));
        Assert.Equal(0.0, mf.GetMembership(110));
    }

    [Fact]
    public void DeveRetornarMeioNaSubida()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(0.5, mf.GetMembership(12.5), 6);
    }

    [Fact]
    public void DeveRetornarMeioNaDescida()
    {
        var mf = new TrapezoidalMembershipFunction("Alta", 0, 25, 75, 100);

        Assert.Equal(0.5, mf.GetMembership(87.5), 6);
    }

    [Fact]
    public void DeveLancarExcecaoQuandoParametrosForemInvalidos()
    {
        Assert.Throws<ArgumentException>(() =>
            new TrapezoidalMembershipFunction("Erro", 10, 5, 20, 30));

        Assert.Throws<ArgumentException>(() =>
            new TrapezoidalMembershipFunction("Erro", 0, 20, 10, 30));

        Assert.Throws<ArgumentException>(() =>
            new TrapezoidalMembershipFunction("Erro", 0, 20, 30, 25));
    }
}