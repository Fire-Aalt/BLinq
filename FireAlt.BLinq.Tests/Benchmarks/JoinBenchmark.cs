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
    public class JoinBenchmark : IBenchmark
    {
        public string Name => "Join";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<JoinBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .Join(values, Key, Key, Result)
                .Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable()
                .Join(values.AsValueEnumerable(), Key, Key, Result)
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery()
                .Join(values.AsQuery(), Key, Key, Result)
                .Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int Key(int value)
        {
            return value & 255;
        }

        private static int Result(int outer, int inner)
        {
            return outer + inner;
        }

        private static int Select(int value)
        {
            return (value & 255) - 128;
        }
    }
}
