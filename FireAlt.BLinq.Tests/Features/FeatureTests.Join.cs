using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Join_ReturnsMatchingOuterInnerPairs()
        {
            var outer = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 12, 22, 13, 33 }, Allocator.Temp);
            var expected = outer.Join(inner, JoinOuterKey, JoinInnerKey, JoinResult).ToArray();
            var actual = outer.AsQuery()
                .Join(inner.AsQuery(), JoinOuterKey, JoinInnerKey, JoinResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Join_ReturnsEmptyWhenNoKeysMatch()
        {
            var outer = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 13, 23 }, Allocator.Temp);
            var expected = outer.Join(inner, JoinOuterKey, JoinInnerKey, JoinResult).ToArray();
            var actual = outer.AsQuery()
                .Join(inner.AsQuery(), JoinOuterKey, JoinInnerKey, JoinResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static int JoinOuterKey(int value)
        {
            return value;
        }

        private static int JoinInnerKey(int value)
        {
            return value % 10;
        }

        private static int JoinResult(int outer, int inner)
        {
            return outer * 100 + inner;
        }
    }
}
