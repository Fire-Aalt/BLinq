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
    public class CountBenchmark : IBenchmark
    {
        public string Name => "Count";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<CountBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Count(CountPredicate) + values.Count();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Count(CountPredicate) + values.AsValueEnumerable().Count();
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Count(CountPredicate) + values.AsQuery().Count();
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool CountPredicate(int value)
        {
            return (value & 1) == 0;
        }
    }
}
