using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void SequenceEquals_ComparesValuesInOrder()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var different = new NativeArray<int>(new[] { 1, 3, 2 }, Allocator.Temp);
            var expectedEqual = left.SequenceEqual(right);
            var expectedDifferent = left.SequenceEqual(different);

            Assert.That(left.AsQuery().SequenceEqual(right.AsQuery()), Is.EqualTo(expectedEqual));
            Assert.That(left.AsQuery().SequenceEqual(different.AsQuery()), Is.EqualTo(expectedDifferent));
        }
    }
}
