using System;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void MinBy_ReturnsElementWithMinimumKey()
        {
            var input = new NativeArray<int>(new[] { 13, 21, 32 }, Allocator.Temp);
            var actual = input.AsQuery().MinBy<int, int, NativeArray<int>.Enumerator>(value => value % 10);

            Assert.That(actual, Is.EqualTo(21));
        }

        [Test]
        public void MinBy_ThrowsWhenSourceIsEmpty()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() =>
                input.AsQuery().MinBy<int, int, NativeArray<int>.Enumerator>(value => value % 10));
        }
    }
}
