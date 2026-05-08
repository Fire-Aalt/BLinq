using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Zip_PairsValuesUntilShortestInputEnds()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);

            var actual = left
                .AsQuery()
                .Zip(right.AsQuery())
                .ToNativeArray(Allocator.Temp);

            AssertSequence(
                actual,
                new[]
                {
                    new ValueTuple<int, int>(1, 10),
                    new ValueTuple<int, int>(2, 20),
                    new ValueTuple<int, int>(3, 30),
                });
        }

        [Test]
        public void Zip_ProjectsValuesUntilShortestInputEnds()
        {
            var left = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var right = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
            var expected = left.Zip(right, ZipResult).ToArray();

            var actual = left
                .AsQuery()
                .Zip(right.AsQuery(), ZipResult)
                .ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        private static int ZipResult(int left, int right)
        {
            return left + right;
        }
    }
}
