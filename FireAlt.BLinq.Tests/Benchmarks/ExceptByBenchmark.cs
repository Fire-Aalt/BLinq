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
            var keys = values.Where(Right).Select(Key).ToHashSet();
            return values.Where(Left)
                .GroupBy(Key)
                .Select(group => group.First())
                .Where(value => !keys.Contains(Key(value)))
                .Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            var keys = values.Where(Right).Select(Key).ToHashSet();
            return values.AsValueEnumerable().Where(Left)
                .GroupBy(Key)
                .Select(group => group.First())
                .Where(value => !keys.Contains(Key(value)))
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Where(Left)
                .ExceptBy(values.AsQuery().Where(Right).Select(Key), Key)
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

        private static bool Right(int value)
        {
            return (value & 2) == 0;
        }

        private static int Key(int value)
        {
            return value & 1023;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
