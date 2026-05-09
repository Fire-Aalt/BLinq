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
    public class AllBenchmark : IBenchmark
    {
        public string Name => "All";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<AllBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.All(AllPredicate) ? 1 : 0;
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().All(AllPredicate) ? 1 : 0;
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().All(AllPredicate) ? 1 : 0;
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool AllPredicate(int value)
        {
            return value >= 0;
        }
    }
}
