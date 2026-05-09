using System;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Index_ReturnsCorrectIndex()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);

            var chunks = input
                .AsQuery()
                .Index()
                .ToManagedArray();

            Assert.That(chunks.Length, Is.EqualTo(5));
            Assert.That(chunks[0].Index, Is.EqualTo(0));
            Assert.That(chunks[2].Index, Is.EqualTo(2));
            Assert.That(chunks[4].Index, Is.EqualTo(4));
        }

        [Test]
        public void Index_ReturnCorrectValue()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);

            var chunks = input
                .AsQuery()
                .Index()
                .ToManagedArray();

            Assert.That(chunks.Length, Is.EqualTo(5));
            Assert.That(chunks[0].Item, Is.EqualTo(1));
            Assert.That(chunks[2].Item, Is.EqualTo(3));
            Assert.That(chunks[4].Item, Is.EqualTo(5));
        }
    }
}
