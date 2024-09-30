namespace Darp.Utils.ResxSourceGenerator;

using System.Collections.Immutable;

internal sealed class ImmutableDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<ImmutableDictionary<TKey, TValue>?>
    where TKey : notnull
{
    public static readonly ImmutableDictionaryEqualityComparer<TKey, TValue> Instance = new();

    public bool Equals(ImmutableDictionary<TKey, TValue>? x, ImmutableDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        if (!Equals(x.KeyComparer, y.KeyComparer))
            return false;

        if (!Equals(x.ValueComparer, y.ValueComparer))
            return false;

        if (x.Count != y.Count)
            return false;

        foreach (KeyValuePair<TKey, TValue> pair in x)
        {
            if (!y.TryGetValue(pair.Key, out TValue? other)
                || !x.ValueComparer.Equals(pair.Value, other))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableDictionary<TKey, TValue>? obj) => obj?.Count ?? 0;
}
