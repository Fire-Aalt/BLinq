using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.PerformanceTesting;
using ZLinq;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests.Benchmarks
{
    [BurstCompile]
    public class SelectManyBenchmark : IBenchmark
    {
        public string Name => "SelectMany";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SelectManyBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            return values.SelectMany(LinqInnerValues).Sum(Select);
        }

        public int ZLinq(in NativeArray<int> values)
        {
            return values.AsValueEnumerable().SelectMany(LinqInnerValues).Sum(Select);
        }

        public static int BLinq(in NativeArray<int> values)
        {
            return values
                .AsQuery()
                .SelectMany(BLinqInnerValues)
                .Sum(Select);
        }

        [BurstCompile]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
        
        private static int[] LinqInnerValues(int value)
        {
            return new[] { value, value + 3 };
        }

        [MustDisposeResource]
        private static FixedList32Bytes<int>.Enumerator BLinqInnerValues(int value)
        {
            var list = new FixedList32Bytes<int> { value, value + 3 };
            return list.GetEnumerator();
        }

        private static int Select(int value)
        {
            return (value & 255) + 1;
        }
    }
}
