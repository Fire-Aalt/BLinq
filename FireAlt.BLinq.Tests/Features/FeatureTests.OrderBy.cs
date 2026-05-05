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
            var ordered = input
                .AsQuery()
                .OrderBy(value => value)
                .ToNativeList(Allocator.Temp);

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void OrderByDescending_SortsDescending()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);
            var expected = input.OrderByDescending(value => value).ToArray();
            var ordered = input
                .AsQuery()
                .OrderByDescending(value => value)
                .ToNativeList(Allocator.Temp);

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ThenBy_UsesDelegateKeySelector()
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
                .OrderBy(value => value.Primary)
                .ThenBy(value => value.Secondary)
                .ToNativeList(Allocator.Temp);
            var expected = input
                .OrderBy(value => value.Primary)
                .ThenBy(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ThenByDescending_UsesDelegateKeySelector()
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
                .OrderBy(value => value.Primary)
                .ThenByDescending(value => value.Secondary)
                .ToNativeList(Allocator.Temp);
            var expected = input
                .OrderBy(value => value.Primary)
                .ThenByDescending(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ThenBy_UsesDefaultAscendingComparer()
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
                .OrderBy(value => value.Primary)
                .ThenBy()
                .ToNativeList(Allocator.Temp);
            var expected = input
                .OrderBy(value => value.Primary)
                .ThenBy(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ThenByDescending_UsesDefaultDescendingComparer()
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
                .OrderBy(value => value.Primary)
                .ThenByDescending()
                .ToNativeList(Allocator.Temp);
            var expected = input
                .OrderBy(value => value.Primary)
                .ThenByDescending(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        private struct SortRecord : System.IComparable<SortRecord>
        {
            public int Primary;
            public int Secondary;

            public int CompareTo(SortRecord other)
            {
                return Secondary.CompareTo(other.Secondary);
            }
        }
    }
}
