namespace ALE.ACS.Domain.Common.Base;

/// <summary>
/// Representa um objeto de valor (Value Object),
/// cuja identidade é definida exclusivamente pelos seus valores.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Retorna os componentes utilizados para comparação.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                17,
                (current, item) => HashCode.Combine(current, item));
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}