using System.Collections.Generic;
using System.Linq;
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
            var expected = input
                .GroupBy(value => value.Group)
                .Select(group => new KeyValuePair<int, int>(group.Key, group.Sum(value => value.Value)))
                .ToArray();

            var aggregates = input
                .AsQuery()
                .AggregateBy(
                    value => value.Group,
                    0,
                    (aggregate, value) => aggregate + value.Value)
                .ToNativeList(Allocator.Temp);

            AssertSequence(aggregates.AsArray(), expected);
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
            var expected = input
                .GroupBy(value => value.Group)
                .Select(group => new KeyValuePair<int, int>(group.Key, (group.Key * 10) + group.Sum(value => value.Value)))
                .ToArray();
            
            var aggregates = input
                .AsQuery()
                .ToAggregatedBy(
                    value => value.Group,
                    key => key * 10,
                    (aggregate, value) => aggregate + value.Value,
                    Allocator.Temp);

            AssertSequence(aggregates.AsArray(), expected);
        }

        [Test]
        public void AggregateBy_HandlesSlimMapResizeAndHashCollisions()
        {
            var input = new NativeArray<int>(160, Allocator.Temp);
            for (var i = 0; i < input.Length; i++)
            {
                input[i] = i;
            }
            var expected = input
                .GroupBy(value => new BadHashKey { Value = value % 80 })
                .Select(group => new KeyValuePair<BadHashKey, int>(group.Key, group.Sum(value => value)))
                .ToArray();

            var aggregates = input
                .AsQuery()
                .ToAggregatedBy(
                    value => new BadHashKey { Value = value % 80 },
                    0,
                    (aggregate, value) => aggregate + value,
                    Allocator.Temp);

            AssertSequence(aggregates.AsArray(), expected);
        }
    }
}
