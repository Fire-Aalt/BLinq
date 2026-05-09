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
    public class JoinRightBenchmark : IBenchmark
    {
        public string Name => "JoinRight";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<JoinRightBenchmark>(elementCount, BLinq, BLinqBurst, true);
        }

        public int Linq(in NativeArray<int> values)
        {
            throw new System.Exception();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable()
                .RightJoin(values.AsValueEnumerable(), Key, Key, Result)
                .Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery()
                .JoinRight(values.AsQuery(), Key, Key, Result)
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

        private static int Result(RightJoinGroup group, int outer)
        {
            return Result(outer, group.Inner);
        }

        private static int Select(int value)
        {
            return (value & 255) - 128;
        }

        private readonly struct RightJoinGroup
        {
            public readonly int Inner;
            public readonly int[] Outer;

            public RightJoinGroup(int inner, int[] outer)
            {
                Inner = inner;
                Outer = outer;
            }
        }
    }
}
