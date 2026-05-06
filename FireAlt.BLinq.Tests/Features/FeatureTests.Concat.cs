using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Concat_ReturnsSourceThenSecondQuery()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 4, 5 }, Allocator.Temp);
            var expected = left.Concat(right).ToArray();
            var actual = left.AsQuery().Concat(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Concat_ReturnsSecondQueryWhenSourceIsEmpty()
        {
            var left = new NativeArray<int>(0, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 4, 5 }, Allocator.Temp);
            var expected = left.Concat(right).ToArray();
            var actual = left.AsQuery().Concat(right.AsQuery()).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
