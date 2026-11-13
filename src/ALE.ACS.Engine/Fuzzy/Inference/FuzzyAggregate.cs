namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// Conjunto fuzzy agregado resultante da inferência Mamdani, pronto para defuzzificação.
/// </summary>
/// <remarks>
/// Corresponde à função μ_out(x) — a união (agregação) dos conjuntos consequentes
/// ativados pelas regras, após clipping/scaling pelo grau de ativação de cada regra.
/// </remarks>
public readonly struct FuzzyAggregate
{
    /// <summary>Pontos x do universo de discurso da variável de saída.</summary>
    public double[] Samples { get; }

    /// <summary>Grau de pertinência μ(x) em [0,1] em cada ponto de <see cref="Samples"/>.</summary>
    public double[] Memberships { get; }

    public FuzzyAggregate(double[] samples, double[] memberships)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentNullException.ThrowIfNull(memberships);
        if (samples.Length != memberships.Length)
            throw new ArgumentException("Samples e Memberships devem ter o mesmo tamanho.");

        Samples = samples;
        Memberships = memberships;
    }

    /// <summary>Número de pontos amostrados.</summary>
    public int Count => Samples.Length;

    /// <summary>True se o agregado não tem pontos (nenhuma regra disparou).</summary>
    public bool IsEmpty => Samples.Length == 0;

    /// <summary>Maior grau de pertinência observado (usado por MOM/SOM/LOM).</summary>
    public double MaxMembership => Memberships.Length == 0 ? 0 : Memberships.Max();

    /// <summary>Constrói um agregado vazio (sem regra disparou).</summary>
    public static FuzzyAggregate Empty { get; } = new(Array.Empty<double>(), Array.Empty<double>());
}
