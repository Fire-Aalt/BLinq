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

            var grouped = input
                .AsQuery()
                .ToLookup(value => value.Group, Allocator.Temp);

            Assert.That(grouped.GroupCount, Is.EqualTo(3));
            Assert.That(grouped.ValueCount, Is.EqualTo(5));

            Assert.That(grouped[0].Key, Is.EqualTo(2));
            Assert.That(grouped[0].Length, Is.EqualTo(2));
            Assert.That(grouped[0][0].Value, Is.EqualTo(10));
            Assert.That(grouped[0][1].Value, Is.EqualTo(30));

            Assert.That(grouped[1].Key, Is.EqualTo(1));
            Assert.That(grouped[1].Length, Is.EqualTo(2));
            Assert.That(grouped[1][0].Value, Is.EqualTo(20));
            Assert.That(grouped[1][1].Value, Is.EqualTo(40));

            Assert.That(grouped[2].Key, Is.EqualTo(3));
            Assert.That(grouped[2].Length, Is.EqualTo(1));
            Assert.That(grouped[2][0].Value, Is.EqualTo(50));
        }

        [Test]
        public void GroupBy_GroupsCanBeEnumeratedAndMaterialized()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
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

            Assert.That(oddSum, Is.EqualTo(9));
            Assert.That(even.Length, Is.EqualTo(2));
            Assert.That(even[0], Is.EqualTo(2));
            Assert.That(even[1], Is.EqualTo(4));
            Assert.That(grouped.AsQuery().Select(group => group.Length).Sum(), Is.EqualTo(5));
        }

        [Test]
        public void GroupBy_HandlesSlimMapResizeAndHashCollisions()
        {
            var input = new NativeArray<int>(160, Allocator.Temp);
            for (var i = 0; i < input.Length; i++)
            {
                input[i] = i;
            }

            var grouped = input
                .AsQuery()
                .ToLookup(value => new BadHashKey { Value = value % 80 }, Allocator.Temp);

            Assert.That(grouped.GroupCount, Is.EqualTo(80));
            Assert.That(grouped.ValueCount, Is.EqualTo(160));

            for (var i = 0; i < grouped.GroupCount; i++)
            {
                Assert.That(grouped[i].Key.Value, Is.EqualTo(i));
                Assert.That(grouped[i].Length, Is.EqualTo(2));
                Assert.That(grouped[i][0], Is.EqualTo(i));
                Assert.That(grouped[i][1], Is.EqualTo(i + 80));
            }
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
