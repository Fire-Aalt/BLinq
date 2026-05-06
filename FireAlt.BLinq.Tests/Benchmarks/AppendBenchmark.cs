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
    public class AppendBenchmark : IBenchmark
    {
        public string Name => "Append";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<AppendBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Where(Include).Append(17).Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Where(Include).Append(17).Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Where(Include).Append(17).Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool Include(int value)
        {
            return (value & 1) == 0;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
