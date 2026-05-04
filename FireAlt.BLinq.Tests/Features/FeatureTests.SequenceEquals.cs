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

            Assert.That(left.AsQuery().SequenceEquals(right.AsQuery()), Is.True);
            Assert.That(left.AsQuery().SequenceEquals(different.AsQuery()), Is.False);
        }
    }
}
