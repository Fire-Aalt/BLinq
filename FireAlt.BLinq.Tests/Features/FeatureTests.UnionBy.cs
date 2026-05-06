using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void UnionBy_ReturnsDistinctValuesFromBothQueriesByKey()
        {
            var left = new NativeArray<int>(new[] { 11, 12, 21 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 13, 22, 34 }, Allocator.Temp);
            var actual = left.AsQuery().UnionBy(right.AsQuery(), value => value % 10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, new[] { 11, 12, 13, 34 });
        }

        [Test]
        public void UnionBy_ReturnsRightValuesWhenLeftIsEmpty()
        {
            var left = new NativeArray<int>(0, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 13, 22, 34, 43 }, Allocator.Temp);
            var actual = left.AsQuery().UnionBy(right.AsQuery(), value => value % 10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, new[] { 13, 22, 34 });
        }
    }
}
