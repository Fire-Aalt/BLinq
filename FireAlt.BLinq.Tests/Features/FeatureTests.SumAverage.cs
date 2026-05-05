using System.Linq;
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
            var expected = input.Sum();

            Assert.That(input.AsQuery().Sum(), Is.EqualTo(expected));
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
            var expected = input.Sum(value => value.Group);

            Assert.That(input.AsQuery().Sum(value => value.Group), Is.EqualTo(expected));
        }

        [Test]
        public void Average_UsesBuiltInAccumulator()
        {
            var input = new NativeArray<float>(new[] { 1f, 2f, 3f }, Allocator.Temp);
            var expected = input.Average();

            Assert.That(input.AsQuery().Average(), Is.EqualTo(expected));
        }

        [Test]
        public void Average_WithSelector_UsesBuiltInAccumulator()
        {
            var input = new NativeArray<int>(new[] { 2, 4, 6 }, Allocator.Temp);
            var expected = input.Average(value => value);

            Assert.That(input.AsQuery().Average(value => value), Is.EqualTo(expected));
        }

        [Test]
        public void SumAndAverage_UseBuiltInVectorAccumulator()
        {
            var input = new NativeArray<float3>(
                new[] { new float3(1f, 2f, 3f), new float3(3f, 4f, 5f) },
                Allocator.Temp);
            var expectedSum = new float3(
                input.Select(value => value.x).Sum(),
                input.Select(value => value.y).Sum(),
                input.Select(value => value.z).Sum());
            var expectedAverage = new float3(
                input.Select(value => value.x).Average(),
                input.Select(value => value.y).Average(),
                input.Select(value => value.z).Average());

            Assert.That(input.AsQuery().Sum(), Is.EqualTo(expectedSum));
            Assert.That(input.AsQuery().Average(), Is.EqualTo(expectedAverage));
        }
    }
}
