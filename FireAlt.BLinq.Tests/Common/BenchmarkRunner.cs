using NUnit.Framework;
using Unity.Collections;
using Unity.PerformanceTesting;

namespace FireAlt.BLinq.Tests.Benchmarks
{
    internal static class BenchmarkRunner
    {
        private const int WARMUP_RUNS = 2;
        private const int MEASURE_RUNS = 10;

        public static void Run<TBenchmark>(
            int elementCount,
            BenchmarkQueryDelegate bLinqQuery,
            BenchmarkQueryDelegate burstQuery)
            where TBenchmark : IBenchmark, new()
        {
            var benchmark = new TBenchmark();

            MeasureNative($"LINQ.{benchmark.Name}/{elementCount}", elementCount, benchmark.Linq, benchmark.Linq);
            MeasureNative($"ZLinq.{benchmark.Name}/{elementCount}", elementCount, benchmark.ZLinq, benchmark.Linq);
            MeasureNative($"BLinq.NoBurst.{benchmark.Name}/{elementCount}", elementCount, bLinqQuery, benchmark.Linq);
            MeasureNative($"BLinq.Burst.{benchmark.Name}/{elementCount}", elementCount, burstQuery, benchmark.Linq);
        }

        private static void MeasureNative(
            string sampleGroupName,
            int elementCount,
            BenchmarkQueryDelegate query,
            BenchmarkQueryDelegate expectedQuery)
        {
            var values = default(NativeArray<int>);
            var expected = 0;
            var result = 0;

            Measure.Method(() => result = query(in values))
                .SetUp(() =>
                {
                    values = CreateInput(elementCount);
                    expected = expectedQuery(in values);
                })
                .CleanUp(() =>
                {
                    Assert.That(result, Is.EqualTo(expected));
                })
                .WarmupCount(WARMUP_RUNS)
                .MeasurementCount(MEASURE_RUNS)
                .SampleGroup(new SampleGroup(sampleGroupName, SampleUnit.Microsecond))
                .Run();
        }

        private static NativeArray<int> CreateInput(int elementCount)
        {
            var values = new NativeArray<int>(elementCount, Allocator.Temp);
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = i;
            }

            return values;
        }
    }
}
