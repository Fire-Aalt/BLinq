using Unity.Collections;

namespace FireAlt.BLinq.Tests.Benchmarks
{
    internal delegate int BenchmarkQueryDelegate(in NativeArray<int> values);

    internal interface IBenchmark
    {
        string Name { get; }

        int Linq(in NativeArray<int> values);

        int ZLinq(in NativeArray<int> values);
    }
}
