using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void TakeWhile_ReturnsMatchingPrefix()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 2, 1 }, Allocator.Temp);
            var expected = input.TakeWhile(LessThanThreeForTakeWhile).ToArray();
            var actual = input.AsQuery().TakeWhile(LessThanThreeForTakeWhile).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void TakeWhile_ReturnsEmptyWhenFirstElementDoesNotMatch()
        {
            var input = new NativeArray<int>(new[] { 3, 2, 1 }, Allocator.Temp);
            var expected = input.TakeWhile(LessThanThreeForTakeWhile).ToArray();
            var actual = input.AsQuery().TakeWhile(LessThanThreeForTakeWhile).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static bool LessThanThreeForTakeWhile(int value)
        {
            return value < 3;
        }
    }
}
