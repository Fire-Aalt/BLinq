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
    public class LongCountBenchmark : IBenchmark
    {
        public string Name => "LongCount";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<LongCountBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return (int)values.LongCount(LongCountPredicate) + (int)values.LongCount();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return (int)values.AsValueEnumerable().LongCount(LongCountPredicate) + (int)values.AsValueEnumerable().LongCount();
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return (int)values.AsQuery().LongCount(LongCountPredicate) + (int)values.AsQuery().LongCount();
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool LongCountPredicate(int value)
        {
            return value % 3 == 0;
        }
    }
}
