using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Select_ProjectsValues()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var mapped = input
                .AsQuery()
                .Select(value => value * 2)
                .ToNativeList(Allocator.Temp);

            Assert.That(mapped.Length, Is.EqualTo(3));
            Assert.That(mapped[0], Is.EqualTo(2));
            Assert.That(mapped[1], Is.EqualTo(4));
            Assert.That(mapped[2], Is.EqualTo(6));
        }
    }
}
