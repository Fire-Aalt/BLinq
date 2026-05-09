using System.Linq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile(DisableSafetyChecks = true)]
    public class DefaultIfEmptyBenchmark : IBenchmark
    {
        public string Name => "DefaultIfEmpty";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<DefaultIfEmptyBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Where(NoMatch).DefaultIfEmpty(7).Sum(Select)
                + values.Where(Where).DefaultIfEmpty(7).Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Where(NoMatch).DefaultIfEmpty(7).Sum(Select)
                + values.AsValueEnumerable().Where(Where).DefaultIfEmpty(7).Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Where(NoMatch).DefaultIfEmpty(7).Sum(Select)
                + values.AsQuery().Where(Where).DefaultIfEmpty(7).Sum(Select);
        }

        [BurstCompile(DisableSafetyChecks = true)]
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

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
