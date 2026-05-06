using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Take_ReturnsRequestedElementCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var expected = input.Take(3).ToArray();
            var actual = input.AsQuery().Take(3).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Take_ReturnsEmptyForNegativeCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Take(-1).ToArray();
            var actual = input.AsQuery().Take(-1).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Take_ReturnsAllElementsWhenCountExceedsLength()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Take(10).ToArray();
            var actual = input.AsQuery().Take(10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
