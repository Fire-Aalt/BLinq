using System.Linq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile]
    public class GroupJoinBenchmark : IBenchmark
    {
        public string Name => "GroupJoin";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<GroupJoinBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .GroupJoin(values, Key, Key, LinqResult)
                .Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable()
                .GroupJoin(values.AsValueEnumerable(), Key, Key, LinqResult)
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery()
                .GroupJoin(values.AsQuery(), Key, Key, BLinqResult)
                .Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool Outer(int value)
        {
            return (value & 1) == 0;
        }

        private static bool Inner(int value)
        {
            return (value & 2) == 0;
        }

        private static int Key(int value)
        {
            return value & 255;
        }

        private static int LinqResult(int outer, System.Collections.Generic.IEnumerable<int> group)
        {
            return outer + group.Sum();
        }

        private static int BLinqResult(int outer, Group<int, int> group)
        {
            var sum = 0;
            for (var i = 0; i < group.Length; i++)
            {
                sum += group[i];
            }

            return outer + sum;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
