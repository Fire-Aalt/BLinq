using NUnit.Framework;
using Unity.Collections;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void JoinLeft_ReturnsMatchesAndDefaultInnerForUnmatchedOuter()
        {
            var outer = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 12, 22, 13, 33 }, Allocator.Temp);
            var expected = outer.AsValueEnumerable()
                .LeftJoin(inner.AsValueEnumerable(), JoinLeftOuterKey, JoinLeftInnerKey, JoinLeftResult)
                .ToArray();

            var actual = outer.AsQuery()
                .JoinLeft(inner.AsQuery(), JoinLeftOuterKey, JoinLeftInnerKey, JoinLeftResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void JoinLeft_ReturnsOneDefaultInnerResultWhenNoKeysMatch()
        {
            var outer = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 13, 23 }, Allocator.Temp);
            var expected = outer.AsValueEnumerable()
                .LeftJoin(inner.AsValueEnumerable(), JoinLeftOuterKey, JoinLeftInnerKey, JoinLeftResult)
                .ToArray();

            var actual = outer.AsQuery()
                .JoinLeft(inner.AsQuery(), JoinLeftOuterKey, JoinLeftInnerKey, JoinLeftResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static int JoinLeftOuterKey(int value)
        {
            return value;
        }

        private static int JoinLeftInnerKey(int value)
        {
            return value % 10;
        }

        private static int JoinLeftResult(int outer, int inner)
        {
            return outer * 100 + inner;
        }
    }
}
