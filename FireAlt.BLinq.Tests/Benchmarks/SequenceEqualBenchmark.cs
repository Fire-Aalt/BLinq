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
    public class SequenceEqualBenchmark : IBenchmark
    {
        public string Name => "SequenceEqual";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SequenceEqualBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.SequenceEqual(values) ? 1 : 0;
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().SequenceEqual(values.AsValueEnumerable()) ? 1 : 0;
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().SequenceEqual(values.AsQuery()) ? 1 : 0;
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
    }
}
