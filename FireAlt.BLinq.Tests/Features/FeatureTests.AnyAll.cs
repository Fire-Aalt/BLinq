using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Any_ReturnsWhetherSourceHasElements()
        {
            var input = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var empty = new NativeArray<int>(0, Allocator.Temp);

            Assert.That(input.AsQuery().Any(), Is.EqualTo(input.Any()));
            Assert.That(empty.AsQuery().Any(), Is.EqualTo(empty.Any()));
        }

        [Test]
        public void Any_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(input.AsQuery().Any(value => value > 2), Is.EqualTo(input.Any(value => value > 2)));
            Assert.That(input.AsQuery().Any(value => value > 10), Is.EqualTo(input.Any(value => value > 10)));
        }

        [Test]
        public void All_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 2, 4, 6 }, Allocator.Temp);
            var empty = new NativeArray<int>(0, Allocator.Temp);

            Assert.That(input.AsQuery().All(value => value % 2 == 0), Is.EqualTo(input.All(value => value % 2 == 0)));
            Assert.That(input.AsQuery().All(value => value > 3), Is.EqualTo(input.All(value => value > 3)));
            Assert.That(empty.AsQuery().All(value => value > 3), Is.EqualTo(empty.All(value => value > 3)));
        }
    }
}
