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
    public class MaxByBenchmark : IBenchmark
    {
        public string Name => "MaxBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<MaxByBenchmark>(elementCount, BLinq, BLinqBurst, true);
        }

        public int Linq(in NativeArray<int> values)
        {
            throw new System.Exception();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().MaxBy(Key);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().MaxBy(Key);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int Key(int value)
        {
            return (value * 31) & 1023;
        }
    }
}
