using System.Linq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;
// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile(DisableSafetyChecks = true)]
    public class ComplexBenchmark : IBenchmark
    {
        public string Name => "Complex";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<ComplexBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values
                .Where(Where0)
                .Select(Select0)
                .Where(Where1)
                .Select(Select1)
                .Where(Where2)
                .Select(Select2)
                .Where(Where3)
                .Select(Select3)
                .Where(Where4)
                .Select(Select4)
                .Where(Where5)
                .Select(Select5)
                .Sum();
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values
                .AsValueEnumerable()
                .Where(Where0)
                .Select(Select0)
                .Where(Where1)
                .Select(Select1)
                .Where(Where2)
                .Select(Select2)
                .Where(Where3)
                .Select(Select3)
                .Where(Where4)
                .Select(Select4)
                .Where(Where5)
                .Select(Select5)
                .Sum();
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values
                .AsQuery()
                .Where(Where0)
                .Select(Select0)
                .Where(Where1)
                .Select(Select1)
                .Where(Where2)
                .Select(Select2)
                .Where(Where3)
                .Select(Select3)
                .Where(Where4)
                .Select(Select4)
                .Where(Where5)
                .Select(Select5)
                .Sum();
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }

        private static bool Where0(int value) => (value & 1) == 0;

        private static int Select0(int value) => ((value * 3) + 7) & 4095;

        private static bool Where1(int value) => value % 3 != 1;

        private static int Select1(int value) => (value ^ 0x5A5) & 4095;

        private static bool Where2(int value) => (value & 7) != 0;

        private static int Select2(int value) => ((value * 5) - 11) & 4095;

        private static bool Where3(int value) => value % 5 != 2;

        private static int Select3(int value) => (value + (value >> 1) + 17) & 4095;

        private static bool Where4(int value) => (value & 15) < 12;

        private static int Select4(int value) => ((value * 7) + 3) & 4095;

        private static bool Where5(int value) => value % 11 != 0;

        private static int Select5(int value) => (value & 255) + 1;
    }
}
