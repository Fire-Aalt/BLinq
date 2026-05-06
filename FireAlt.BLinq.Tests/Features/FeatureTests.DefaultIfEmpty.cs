using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void DefaultIfEmpty_ReturnsSourceWhenSourceHasElements()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.DefaultIfEmpty().ToArray();
            var actual = input.AsQuery().DefaultIfEmpty().ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void DefaultIfEmpty_ReturnsDefaultForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.DefaultIfEmpty().ToArray();
            var actual = input.AsQuery().DefaultIfEmpty().ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }

        [Test]
        public void DefaultIfEmpty_ReturnsProvidedDefaultForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.DefaultIfEmpty(42).ToArray();
            var actual = input.AsQuery().DefaultIfEmpty(42).ToNativeArray(Allocator.Temp);

            AssertSequence(actual, expected);
        }
    }
}
