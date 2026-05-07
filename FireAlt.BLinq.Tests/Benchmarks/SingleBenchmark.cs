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
    public class SingleBenchmark : IBenchmark
    {
        public string Name => "Single";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SingleBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Single(SinglePredicate);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Single(SinglePredicate);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Single(SinglePredicate);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool SinglePredicate(int value)
        {
            return value == 997;
        }
    }
}
