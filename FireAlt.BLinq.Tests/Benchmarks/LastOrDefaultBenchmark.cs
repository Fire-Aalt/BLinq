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
    public class LastOrDefaultBenchmark : IBenchmark
    {
        public string Name => "LastOrDefault";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<LastOrDefaultBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.LastOrDefault(Where)
                   + values.LastOrDefault(NoMatch);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().LastOrDefault(Where)
                + values.AsValueEnumerable().LastOrDefault(NoMatch);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().LastOrDefault(Where)
                + values.AsQuery().LastOrDefault(NoMatch);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool Where(int value)
        {
            return (value & 1) == 0;
        }

        private static bool NoMatch(int value)
        {
            return value < 0;
        }
    }
}
