using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Contains_UsesNativeEquality()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expectedContainsTwo = input.Contains(2);
            var expectedContainsFour = input.Contains(4);

            Assert.That(input.AsQuery().Contains(2), Is.EqualTo(expectedContainsTwo));
            Assert.That(input.AsQuery().Contains(4), Is.EqualTo(expectedContainsFour));
        }
    }
}
