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
    public class UnionBenchmark : IBenchmark
    {
        public string Name => "Union";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<UnionBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.Union(values).Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable()
                .Union(values.AsValueEnumerable())
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery()
                .Union(values.AsQuery())
                .Sum(Select);
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
