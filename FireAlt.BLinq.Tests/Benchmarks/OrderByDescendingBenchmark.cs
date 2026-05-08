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
    public class OrderByDescendingBenchmark : IBenchmark
    {
        public string Name => "OrderByDescending";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<OrderByDescendingBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return WeightedSum(values.OrderByDescending(Key).ToArray());
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return WeightedSum(values.AsValueEnumerable().OrderByDescending(Key).ToArray());
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return WeightedSum(values.AsQuery().OrderByDescending(Key).ToNativeList(Allocator.Temp));
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int Key(int value)
        {
            return ((value * 73) ^ (value >> 3)) & 4095;
        }

        private static int WeightedSum(int[] values)
        {
            var sum = 0;
            for (var i = 0; i < values.Length; i++)
            {
                sum += ((i + 1) & 255) * ((values[i] & 255) + 1);
            }

            return sum;
        }

        private static int WeightedSum(NativeList<int> values)
        {
            var sum = 0;
            for (var i = 0; i < values.Length; i++)
            {
                sum += ((i + 1) & 255) * ((values[i] & 255) + 1);
            }
            return sum;
        }
    }
}
