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
    public class ExceptByBenchmark : IBenchmark
    {
        public string Name => "ExceptBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ExceptByBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            var keys = values.ToHashSet();
            return values.Where(Left)
                .GroupBy(ShiftedKey)
                .Select(group => group.First())
                .Where(value => !keys.Contains(ShiftedKey(value)))
                .Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            var keys = values.ToHashSet();
            return values.AsValueEnumerable().Where(Left)
                .GroupBy(ShiftedKey)
                .Select(group => group.First())
                .Where(value => !keys.Contains(ShiftedKey(value)))
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Where(Left)
                .ExceptBy(values.AsQuery(), ShiftedKey)
                .Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool Left(int value)
        {
            return (value & 1) == 0;
        }

        private static int ShiftedKey(int value)
        {
            return value + 1_000_000;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
