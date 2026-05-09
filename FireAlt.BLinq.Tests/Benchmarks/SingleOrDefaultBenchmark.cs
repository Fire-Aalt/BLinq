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
    public class SingleOrDefaultBenchmark : IBenchmark
    {
        public string Name => "SingleOrDefault";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SingleOrDefaultBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.SingleOrDefault(NoMatch);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().SingleOrDefault(NoMatch);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().SingleOrDefault(NoMatch);
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool SingleMatch(int value)
        {
            return value == 512;
        }

        private static bool NoMatch(int value)
        {
            return value < 0;
        }
    }
}
