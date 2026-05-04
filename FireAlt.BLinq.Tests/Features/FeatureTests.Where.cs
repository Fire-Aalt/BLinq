using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Where_ToNativeList_FiltersValues()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3 }, Allocator.Temp);
            var filtered = input
                .AsQuery()
                .Where(value => value > 1)
                .ToNativeList(Allocator.Temp);

            Assert.That(filtered.Length, Is.EqualTo(2));
            Assert.That(filtered[0], Is.EqualTo(2));
            Assert.That(filtered[1], Is.EqualTo(3));
        }

        [Test]
        public void Where_EnumeratesFilteredValuesInOrder()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3 }, Allocator.Temp);

            var sum = 0;
            foreach (var value in input.AsQuery().Where(value => value > 1))
            {
                sum += value;
            }

            Assert.That(sum, Is.EqualTo(5));
        }
    }
}
