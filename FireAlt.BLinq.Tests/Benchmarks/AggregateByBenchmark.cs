using System.Collections.Generic;
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
        public void CompareLINQs(int elementCount)
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
                .AggregateBy(Key, _ => 0, Aggregator)
                .Sum(AggregateSelector);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values
                .AsQuery()
                .AggregateBy(Key, _ => 0, Aggregator)
                .Sum(AggregateSelector);
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
        
        private static int Aggregator(int aggregate, int value)
        {
            return aggregate + Select(value);
        }
        
        private static int AggregateSelector(KeyValuePair<int, int> value)
        {
            return (value.Key + 1) * value.Value;
        }
    }
}
