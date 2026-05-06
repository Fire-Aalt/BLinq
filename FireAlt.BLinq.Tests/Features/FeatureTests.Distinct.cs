using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Distinct_ReturnsFirstOccurrenceOfEachValue()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 1, 3, 2, 4 }, Allocator.Temp);
            var expected = input.Distinct().ToArray();
            var actual = input.AsQuery().Distinct().ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Distinct_ReturnsEmptyForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.Distinct().ToArray();
            var actual = input.AsQuery().Distinct().ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
