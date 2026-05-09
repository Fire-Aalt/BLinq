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
    public class SequenceEqualBenchmark : IBenchmark
    {
        public string Name => "SequenceEqual";

        [Test]
        [Performance]
        [Explicit("Benchmark test. Run manually.")]
        [Category("Benchmark")]
        [TestCase(1_000)]
        [TestCase(10_000)]
        
        public void CompareLINQs(int elementCount)
        {
            BenchmarkRunner.Run<SequenceEqualBenchmark>(elementCount, BLinq, BLinqBurst);
        }

        public int Linq(in NativeArray<int> values)
        {
            var second = CreateInverseQuery(values);
            return values.SequenceEqual(second) ? 1 : 0;
        }

        public int ZLinq(in NativeArray<int> values)
        {
            var second = CreateInverseQuery(values);
            return values.AsValueEnumerable().SequenceEqual(second.AsValueEnumerable()) ? 1 : 0;
        }

        public static int BLinq(in NativeArray<int> values)
        {
            var second = CreateInverseQuery(values);
            return values.AsQuery().SequenceEqual(second.AsQuery()) ? 1 : 0;
        }

        [BurstCompile(DisableSafetyChecks = true)]
        public static int BLinqBurst(in NativeArray<int> values)
        {
            return BLinq(values);
        }
        
        private static NativeArray<int> CreateInverseQuery(NativeArray<int> values)
        {
            var length = values.Length;
            return values.AsQuery().Select(s =>
            {
                if (s == length - 1)
                {
                    return s + 1;
                }
                return s;
            }).ToNativeList(Allocator.Temp).AsArray();
        }
    }
}
