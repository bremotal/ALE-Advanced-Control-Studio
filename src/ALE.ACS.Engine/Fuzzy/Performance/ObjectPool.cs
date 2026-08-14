using System.Buffers;

namespace ALE.ACS.Engine.Fuzzy.Performance;

/// <summary>
/// Generic object pool implementation backed by <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The pooled object type.</typeparam>
public sealed class ObjectPool<T> where T : class
{
    private readonly Func<T> _factory;
    private readonly T?[] _items;
    private readonly object _sync = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
    /// </summary>
    /// <param name="factory">Factory used when the pool is empty.</param>
    /// <param name="capacity">Maximum number of retained objects.</param>
    public ObjectPool(Func<T> factory, int capacity = 32)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _factory = factory;
        _items = ArrayPool<T?>.Shared.Rent(capacity);
    }

    /// <summary>
    /// Rents an object from the pool.
    /// </summary>
    /// <returns>The rented object.</returns>
    public T Rent()
    {
        lock (_sync)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                if (item is null)
                {
                    continue;
                }

                _items[i] = null;
                return item;
            }
        }

        return _factory();
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="item">The object to return.</param>
    /// <param name="reset">Optional reset action executed before storing the object.</param>
    public void Return(T item, Action<T>? reset = null)
    {
        ArgumentNullException.ThrowIfNull(item);

        reset?.Invoke(item);

        lock (_sync)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] is not null)
                {
                    continue;
                }

                _items[i] = item;
                return;
            }
        }
    }
}
