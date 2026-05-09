using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile]
    public class IndexBenchmark : IBenchmark
    {
        public string Name => "Index";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<IndexBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            var i = 0;
            var sum = 0;

            var enumerator = values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var tuple = (i, values[i]);
                sum += Select(tuple.i);
                i++;
            }
            enumerator.Dispose();
            return sum;
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().Index().Sum(tuple => Select(tuple.Index));
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values.AsQuery().Index().Sum(tuple => Select(tuple.Index));
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
