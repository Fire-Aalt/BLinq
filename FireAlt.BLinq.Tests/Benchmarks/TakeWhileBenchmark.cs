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
    public class TakeWhileBenchmark : IBenchmark
    {
        public string Name => "TakeWhile";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<TakeWhileBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.TakeWhile(TakePrefix).Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().TakeWhile(TakePrefix).Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().TakeWhile(TakePrefix).Sum(Select);
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool TakePrefix(int value)
        {
            return value < 128;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
