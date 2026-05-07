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
    public class IntersectByBenchmark : IBenchmark
    {
        public string Name => "IntersectBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<IntersectByBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            var keys = values.Select(Key).ToHashSet();
            return values
                .GroupBy(Key)
                .Select(group => group.First())
                .Where(value => keys.Contains(Key(value)))
                .Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable()
                .IntersectBy(values.AsValueEnumerable().Select(Key), Key)
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery()
                .IntersectBy(values.AsQuery().Select(Key), Key)
                .Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
        
        private static int Key(int value)
        {
            return value & 1023;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
