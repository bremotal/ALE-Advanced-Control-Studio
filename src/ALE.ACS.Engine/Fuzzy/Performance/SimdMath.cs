using System.Numerics;

namespace ALE.ACS.Engine.Fuzzy.Performance;

/// <summary>
/// SIMD-enabled math operations for batch fuzzy calculations.
/// </summary>
public static class SimdMath
{
    /// <summary>
    /// Gets a value indicating whether SIMD hardware acceleration is available.
    /// </summary>
    public static bool IsHardwareAccelerated => Vector.IsHardwareAccelerated;

    /// <summary>
    /// Performs element-wise multiplication for two input vectors.
    /// </summary>
    /// <param name="left">The left operand values.</param>
    /// <param name="right">The right operand values.</param>
    /// <param name="destination">The destination span receiving the multiplication result.</param>
    public static void Multiply(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        if (left.Length != right.Length || destination.Length != left.Length)
        {
            throw new ArgumentException("All spans must have the same length.");
        }

        var width = Vector<double>.Count;
        var i = 0;

        if (IsHardwareAccelerated)
        {
            for (; i <= left.Length - width; i += width)
            {
                var leftVector = new Vector<double>(left.Slice(i, width));
                var rightVector = new Vector<double>(right.Slice(i, width));
                (leftVector * rightVector).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < left.Length; i++)
        {
            destination[i] = left[i] * right[i];
        }
    }
}
