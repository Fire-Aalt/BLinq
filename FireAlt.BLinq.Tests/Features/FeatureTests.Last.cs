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
        public void Last_ReturnsLastElement()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(input.AsQuery().Last(), Is.EqualTo(input.Last()));
        }

        [Test]
        public void Last_ThrowsForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => input.AsQuery().Last());
        }

        [Test]
        public void LastOrDefault_ReturnsDefaultForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            Assert.That(input.AsQuery().LastOrDefault(), Is.EqualTo(input.LastOrDefault()));
        }

        [Test]
        public void Last_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            Assert.That(input.AsQuery().Last(value => value % 2 == 0), Is.EqualTo(input.Last(value => value % 2 == 0)));
            Assert.That(input.AsQuery().LastOrDefault(value => value > 10), Is.EqualTo(input.LastOrDefault(value => value > 10)));
        }
    }
}
