using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void MinAndMax_ReturnExtremes()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Temp);

            Assert.That(input.AsQuery().Min(), Is.EqualTo(1));
            Assert.That(input.AsQuery().Max(), Is.EqualTo(3));
        }
    }
}
