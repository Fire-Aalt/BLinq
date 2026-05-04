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
    public class SimpleBenchmark : IBenchmark
    {
        public string Name => "Simple";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SimpleBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .Where(SimpleWhere)
                .Sum(SimpleSelect);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values
                .AsValueEnumerable()
                .Where(SimpleWhere)
                .Sum(SimpleSelect);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values
                .AsQuery()
                .Where(SimpleWhere)
                .Sum(SimpleSelect);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool SimpleWhere(int value)
        {
            return (value & 1) == 0;
        }

        private static int SimpleSelect(int value)
        {
            return (value & 1023) + 1;
        }
    }
}
