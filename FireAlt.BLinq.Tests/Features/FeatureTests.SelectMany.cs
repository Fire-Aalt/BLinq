using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void SelectMany_FlattensEnumerators()
        {
            var input = new NativeArray<int>(new[] { 1, 3 }, Allocator.Temp);
            var expected = input
                .SelectMany(value => new[] { value, value + 10 })
                .ToArray();
            var flattened = input
                .AsQuery()
                .SelectMany<int, int, NativeArray<int>.Enumerator, FixedList32Bytes<int>.Enumerator>(value =>
                {
                    var list = new FixedList32Bytes<int>();
                    list.Add(value);
                    list.Add(value + 10);
                    return list.GetEnumerator();
                })
                .ToNativeList(Allocator.Temp);

            AssertSequence(flattened.AsArray(), expected);
        }
    }
}
