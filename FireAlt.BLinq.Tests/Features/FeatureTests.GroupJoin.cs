using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void GroupJoin_ReturnsOneResultForEachOuterElement()
        {
            var outer = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 12, 22, 13, 33 }, Allocator.Temp);
            var expected = outer
                .GroupJoin(inner, GroupJoinOuterKey, GroupJoinInnerKey, GroupJoinExpectedResult)
                .ToArray();
            var actual = outer.AsQuery()
                .GroupJoin(inner.AsQuery(), GroupJoinOuterKey, GroupJoinInnerKey, GroupJoinResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void GroupJoin_UsesEmptyGroupWhenNoInnerKeysMatch()
        {
            var outer = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            var inner = new NativeArray<int>(new[] { 13, 23 }, Allocator.Temp);
            var expected = outer
                .GroupJoin(inner, GroupJoinOuterKey, GroupJoinInnerKey, GroupJoinExpectedResult)
                .ToArray();
            var actual = outer.AsQuery()
                .GroupJoin(inner.AsQuery(), GroupJoinOuterKey, GroupJoinInnerKey, GroupJoinResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static int GroupJoinOuterKey(int value)
        {
            return value;
        }

        private static int GroupJoinInnerKey(int value)
        {
            return value % 10;
        }

        private static int GroupJoinExpectedResult(int outer, System.Collections.Generic.IEnumerable<int> group)
        {
            return outer * 1000 + group.Sum();
        }

        private static int GroupJoinResult(int outer, Group<int, int> group)
        {
            var sum = 0;
            for (var i = 0; i < group.Length; i++)
            {
                sum += group[i];
            }

            return outer * 1000 + sum;
        }
    }
}
