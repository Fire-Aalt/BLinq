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
            var linqGroup = $"LINQ.{benchmark.Name}/{elementCount}";
            var zLinqGroup = $"ZLinq.{benchmark.Name}/{elementCount}";
            var bLinqGroup = $"BLinq.NoBurst.{benchmark.Name}/{elementCount}";
            var burstGroup = $"BLinq.Burst.{benchmark.Name}/{elementCount}";

            MeasureNative(linqGroup, elementCount, benchmark.Linq, benchmark.Linq);
            MeasureNative(zLinqGroup, elementCount, benchmark.ZLinq, benchmark.Linq);
            MeasureNative(bLinqGroup, elementCount, bLinqQuery, benchmark.Linq);
            MeasureNative(burstGroup, elementCount, burstQuery, benchmark.Linq);
            ReportRelativePerformance(
                benchmark.Name,
                elementCount,
                linqGroup,
                zLinqGroup,
                bLinqGroup,
                burstGroup);
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
                    if (values.IsCreated)
                    {
                        values.Dispose();
                    }
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

        private static void ReportRelativePerformance(
            string benchmarkName,
            int elementCount,
            string linqGroup,
            string zLinqGroup,
            string bLinqGroup,
            string burstGroup)
        {
            var linqMean = Mean(linqGroup);
            var zLinqSpeedup = Speedup(linqMean, Mean(zLinqGroup));
            var bLinqSpeedup = Speedup(linqMean, Mean(bLinqGroup));
            var burstSpeedup = Speedup(linqMean, Mean(burstGroup));

            Measure.Custom(new SampleGroup($"Relative.ZLinq.{benchmarkName}/{elementCount}", SampleUnit.Undefined, true), zLinqSpeedup);
            Measure.Custom(new SampleGroup($"Relative.BLinq.NoBurst.{benchmarkName}/{elementCount}", SampleUnit.Undefined, true), bLinqSpeedup);
            Measure.Custom(new SampleGroup($"Relative.BLinq.Burst.{benchmarkName}/{elementCount}", SampleUnit.Undefined, true), burstSpeedup);

            TestContext.Out.WriteLine("\n");
            TestContext.Out.WriteLine($"{benchmarkName}.Benchmark @ {elementCount} elements | LINQ x1.00 | ZLinq x{zLinqSpeedup:0.00} | BLinq.NoBurst x{bLinqSpeedup:0.00} | BLinq.Burst x{burstSpeedup:0.00}");
            TestContext.Out.WriteLine("\n");
        }

        private static double Speedup(double baselineMean, double variantMean)
        {
            return variantMean <= 0.0 ? 0.0 : baselineMean / variantMean;
        }

        private static double Mean(string sampleGroupName)
        {
            var samples = PerformanceTest.GetSampleGroup(sampleGroupName).Samples;
            var sum = 0.0;

            for (var i = 0; i < samples.Count; i++)
            {
                sum += samples[i];
            }

            return samples.Count == 0 ? 0.0 : sum / samples.Count;
        }
    }
}
