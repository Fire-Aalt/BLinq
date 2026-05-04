using System.Collections.Generic;
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
    public class OrderByBenchmark : IBenchmark
    {
        public string Name => "OrderBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLinqZLinqBLinq(int elementCount)
        {
            BenchmarkRunner.Run<OrderByBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .Where(Where)
                .OrderBy(Key)
                .Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values
                .AsValueEnumerable()
                .Where(Where)
                .OrderBy(Key)
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values
                .AsQuery()
                .Where(Where)
                .ToOrderedBy(new KeyComparer(), Allocator.Temp)
                .AsQuery()
                .Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool Where(int value)
        {
            return value % 3 != 1;
        }

        private static int Key(int value)
        {
            return ((value * 73) ^ (value >> 3)) & 4095;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }

        private struct KeyComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return Key(x).CompareTo(Key(y));
            }
        }
    }
}
