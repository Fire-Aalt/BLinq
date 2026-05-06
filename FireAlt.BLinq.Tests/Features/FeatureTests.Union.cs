using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Union_ReturnsDistinctValuesFromBothQueries()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 1, 3 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 3, 4, 2, 5 }, Allocator.Temp);
            var expected = left.Union(right).ToArray();
            var actual = left.AsQuery().Union(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Union_ReturnsRightValuesWhenLeftIsEmpty()
        {
            var left = new NativeArray<int>(0, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 3, 4, 3 }, Allocator.Temp);
            var expected = left.Union(right).ToArray();
            var actual = left.AsQuery().Union(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
