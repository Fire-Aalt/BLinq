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
    public class FirstBenchmark : IBenchmark
    {
        public string Name => "First";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<FirstBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.First(FirstPredicate);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().First(FirstPredicate);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().First(FirstPredicate);
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool FirstPredicate(int value)
        {
            return value == 997;
        }
    }
}
