using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile]
    public class AggregateByBenchmark : IBenchmark
    {
        public string Name => "AggregateBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLinqZLinqBLinq(int elementCount)
        {
            BenchmarkRunner.Run<AggregateByBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .GroupBy(Key)
                .Sum(group => (group.Key + 1) * group.Sum(Select));
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values
                .AsValueEnumerable()
                .AggregateBy(
                    Key,
                    _ => 0,
                    (aggregate, value) => aggregate + Select(value))
                .Sum(value => (value.Key + 1) * value.Value);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values
                .AsQuery()
                .AggregateBy(
                    new KeySelector(),
                    0,
                    new SelectAggregator())
                .Sum(new AggregateSelector());
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int Key(int value)
        {
            return value & 15;
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }

        private struct KeySelector : ISelector<int, int>
        {
            public int Select(in int value)
            {
                return Key(value);
            }
        }

        private struct SelectAggregator : IAggregator<int, int>
        {
            public int Aggregate(in int aggregate, in int value)
            {
                return aggregate + Select(value);
            }
        }

        private struct AggregateSelector : ISelector<KeyValuePair<int, int>, int>
        {
            public int Select(in KeyValuePair<int, int> value)
            {
                return (value.Key + 1) * value.Value;
            }
        }
    }
}
