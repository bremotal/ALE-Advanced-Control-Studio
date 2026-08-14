namespace ALE.ACS.Engine.Fuzzy.Performance;

/// <summary>
/// Provides allocation-free numeric evaluation helpers using spans.
/// </summary>
public static class AllocationFreeEvaluation
{
    /// <summary>
    /// Computes the minimum value in the provided span.
    /// </summary>
    /// <param name="values">The values to evaluate.</param>
    /// <returns>The minimum value.</returns>
    public static double Min(ReadOnlySpan<double> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", nameof(values));
        }

        var min = values[0];
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] < min)
            {
                min = values[i];
            }
        }

        return min;
    }
}
