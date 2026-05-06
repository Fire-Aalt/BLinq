using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Count_ReturnsElementCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var empty = new NativeArray<int>(0, Allocator.Temp);

            Assert.That(input.AsQuery().Count(), Is.EqualTo(input.Count()));
            Assert.That(empty.AsQuery().Count(), Is.EqualTo(empty.Count()));
        }

        [Test]
        public void Count_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.That(input.AsQuery().Count(value => value % 2 == 0), Is.EqualTo(input.Count(value => value % 2 == 0)));
            Assert.That(input.AsQuery().Count(value => value > 10), Is.EqualTo(input.Count(value => value > 10)));
        }

        [Test]
        public void LongCount_ReturnsElementCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var empty = new NativeArray<int>(0, Allocator.Temp);

            Assert.That(input.AsQuery().LongCount(), Is.EqualTo(input.LongCount()));
            Assert.That(empty.AsQuery().LongCount(), Is.EqualTo(empty.LongCount()));
        }

        [Test]
        public void LongCount_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.That(input.AsQuery().LongCount(value => value % 2 == 0), Is.EqualTo(input.LongCount(value => value % 2 == 0)));
            Assert.That(input.AsQuery().LongCount(value => value > 10), Is.EqualTo(input.LongCount(value => value > 10)));
        }

        [Test]
        public void CountLongCount_WorkOnOrderedQuery()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2, 4 }, Allocator.Temp);
            var ordered = input.AsQuery().OrderBy();

            Assert.That(ordered.Count(), Is.EqualTo(input.Count()));
            Assert.That(ordered.Count(value => value % 2 == 0), Is.EqualTo(input.Count(value => value % 2 == 0)));
            Assert.That(ordered.LongCount(), Is.EqualTo(input.LongCount()));
            Assert.That(ordered.LongCount(value => value % 2 == 0), Is.EqualTo(input.LongCount(value => value % 2 == 0)));
        }
    }
}
