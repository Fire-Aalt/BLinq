using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void DistinctBy_ReturnsFirstOccurrenceOfEachKey()
        {
            var input = new NativeArray<int>(new[] { 11, 12, 21, 13, 22, 31 }, Allocator.Temp);
            var actual = input.AsQuery().DistinctBy(value => value % 10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, new[] { 11, 12, 13 });
        }

        [Test]
        public void DistinctBy_ReturnsEmptyForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var actual = input.AsQuery().DistinctBy(value => value % 10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, new int[0]);
        }
    }
}
