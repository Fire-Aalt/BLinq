using System;
using System.Collections.Generic;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Compares the current query against another enumerator element by element.
        /// </summary>
        /// <param name="other">The enumerator to compare against.</param>
        /// <param name="comparer">The equality comparer used for value comparison.</param>
        /// <returns>
        /// <c>true</c> when both sequences have the same length and the same values in the same order; otherwise <c>false</c>.
        /// </returns>
        public bool SequenceEqual<TOtherEnumerator, TEqualityComparer>(TOtherEnumerator other, TEqualityComparer comparer)
            where TOtherEnumerator : unmanaged, IEnumerator<T>
            where TEqualityComparer : unmanaged, INativeEqualityComparer<T>
        {
            var left = GetEnumerator();
            var right = other;
            while (true)
            {
                var leftHasValue = left.MoveNext();
                var rightHasValue = right.MoveNext();
                if (leftHasValue != rightHasValue)
                {
                    left.Dispose();
                    right.Dispose();
                    return false;
                }

                if (!leftHasValue)
                {
                    left.Dispose();
                    right.Dispose();
                    return true;
                }

                var leftValue = left.Current;
                var rightValue = right.Current;
                if (!comparer.Equals(in leftValue, in rightValue))
                {
                    left.Dispose();
                    right.Dispose();
                    return false;
                }
            }
        }
    }
    
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Compares two queries element by element and returns whether they contain the same sequence of values.
        /// </summary>
        /// <param name="source">The left-hand query.</param>
        /// <param name="other">The right-hand query.</param>
        /// <returns>
        /// <c>true</c> when both queries have the same length and the same values in the same order; otherwise <c>false</c>.
        /// </returns>
        public static bool SequenceEqual<T, TEnumerator, TOtherEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, T> other)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TOtherEnumerator : unmanaged, IEnumerator<T>
        {
            return source.SequenceEqual(other.GetEnumerator(), new NativeEqualityComparer<T>());
        }
    }
}
