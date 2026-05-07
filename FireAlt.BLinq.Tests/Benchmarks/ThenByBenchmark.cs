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
    public class ThenByBenchmark : IBenchmark
    {
        public string Name => "ThenBy";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ThenByBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return WeightedSum(values.OrderBy(PrimaryKey).ThenBy(SecondaryKey).ToArray());
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return WeightedSum(values.AsValueEnumerable().OrderBy(PrimaryKey).ThenBy(SecondaryKey).ToArray());
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return WeightedSum(values.AsQuery().OrderBy(PrimaryKey).ThenBy(SecondaryKey).ToNativeList(Allocator.Temp));
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
        
        private static int PrimaryKey(int value)
        {
            return value & 15;
        }

        private static int SecondaryKey(int value)
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
