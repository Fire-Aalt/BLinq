using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void GroupBy_GroupsValuesPreservingGroupAndElementOrder()
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
            var expected = input.GroupBy(value => value.Group).ToArray();

            var grouped = input
                .AsQuery()
                .ToLookup(value => value.Group, Allocator.Temp);

            AssertLookup(grouped, expected);
        }

        [Test]
        public void GroupBy_GroupsCanBeEnumeratedAndMaterialized()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var expected = input.GroupBy(value => value % 2).ToArray();
            var grouped = input
                .AsQuery()
                .ToLookup(value => value % 2, Allocator.Temp);

            var oddSum = 0;
            var even = default(NativeList<int>);

            foreach (var group in grouped)
            {
                if (group.Key == 1)
                {
                    foreach (var value in group)
                    {
                        oddSum += value;
                    }
                }
                else
                {
                    even = group.AsQuery().ToNativeList(Allocator.Temp);
                }
            }

            var expectedOddSum = expected.Single(group => group.Key == 1).Sum();
            var expectedEven = expected.Single(group => group.Key == 0).ToArray();
            var expectedValueCount = expected.Sum(group => group.Count());

            Assert.That(oddSum, Is.EqualTo(expectedOddSum));
            AssertSequence(even.AsArray(), expectedEven);
            Assert.That(grouped.ValueCount, Is.EqualTo(expectedValueCount));
        }

        [Test]
        public void GroupBy_HandlesSlimMapResizeAndHashCollisions()
        {
            var input = new NativeArray<int>(160, Allocator.Temp);
            for (var i = 0; i < input.Length; i++)
            {
                input[i] = i;
            }
            var expected = input.GroupBy(value => new BadHashKey { Value = value % 80 }).ToArray();

            var grouped = input
                .AsQuery()
                .ToLookup(value => new BadHashKey { Value = value % 80 }, Allocator.Temp);

            AssertLookup(grouped, expected);
        }

        private struct GroupRecord
        {
            public int Group;
            public int Value;
        }

        private struct BadHashKey : System.IEquatable<BadHashKey>
        {
            public int Value;

            public bool Equals(BadHashKey other)
            {
                return Value == other.Value;
            }

            public override int GetHashCode()
            {
                return Value & 7;
            }
        }
    }
}
