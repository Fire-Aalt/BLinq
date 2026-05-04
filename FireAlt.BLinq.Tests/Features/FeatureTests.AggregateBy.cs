using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void AggregateBy_AggregatesValuesPreservingKeyOrder()
        {
            var input = new NativeArray<GroupRecord>(
                new[]
                {
                    new GroupRecord { Group = 2, Value = 10 },
                    new GroupRecord { Group = 1, Value = 20 },
                    new GroupRecord { Group = 2, Value = 30 },
                    new GroupRecord { Group = 1, Value = 40 },
                    new GroupRecord { Group = 3, Value = 50 },
                },
                Allocator.Temp);

            var aggregates = input
                .AsQuery()
                .AggregateBy(
                    value => value.Group,
                    0,
                    (aggregate, value) => aggregate + value.Value)
                .ToNativeList(Allocator.Temp);

            Assert.That(aggregates.Length, Is.EqualTo(3));
            Assert.That(aggregates[0].Key, Is.EqualTo(2));
            Assert.That(aggregates[0].Value, Is.EqualTo(40));
            Assert.That(aggregates[1].Key, Is.EqualTo(1));
            Assert.That(aggregates[1].Value, Is.EqualTo(60));
            Assert.That(aggregates[2].Key, Is.EqualTo(3));
            Assert.That(aggregates[2].Value, Is.EqualTo(50));
        }

        [Test]
        public void AggregateBy_WithSeedSelector_InitializesPerKey()
        {
            var input = new NativeArray<GroupRecord>(
                new[]
                {
                    new GroupRecord { Group = 2, Value = 1 },
                    new GroupRecord { Group = 1, Value = 4 },
                    new GroupRecord { Group = 2, Value = 3 },
                },
                Allocator.Temp);

            var aggregates = input
                .AsQuery()
                .ToAggregatedBy(
                    value => value.Group,
                    key => key * 10,
                    (aggregate, value) => aggregate + value.Value,
                    Allocator.Temp);

            Assert.That(aggregates.Length, Is.EqualTo(2));
            Assert.That(aggregates[0].Key, Is.EqualTo(2));
            Assert.That(aggregates[0].Value, Is.EqualTo(24));
            Assert.That(aggregates[1].Key, Is.EqualTo(1));
            Assert.That(aggregates[1].Value, Is.EqualTo(14));
        }

        [Test]
        public void AggregateBy_HandlesSlimMapResizeAndHashCollisions()
        {
            var input = new NativeArray<int>(160, Allocator.Temp);
            for (var i = 0; i < input.Length; i++)
            {
                input[i] = i;
            }

            var aggregates = input
                .AsQuery()
                .ToAggregatedBy(
                    value => new BadHashKey { Value = value % 80 },
                    0,
                    (aggregate, value) => aggregate + value,
                    Allocator.Temp);

            Assert.That(aggregates.Length, Is.EqualTo(80));

            for (var i = 0; i < aggregates.Length; i++)
            {
                Assert.That(aggregates[i].Key.Value, Is.EqualTo(i));
                Assert.That(aggregates[i].Value, Is.EqualTo(i + i + 80));
            }
        }
    }
}
