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
    public class ElementAtOrDefaultBenchmark : IBenchmark
    {
        public string Name => "ElementAtOrDefault";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ElementAtOrDefaultBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.ElementAtOrDefault(values.Length / 4);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().ElementAtOrDefault(values.Length / 4);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().ElementAtOrDefault(values.Length / 4);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
    }
}
