using NUnit.Framework;
using Unity.Collections;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void JoinRight_ReturnsMatchesAndDefaultOuterForUnmatchedInner()
        {
            var outer = new NativeArray<int>(new[] { 2, 3 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 11, 12, 22, 13, 33, 44 }, Allocator.Temp);
            var expected = outer.AsValueEnumerable()
                .RightJoin(inner.AsValueEnumerable(), JoinRightOuterKey, JoinRightInnerKey, JoinRightResult)
                .ToArray();

            var actual = outer.AsQuery()
                .JoinRight(inner.AsQuery(), JoinRightOuterKey, JoinRightInnerKey, JoinRightResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void JoinRight_ReturnsOneDefaultOuterResultWhenNoKeysMatch()
        {
            var outer = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 13, 23 }, Allocator.Temp);
            var expected = outer.AsValueEnumerable()
                .RightJoin(inner.AsValueEnumerable(), JoinRightOuterKey, JoinRightInnerKey, JoinRightResult)
                .ToArray();

            var actual = outer.AsQuery()
                .JoinRight(inner.AsQuery(), JoinRightOuterKey, JoinRightInnerKey, JoinRightResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static int JoinRightOuterKey(int value)
        {
            return value;
        }

        private static int JoinRightInnerKey(int value)
        {
            return value % 10;
        }

        private static int JoinRightResult(int outer, int inner)
        {
            return outer * 100 + inner;
        }
    }
}
