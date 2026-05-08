using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void CountBy_CountsKeysInFirstSeenOrder()
        {
            var input = new NativeArray<int>(new[] { 11, 22, 13, 24, 35, 16 }, Allocator.Temp);

            var actual = input
                .AsQuery()
                .CountBy(CountByKey)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(
                actual,
                new[]
                {
                    new KeyValuePair<int, int>(1, 3),
                    new KeyValuePair<int, int>(2, 2),
                    new KeyValuePair<int, int>(3, 1),
                });
        }

        [Test]
        public void CountBy_CanMaterializeToNativeList()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            var actual = input
                .AsQuery()
                .ToCountedBy(value => value % 2, Allocator.Temp);

            AssertSequence(
                actual.AsArray(),
                new[]
                {
                    new KeyValuePair<int, int>(1, 3),
                    new KeyValuePair<int, int>(0, 3),
                });
        }

        [Test]
        public void CountBy_EmptyInputProducesNoCounts()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            var actual = input
                .AsQuery()
                .CountBy(CountByKey)
                .ToNativeArray(Allocator.Temp);

            Assert.That(actual.Length, Is.EqualTo(0));
        }

        private static int CountByKey(int value)
        {
            return value / 10;
        }
    }
}
