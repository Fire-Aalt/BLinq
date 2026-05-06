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
        public void Single_ReturnsOnlyElement()
        {
            var input = new NativeArray<int>(new[] { 7 }, Allocator.Temp);

            Assert.That(input.AsQuery().Single(), Is.EqualTo(input.Single()));
        }

        [Test]
        public void Single_ThrowsForEmptyOrMultipleElements()
        {
            var empty = new NativeArray<int>(0, Allocator.Temp);
            var multiple = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => empty.AsQuery().Single());
            Assert.Throws<InvalidOperationException>(() => multiple.AsQuery().Single());
        }

        [Test]
        public void Single_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(input.AsQuery().Single(value => value == 2), Is.EqualTo(input.Single(value => value == 2)));
            Assert.Throws<InvalidOperationException>(() => input.AsQuery().Single(value => value > 10));
            Assert.Throws<InvalidOperationException>(() => input.AsQuery().Single(value => value > 1));
        }

        [Test]
        public void SingleOrDefault_ReturnsOnlyElementOrDefault()
        {
            var input = new NativeArray<int>(new[] { 7 }, Allocator.Temp);
            var empty = new NativeArray<int>(0, Allocator.Temp);

            Assert.That(input.AsQuery().SingleOrDefault(), Is.EqualTo(input.SingleOrDefault()));
            Assert.That(empty.AsQuery().SingleOrDefault(), Is.EqualTo(empty.SingleOrDefault()));
        }

        [Test]
        public void SingleOrDefault_WithPredicate_MatchesLinq()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(input.AsQuery().SingleOrDefault(value => value == 2), Is.EqualTo(input.SingleOrDefault(value => value == 2)));
            Assert.That(input.AsQuery().SingleOrDefault(value => value > 10), Is.EqualTo(input.SingleOrDefault(value => value > 10)));
            Assert.Throws<InvalidOperationException>(() => input.AsQuery().SingleOrDefault(value => value > 1));
        }
    }
}
