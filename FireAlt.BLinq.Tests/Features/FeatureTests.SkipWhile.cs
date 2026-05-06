using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void SkipWhile_BypassesMatchingPrefix()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 2, 1 }, Allocator.Temp);
            var expected = input.SkipWhile(LessThanThree).ToArray();
            var actual = input.AsQuery().SkipWhile(LessThanThree).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void SkipWhile_ReturnsEmptyWhenAllElementsMatch()
        {
            var input = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var expected = input.SkipWhile(LessThanThree).ToArray();
            var actual = input.AsQuery().SkipWhile(LessThanThree).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static bool LessThanThree(int value)
        {
            return value < 3;
        }
    }
}
