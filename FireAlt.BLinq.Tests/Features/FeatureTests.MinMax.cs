using System.Linq;
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
            var expectedMin = input.Min();
            var expectedMax = input.Max();

            Assert.That(input.AsQuery().Min(), Is.EqualTo(expectedMin));
            Assert.That(input.AsQuery().Max(), Is.EqualTo(expectedMax));
        }
    }
}
