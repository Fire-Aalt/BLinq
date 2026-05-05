using System.Linq;
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
            var expected = input.Where(value => value > 1).ToArray();
            var filtered = input
                .AsQuery()
                .Where(value => value > 1)
                .ToNativeList(Allocator.Temp);

            AssertSequence(filtered.AsArray(), expected);
        }

        [Test]
        public void Where_EnumeratesFilteredValuesInOrder()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3 }, Allocator.Temp);
            var expected = input.Where(value => value > 1).Sum();

            var sum = 0;
            foreach (var value in input.AsQuery().Where(value => value > 1))
            {
                sum += value;
            }

            Assert.That(sum, Is.EqualTo(expected));
        }
    }
}
