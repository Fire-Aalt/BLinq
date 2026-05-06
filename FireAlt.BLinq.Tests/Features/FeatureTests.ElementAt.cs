using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void ElementAt_ReturnsIndexedElement()
        {
            var input = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);

            Assert.That(input.AsQuery().ElementAt(1), Is.EqualTo(input.ElementAt(1)));
        }

        [Test]
        public void ElementAt_ThrowsForOutOfRangeIndex()
        {
            var input = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => input.AsQuery().ElementAt(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => input.AsQuery().ElementAt(3));
        }

        [Test]
        public void ElementAtOrDefault_ReturnsDefaultForOutOfRangeIndex()
        {
            var input = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);

            Assert.That(input.AsQuery().ElementAtOrDefault(1), Is.EqualTo(input.ElementAtOrDefault(1)));
            Assert.That(input.AsQuery().ElementAtOrDefault(-1), Is.EqualTo(input.ElementAtOrDefault(-1)));
            Assert.That(input.AsQuery().ElementAtOrDefault(3), Is.EqualTo(input.ElementAtOrDefault(3)));
        }

        [Test]
        public void ElementAt_WorksOnOrderedQuery()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);
            var ordered = input.AsQuery().OrderBy();

            Assert.That(ordered.ElementAt(0), Is.EqualTo(1));
            Assert.That(ordered.ElementAtOrDefault(2), Is.EqualTo(3));
            Assert.That(ordered.ElementAtOrDefault(3), Is.EqualTo(default(int)));
        }
    }
}
