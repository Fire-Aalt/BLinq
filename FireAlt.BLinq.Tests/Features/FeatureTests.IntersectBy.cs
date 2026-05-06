using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void IntersectBy_ReturnsFirstSourceValueForEachMatchingKey()
        {
            var source = new NativeArray<int>(new[] { 11, 12, 21, 13, 22, 31 }, Allocator.Temp);
            var keys = new NativeArray<int>(new[] { 1, 3, 3 }, Allocator.Temp);
            var actual = source.AsQuery().IntersectBy(keys.AsQuery(), value => value % 10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, new[] { 11, 13 });
        }

        [Test]
        public void IntersectBy_ReturnsEmptyWhenNoKeysMatch()
        {
            var source = new NativeArray<int>(new[] { 11, 12, 21 }, Allocator.Temp);
            var keys = new NativeArray<int>(new[] { 4, 5 }, Allocator.Temp);
            var actual = source.AsQuery().IntersectBy(keys.AsQuery(), value => value % 10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, new int[0]);
        }
    }
}
