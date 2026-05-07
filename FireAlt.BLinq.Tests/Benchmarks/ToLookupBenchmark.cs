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
    public class ToLookupBenchmark : IBenchmark
    {
        public string Name => "ToLookup";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ToLookupBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .ToLookup(Key)
                .Sum(group => (group.Key + 1) * group.Sum(Select));
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values
                .AsValueEnumerable()
                .GroupBy(Key)
                .Sum(group => (group.Key + 1) * group.AsValueEnumerable().Sum(Select));
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return SumLookup(values.AsQuery().ToLookup(Key, Allocator.Temp));
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int SumLookup(Lookup<int, int> lookup)
        {
            var sum = 0;

            foreach (var group in lookup)
            {
                sum += (group.Key + 1) * group.AsQuery().Sum(Select);
            }

            return sum;
        }

        private static int Key(int value)
        {
            return value & 15;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
