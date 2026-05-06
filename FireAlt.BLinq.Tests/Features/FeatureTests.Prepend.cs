using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Prepend_ReturnsElementThenSource()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Prepend(0).ToArray();
            var actual = input.AsQuery().Prepend(0).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Prepend_ReturnsElementWhenSourceIsEmpty()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.Prepend(0).ToArray();
            var actual = input.AsQuery().Prepend(0).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
