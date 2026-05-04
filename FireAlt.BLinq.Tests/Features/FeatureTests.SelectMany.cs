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

            Assert.That(flattened.Length, Is.EqualTo(4));
            Assert.That(flattened[0], Is.EqualTo(1));
            Assert.That(flattened[1], Is.EqualTo(11));
            Assert.That(flattened[2], Is.EqualTo(3));
            Assert.That(flattened[3], Is.EqualTo(13));
        }
    }
}
