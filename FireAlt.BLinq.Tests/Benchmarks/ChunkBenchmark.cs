using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile]
    public class ChunkBenchmark : IBenchmark
    {
        private const int CHUNK_SIZE = 256;

        public string Name => "Chunk";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ChunkBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            var sum = 0;
            for (var i = 0; i < values.Length; i += CHUNK_SIZE)
            {
                var end = i + CHUNK_SIZE < values.Length ? i + CHUNK_SIZE : values.Length;
                for (var j = i; j < end; j++)
                {
                    sum += Select(values[j]);
                }
            }

            return sum;
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Chunk(CHUNK_SIZE).Sum(c => c.AsValueEnumerable().Sum(Select));
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Chunk(CHUNK_SIZE).Sum(c => c.AsQuery().Sum(Select));
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
