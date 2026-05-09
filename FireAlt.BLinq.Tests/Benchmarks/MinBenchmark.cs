using System.Linq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile(DisableSafetyChecks = true)]
    public class MinBenchmark : IBenchmark
    {
        public string Name => "Min";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<MinBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Min();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Min();
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Min();
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
    }
}
