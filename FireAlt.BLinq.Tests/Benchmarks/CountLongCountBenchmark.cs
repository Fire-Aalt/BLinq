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
    public class CountLongCountBenchmark : IBenchmark
    {
        public string Name => "CountLongCount";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<CountLongCountBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Count(CountPredicate) + (int)values.LongCount(LongCountPredicate);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Count(CountPredicate)
                + (int)values.AsValueEnumerable().LongCount(LongCountPredicate);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Count(CountPredicate)
                + (int)values.AsQuery().LongCount(LongCountPredicate);
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

        private static bool LongCountPredicate(int value)
        {
            return value % 3 == 0;
        }
    }
}
