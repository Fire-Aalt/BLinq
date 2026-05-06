using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Skip_BypassesRequestedElementCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var expected = input.Skip(2).ToArray();
            var actual = input.AsQuery().Skip(2).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Skip_ReturnsAllElementsForNegativeCount()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Skip(-1).ToArray();
            var actual = input.AsQuery().Skip(-1).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Skip_ReturnsEmptyWhenCountExceedsLength()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Skip(10).ToArray();
            var actual = input.AsQuery().Skip(10).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
