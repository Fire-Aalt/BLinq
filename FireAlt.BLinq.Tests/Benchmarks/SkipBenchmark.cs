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
    public class SkipBenchmark : IBenchmark
    {
        public string Name => "Skip";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SkipBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Skip(values.Length / 4).Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Skip(values.Length / 4).Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Skip(values.Length / 4).Sum(Select);
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
