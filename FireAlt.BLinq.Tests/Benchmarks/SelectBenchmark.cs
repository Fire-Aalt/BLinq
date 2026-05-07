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
    public class SelectBenchmark : IBenchmark
    {
        public string Name => "Select";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SelectBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Select(SelectValue).Sum();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Select(SelectValue).Sum();
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Select(SelectValue).Sum();
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int SelectValue(int value)
        {
            return (value & 255) + 1;
        }
    }
}
