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
    public class FirstOrDefaultBenchmark : IBenchmark
    {
        public string Name => "FirstOrDefault";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<FirstOrDefaultBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.FirstOrDefault(FirstPredicate) + values.FirstOrDefault(NoMatch);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().FirstOrDefault(FirstPredicate)
                + values.AsValueEnumerable().FirstOrDefault(NoMatch);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().FirstOrDefault(FirstPredicate)
                + values.AsQuery().FirstOrDefault(NoMatch);
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

        private static bool NoMatch(int value)
        {
            return value < 0;
        }
    }
}
