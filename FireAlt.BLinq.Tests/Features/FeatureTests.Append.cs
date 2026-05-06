using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Append_ReturnsSourceThenElement()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Append(4).ToArray();
            var actual = input.AsQuery().Append(4).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void Append_ReturnsElementWhenSourceIsEmpty()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.Append(4).ToArray();
            var actual = input.AsQuery().Append(4).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
