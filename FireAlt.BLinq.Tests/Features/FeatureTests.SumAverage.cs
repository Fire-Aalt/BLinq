using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Sum_UsesBuiltInAccumulator()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            Assert.That(input.AsQuery().Sum(), Is.EqualTo(6));
        }

        [Test]
        public void Sum_WithSelector_UsesBuiltInAccumulator()
        {
            var input = new NativeArray<GroupRecord>(
                new[]
                {
                    new GroupRecord { Group = 1, Value = 10 },
                    new GroupRecord { Group = 2, Value = 20 },
                    new GroupRecord { Group = 3, Value = 30 },
                },
                Allocator.Temp);

            Assert.That(input.AsQuery().Sum(value => value.Group), Is.EqualTo(6));
        }

        [Test]
        public void Average_UsesBuiltInAccumulator()
        {
            var input = new NativeArray<float>(new[] { 1f, 2f, 3f }, Allocator.Temp);

            Assert.That(input.AsQuery().Average(), Is.EqualTo(2f));
        }

        [Test]
        public void Average_WithSelector_UsesBuiltInAccumulator()
        {
            var input = new NativeArray<int>(new[] { 2, 4, 6 }, Allocator.Temp);

            Assert.That(input.AsQuery().Average(value => value), Is.EqualTo(4));
        }

        [Test]
        public void SumAndAverage_UseBuiltInVectorAccumulator()
        {
            var input = new NativeArray<float3>(
                new[] { new float3(1f, 2f, 3f), new float3(3f, 4f, 5f) },
                Allocator.Temp);

            Assert.That(input.AsQuery().Sum(), Is.EqualTo(new float3(4f, 6f, 8f)));
            Assert.That(input.AsQuery().Average(), Is.EqualTo(new float3(2f, 3f, 4f)));
        }
    }
}
