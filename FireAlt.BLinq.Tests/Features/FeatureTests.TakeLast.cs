using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void TakeLast_ReturnsRequestedTailElements()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var expected = input.TakeLast(3).ToArray();
            var actual = input.AsQuery().TakeLast(3).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void TakeLast_ReturnsEmptyForNegativeCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.TakeLast(-1).ToArray();
            var actual = input.AsQuery().TakeLast(-1).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void TakeLast_ReturnsAllElementsWhenCountExceedsLength()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.TakeLast(10).ToArray();
            var actual = input.AsQuery().TakeLast(10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
