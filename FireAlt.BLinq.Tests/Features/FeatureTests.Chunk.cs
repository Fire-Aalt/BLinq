using System;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Chunk_SplitsValuesIntoFixedSizeChunks()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);

            var chunks = input
                .AsQuery()
                .Chunk(2)
                .ToManagedArray();

            Assert.That(chunks.Length, Is.EqualTo(3));
            AssertSequence(chunks[0].Values, new[] { 1, 2 });
            AssertSequence(chunks[1].Values, new[] { 3, 4 });
            AssertSequence(chunks[2].Values, new[] { 5 });
        }

        [Test]
        public void Chunk_EmptyInputProducesNoChunks()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);

            var chunks = input
                .AsQuery()
                .Chunk(3)
                .ToManagedArray();

            Assert.That(chunks.Length, Is.EqualTo(0));
        }

        [Test]
        public void Chunk_RejectsNonPositiveSize()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.Throws<ArgumentOutOfRangeException>(() => input.AsQuery().Chunk(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => input.AsQuery().Chunk(-1));
        }
    }
}
