using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Reverse_ReturnsElementsInReverseOrder()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var expected = input.Reverse().ToArray();
            var actual = input.AsQuery().Reverse().ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Reverse_ReturnsEmptyForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.Reverse().ToArray();
            var actual = input.AsQuery().Reverse().ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
