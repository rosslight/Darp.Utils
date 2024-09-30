namespace Darp.Utils.ResxSourceGenerator.Tests;

using System;
using System.Collections.Immutable;
using Darp.Utils.ResxSourceGenerator;
using FluentAssertions;
using Xunit;

public class ImmutableDictionaryEqualityComparerTests
{
    private static readonly ImmutableDictionaryEqualityComparer<string, int> Comparer = ImmutableDictionaryEqualityComparer<string, int>.Instance;

    [Fact]
    public void Equals_BothNull_ReturnsTrue() => Comparer.Equals(null, null).Should().BeTrue();

    [Fact]
    public void Equals_XNull_YNotNull_ReturnsFalse()
    {
        ImmutableDictionary<string, int>? y = ImmutableDictionary<string, int>.Empty;

        Comparer.Equals(null, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_XNotNull_YNull_ReturnsFalse()
    {
        ImmutableDictionary<string, int>? x = ImmutableDictionary<string, int>.Empty;

        Comparer.Equals(x, null).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty;

        Comparer.Equals(x, x).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameContent_ReturnsTrue()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty.Add("key1", 1);
        ImmutableDictionary<string, int> y = ImmutableDictionary<string, int>.Empty.Add("key1", 1);

        Comparer.Equals(x, y).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty.Add("key1", 1);
        ImmutableDictionary<string, int> y = ImmutableDictionary<string, int>.Empty.Add("key2", 1);

        Comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty.Add("key1", 1);
        ImmutableDictionary<string, int> y = ImmutableDictionary<string, int>.Empty.Add("key1", 2);

        Comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentKeyComparers_ReturnsFalse()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary.Create<string, int>(StringComparer.OrdinalIgnoreCase).Add("key1", 1);
        ImmutableDictionary<string, int> y = ImmutableDictionary.Create<string, int>(StringComparer.Ordinal).Add("key1", 1);

        Comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameKeyComparer_DifferentCasing_ReturnsTrue()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary.Create<string, int>(StringComparer.OrdinalIgnoreCase).Add("key1", 1);
        ImmutableDictionary<string, int> y = ImmutableDictionary.Create<string, int>(StringComparer.OrdinalIgnoreCase).Add("KEY1", 1);

        Comparer.Equals(x, y).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValueComparers_ReturnsFalse()
    {
        ImmutableDictionaryEqualityComparer<string, string> comparer = ImmutableDictionaryEqualityComparer<string, string>.Instance;
        ImmutableDictionary<string, string> x = ImmutableDictionary
            .Create<string, string>(EqualityComparer<string>.Default, StringComparer.Ordinal)
            .Add("key1", "value");
        ImmutableDictionary<string, string> y = ImmutableDictionary
            .Create<string, string>(EqualityComparer<string>.Default, StringComparer.OrdinalIgnoreCase)
            .Add("key1", "value");

        comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameValueComparer_DifferentValues_ReturnsFalse()
    {
        ImmutableDictionaryEqualityComparer<string, string> comparer = ImmutableDictionaryEqualityComparer<string, string>.Instance;
        ImmutableDictionary<string, string> x = ImmutableDictionary
            .Create<string, string>(EqualityComparer<string>.Default, StringComparer.Ordinal)
            .Add("key1", "value1");
        ImmutableDictionary<string, string> y = ImmutableDictionary
            .Create<string, string>(EqualityComparer<string>.Default, StringComparer.Ordinal)
            .Add("key1", "value2");

        comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameValueComparer_DifferentCasing_ReturnsTrue()
    {
        ImmutableDictionaryEqualityComparer<string, string> comparer = ImmutableDictionaryEqualityComparer<string, string>.Instance;
        ImmutableDictionary<string, string> x = ImmutableDictionary
            .Create<string, string>(EqualityComparer<string>.Default, StringComparer.OrdinalIgnoreCase)
            .Add("key1", "value");
        ImmutableDictionary<string, string> y = ImmutableDictionary
            .Create<string, string>(EqualityComparer<string>.Default, StringComparer.OrdinalIgnoreCase)
            .Add("key1", "VALUE");

        comparer.Equals(x, y).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Null_ReturnsZero() => Comparer.GetHashCode(null).Should().Be(0);

    [Fact]
    public void GetHashCode_EmptyDictionary_ReturnsZero()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty;

        Comparer.GetHashCode(x).Should().Be(0);
    }

    [Fact]
    public void GetHashCode_NonEmptyDictionary_ReturnsCount()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty.Add("key1", 1).Add("key2", 2);

        Comparer.GetHashCode(x).Should().Be(2);
    }

    [Fact]
    public void Equals_DictionariesWithDifferentCounts_ReturnsFalse()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty.Add("key1", 1);
        ImmutableDictionary<string, int> y = ImmutableDictionary<string, int>.Empty.Add("key1", 1).Add("key2", 2);

        Comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Equals_DictionariesWithSameContentDifferentOrder_ReturnsTrue()
    {
        ImmutableDictionary<string, int> x = ImmutableDictionary<string, int>.Empty.Add("key1", 1).Add("key2", 2);
        ImmutableDictionary<string, int> y = ImmutableDictionary<string, int>.Empty.Add("key2", 2).Add("key1", 1);

        Comparer.Equals(x, y).Should().BeTrue();
    }
}
