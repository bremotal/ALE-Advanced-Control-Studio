namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Classe base para todas as funções de pertinência.
/// Centraliza propriedades e validações comuns.
/// </summary>
public abstract class MembershipFunctionBase : IMembershipFunction
{
    protected MembershipFunctionBase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da função de pertinência não pode ser vazio.", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Nome da função de pertinência.
    /// Ex.: Baixa, Média, Alta.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Retorna o grau de pertinência para um determinado valor.
    /// </summary>
    /// <param name="x">Valor de entrada.</param>
    /// <returns>Valor entre 0 e 1.</returns>
    public abstract double GetMembership(double x);

    /// <summary>
    /// Garante que um valor esteja dentro do intervalo [0,1].
    /// Evita erros numéricos por arredondamento.
    /// </summary>
    protected static double Clamp01(double value)
    {
        if (value < 0.0)
            return 0.0;

        if (value > 1.0)
            return 1.0;

        return value;
    }
}