namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Define o contrato para uma função de pertinência fuzzy.
/// </summary>
public interface IMembershipFunction
{
    /// <summary>
    /// Nome da função de pertinência.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Calcula o grau de pertinência (μ) para um valor de entrada.
    /// </summary>
    /// <param name="x">Valor de entrada.</param>
    /// <returns>Valor entre 0.0 e 1.0.</returns>
    double GetMembership(double x);
}