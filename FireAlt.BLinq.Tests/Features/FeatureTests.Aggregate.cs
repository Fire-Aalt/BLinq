using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Aggregate_WithoutSeed_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.That(input.AsQuery().Aggregate((aggregate, value) => aggregate + value), Is.EqualTo(input.Aggregate((aggregate, value) => aggregate + value)));
        }

        [Test]
        public void Aggregate_WithoutSeed_ThrowsForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => input.AsQuery().Aggregate((aggregate, value) => aggregate + value));
        }

        [Test]
        public void Aggregate_WithSeed_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(input.AsQuery().Aggregate(10, (aggregate, value) => aggregate + value), Is.EqualTo(input.Aggregate(10, (aggregate, value) => aggregate + value)));
        }

        [Test]
        public void Aggregate_WithResultSelector_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(
                input.AsQuery().Aggregate(10, (aggregate, value) => aggregate + value, aggregate => aggregate * 2),
                Is.EqualTo(input.Aggregate(10, (aggregate, value) => aggregate + value, aggregate => aggregate * 2)));
        }

        [Test]
        public void Aggregate_WorksOnOrderedQuery()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);

            Assert.That(input.AsQuery().OrderBy().Aggregate((aggregate, value) => aggregate * 10 + value), Is.EqualTo(123));
            Assert.That(input.AsQuery().OrderBy().Aggregate(0, (aggregate, value) => aggregate * 10 + value), Is.EqualTo(123));
            Assert.That(input.AsQuery().OrderBy().Aggregate(0, (aggregate, value) => aggregate * 10 + value, aggregate => aggregate + 1), Is.EqualTo(124));
        }
    }
}
