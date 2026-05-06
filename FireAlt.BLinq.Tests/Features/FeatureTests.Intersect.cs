using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Intersect_ReturnsDistinctValuesFoundInBothQueries()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 2, 3, 4 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 2, 4, 4, 5 }, Allocator.Temp);
            var expected = left.Intersect(right).ToArray();
            var actual = left.AsQuery().Intersect(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Intersect_ReturnsEmptyWhenNoValuesMatch()
        {
            var left = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 3, 4 }, Allocator.Temp);
            var expected = left.Intersect(right).ToArray();
            var actual = left.AsQuery().Intersect(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
