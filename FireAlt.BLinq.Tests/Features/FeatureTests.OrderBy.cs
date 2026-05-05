using System.Collections.Generic;
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
                .OrderBy()
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
                .OrderByDescending()
                .ToNativeList(Allocator.Temp);

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ThenBy_UsesDelegateComparer()
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
                .OrderBy(new PrimaryComparer())
                .ThenBy((x, y) =>
                {
                    return x.Secondary.CompareTo(y.Secondary);
                })
                .ToNativeList(Allocator.Temp);
            var expected = input
                .OrderBy(value => value.Primary)
                .ThenBy(value => value.Secondary)
                .ToArray();

            AssertSequence(ordered.AsArray(), expected);
        }

        [Test]
        public void ThenByDescending_UsesDelegateComparer()
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
                .OrderBy(new PrimaryComparer())
                .ThenByDescending((x, y) =>
                {
                    return x.Secondary.CompareTo(y.Secondary);
                })
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
                .OrderBy(new PrimaryComparer())
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
                .OrderBy(new PrimaryComparer())
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

        private struct PrimaryComparer : IComparer<SortRecord>
        {
            public int Compare(SortRecord x, SortRecord y)
            {
                return x.Primary.CompareTo(y.Primary);
            }
        }
    }
}
