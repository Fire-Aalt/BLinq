using System.Collections.Generic;
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
            var ordered = input.AsQuery().ToOrderedBy(Allocator.Temp);

            Assert.That(ordered.Length, Is.EqualTo(3));
            Assert.That(ordered[0], Is.EqualTo(1));
            Assert.That(ordered[1], Is.EqualTo(2));
            Assert.That(ordered[2], Is.EqualTo(3));
        }

        [Test]
        public void OrderByDescending_SortsDescending()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);
            var ordered = input.AsQuery().ToOrderedByDescending(Allocator.Temp);

            Assert.That(ordered.Length, Is.EqualTo(3));
            Assert.That(ordered[0], Is.EqualTo(3));
            Assert.That(ordered[1], Is.EqualTo(2));
            Assert.That(ordered[2], Is.EqualTo(1));
        }

        [Test]
        public void ThenBy_ComposesPrimaryAndSecondaryComparers()
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
                .ThenBy(new SecondaryComparer())
                .ToNativeList(Allocator.Temp);

            Assert.That(ordered.Length, Is.EqualTo(4));
            Assert.That(ordered[0], Is.EqualTo(new SortRecord { Primary = 0, Secondary = 3 }));
            Assert.That(ordered[1], Is.EqualTo(new SortRecord { Primary = 0, Secondary = 5 }));
            Assert.That(ordered[2], Is.EqualTo(new SortRecord { Primary = 1, Secondary = 1 }));
            Assert.That(ordered[3], Is.EqualTo(new SortRecord { Primary = 1, Secondary = 2 }));
        }

        [Test]
        public void ThenByDescending_ComposesPrimaryAndDescendingSecondaryComparers()
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
                .ThenByDescending(new SecondaryComparer())
                .ToNativeList(Allocator.Temp);

            Assert.That(ordered.Length, Is.EqualTo(4));
            Assert.That(ordered[0], Is.EqualTo(new SortRecord { Primary = 0, Secondary = 5 }));
            Assert.That(ordered[1], Is.EqualTo(new SortRecord { Primary = 0, Secondary = 3 }));
            Assert.That(ordered[2], Is.EqualTo(new SortRecord { Primary = 1, Secondary = 2 }));
            Assert.That(ordered[3], Is.EqualTo(new SortRecord { Primary = 1, Secondary = 1 }));
        }

        private struct SortRecord
        {
            public int Primary;
            public int Secondary;
        }

        private struct PrimaryComparer : IComparer<SortRecord>
        {
            public int Compare(SortRecord x, SortRecord y)
            {
                return x.Primary.CompareTo(y.Primary);
            }
        }

        private struct SecondaryComparer : IComparer<SortRecord>
        {
            public int Compare(SortRecord x, SortRecord y)
            {
                return x.Secondary.CompareTo(y.Secondary);
            }
        }
    }
}
