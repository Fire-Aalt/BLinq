using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void OrderBy_SortsAscending()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);
            var expected = input.OrderBy(value => value).ToArray();
            var ordered = input.AsQuery().ToOrderedBy(Allocator.Temp);

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void OrderByDescending_SortsDescending()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);
            var expected = input.OrderByDescending(value => value).ToArray();
            var ordered = input.AsQuery().ToOrderedByDescending(Allocator.Temp);

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ToOrderedBy_UsesCustomComparator()
        {
            var input = new NativeArray<SortRecord>(
                new[]
                {
                    new SortRecord { Primary = 1, Secondary = 2 },
                    new SortRecord { Primary = 0, Secondary = 5 },
                    new SortRecord { Primary = 1, Secondary = 1 },
                    new SortRecord { Primary = 0, Secondary = 3 },
                },
                Allocator.Temp);

            var ordered = input
                .AsQuery()
                .ToOrderedBy((x, y) =>
                {
                    var primary = x.Primary.CompareTo(y.Primary);
                    return primary != 0 ? primary : x.Secondary.CompareTo(y.Secondary);
                }, Allocator.Temp);
            var expected = input
                .OrderBy(value => value.Primary)
                .ThenBy(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ToOrderedByDescending_UsesCustomComparator()
        {
            var input = new NativeArray<SortRecord>(
                new[]
                {
                    new SortRecord { Primary = 1, Secondary = 2 },
                    new SortRecord { Primary = 0, Secondary = 5 },
                    new SortRecord { Primary = 1, Secondary = 1 },
                    new SortRecord { Primary = 0, Secondary = 3 },
                },
                Allocator.Temp);

            var ordered = input
                .AsQuery()
                .ToOrderedByDescending((x, y) =>
                {
                    var primary = x.Primary.CompareTo(y.Primary);
                    return primary != 0 ? primary : x.Secondary.CompareTo(y.Secondary);
                }, Allocator.Temp);
            var expected = input
                .OrderByDescending(value => value.Primary)
                .ThenByDescending(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        private struct SortRecord
        {
            public int Primary;
            public int Secondary;
        }
    }
}
