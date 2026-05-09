using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void LengthMetadata_TerminalsUseKnownLengthWithoutEnumerating()
        {
            var query = new Query<ThrowingEnumerator, int>(new ThrowingEnumerator(3));

            Assert.That(query.Count(), Is.EqualTo(3));
            Assert.That(query.LongCount(), Is.EqualTo(3));
            Assert.That(query.Any(), Is.True);
            Assert.That(query.ElementAtOrDefault(3), Is.EqualTo(0));
        }

        [Test]
        public void LengthMetadata_PreservingOperatorsKeepKnownLength()
        {
            var query = new Query<ThrowingEnumerator, int>(new ThrowingEnumerator(3));
            var other = new Query<ThrowingEnumerator, int>(new ThrowingEnumerator(2));

            Assert.That(query.Select(new IdentitySelector<int>()).Count(), Is.EqualTo(3));
            Assert.That(query.Take(2).Count(), Is.EqualTo(2));
            Assert.That(query.Skip(2).Count(), Is.EqualTo(1));
            Assert.That(query.Append(4).Count(), Is.EqualTo(4));
            Assert.That(query.Prepend(0).Count(), Is.EqualTo(4));
            Assert.That(query.Concat(other).Count(), Is.EqualTo(5));
        }

        [Test]
        public void LengthMetadata_FilteringOperatorsInvalidateKnownLength()
        {
            var query = new Query<ThrowingEnumerator, int>(new ThrowingEnumerator(3));

            Assert.Throws<InvalidOperationException>(() => query.Where(new TruePredicate()).Count());
        }

        [Test]
        public void LengthMetadata_SequenceEqualUsesDifferentKnownLengths()
        {
            var left = new Query<ThrowingEnumerator, int>(new ThrowingEnumerator(3));
            var right = new Query<ThrowingEnumerator, int>(new ThrowingEnumerator(2));

            Assert.That(BLinqExtensions.SequenceEqual(left, right), Is.False);
        }

        private struct ThrowingEnumerator : IQueryEnumerator<int>
        {
            private int _count;

            public ThrowingEnumerator(int count)
            {
                _count = count;
            }

            public int Current => throw new InvalidOperationException("Enumerator should not be read.");

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                throw new InvalidOperationException("Enumerator should not be advanced.");
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
            }

            public bool TryGetNonEnumeratedCount(out int count)
            {
                count = _count;
                return true;
            }

            public bool TryGetSpan(out ReadOnlySpan<int> span)
            {
                span = default;
                return false;
            }

            public bool TryGetElementAt(int index, out int value)
            {
                value = default;
                return false;
            }
        }

        private struct TruePredicate : IPredicate<int>
        {
            public bool Match(in int value)
            {
                return true;
            }
        }
    }
}
