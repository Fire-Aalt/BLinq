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
    public class JoinLeftBenchmark : IBenchmark
    {
        public string Name => "JoinLeft";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<JoinLeftBenchmark>(elementCount, BLinq, BLinqBurst, true);
        }

        public int Linq(in NativeArray<int> values)
        {
            throw new System.Exception();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable()
                .LeftJoin(values.AsValueEnumerable(), Key, Key, Result)
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery()
                .JoinLeft(values.AsQuery(), Key, Key, Result)
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

        private static int Result(LeftJoinGroup group, int inner)
        {
            return Result(group.Outer, inner);
        }

        private static int Select(int value)
        {
            return (value & 255) - 128;
        }

        private readonly struct LeftJoinGroup
        {
            public readonly int Outer;
            public readonly int[] Inner;

            public LeftJoinGroup(int outer, int[] inner)
            {
                Outer = outer;
                Inner = inner;
            }
        }
    }
}
