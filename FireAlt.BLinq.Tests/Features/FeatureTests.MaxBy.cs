using System;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void MaxBy_ReturnsElementWithMaximumKey()
        {
            var input = new NativeArray<int>(new[] { 13, 21, 32 }, Allocator.Temp);
            var actual = input.AsQuery().MaxBy<int, int, NativeArray<int>.Enumerator>(value => value % 10);

            Assert.That(actual, Is.EqualTo(13));
        }

        [Test]
        public void MaxBy_ThrowsWhenSourceIsEmpty()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() =>
                input.AsQuery().MaxBy<int, int, NativeArray<int>.Enumerator>(value => value % 10));
        }
    }
}
