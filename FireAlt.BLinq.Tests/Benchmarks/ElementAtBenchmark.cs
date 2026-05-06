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
    public class ElementAtBenchmark : IBenchmark
    {
        public string Name => "ElementAt";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ElementAtBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Where(Where).ElementAt(values.Length / 4)
                + values.Where(Where).ElementAtOrDefault(values.Length);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Where(Where).ElementAt(values.Length / 4)
                + values.AsValueEnumerable().Where(Where).ElementAtOrDefault(values.Length);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Where(Where).ElementAt(values.Length / 4)
                + values.AsQuery().Where(Where).ElementAtOrDefault(values.Length);
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
    }
}
