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
    public class ContainsBenchmark : IBenchmark
    {
        public string Name => "Contains";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ContainsBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return (values.Contains(values.Length - 1) ? 1 : 0) + (values.Contains(-1) ? 2 : 0);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return (values.AsValueEnumerable().Contains(values.Length - 1) ? 1 : 0)
                + (values.AsValueEnumerable().Contains(-1) ? 2 : 0);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return (values.AsQuery().Contains(values.Length - 1) ? 1 : 0)
                + (values.AsQuery().Contains(-1) ? 2 : 0);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
    }
}
