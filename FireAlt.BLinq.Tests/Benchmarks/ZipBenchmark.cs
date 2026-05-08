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
    public class ZipBenchmark : IBenchmark
    {
        public string Name => "Zip";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ZipBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Zip(values, ZipResult).Sum();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Zip(values.AsValueEnumerable(), ZipResult).Sum();
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Zip(values.AsQuery(), ZipResult).Sum();
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int ZipResult(int left, int right)
        {
            return (left + right) & 255;
        }
    }
}
