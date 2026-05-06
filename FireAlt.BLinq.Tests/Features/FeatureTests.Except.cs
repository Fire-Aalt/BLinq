using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Except_ReturnsDistinctValuesNotFoundInOtherQuery()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 2, 3, 4 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 2, 4, 5 }, Allocator.Temp);
            var expected = left.Except(right).ToArray();
            var actual = left.AsQuery().Except(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Except_ReturnsDistinctSourceValuesWhenOtherIsEmpty()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 2, 3 }, Allocator.Temp);
            var right = new NativeArray<int>(0, Allocator.Temp);
            var expected = left.Except(right).ToArray();
            var actual = left.AsQuery().Except(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
