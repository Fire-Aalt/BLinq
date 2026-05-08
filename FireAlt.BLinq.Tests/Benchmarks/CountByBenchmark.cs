using System.Collections.Generic;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile]
    public class CountByBenchmark : IBenchmark
    {
        public string Name => "CountBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<CountByBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            var counts = new Dictionary<int, int>();
            for (var i = 0; i < values.Length; i++)
            {
                var key = Key(values[i]);
                counts.TryGetValue(key, out var count);
                counts[key] = count + 1;
            }

            var sum = 0;
            foreach (var pair in counts)
            {
                sum += (pair.Key + 1) * pair.Value;
            }

            return sum;
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().CountBy(Key).Sum(g => (g.Key + 1) * g.Value);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().CountBy(Key).Sum(g => (g.Key + 1) * g.Value);
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
    }
}
