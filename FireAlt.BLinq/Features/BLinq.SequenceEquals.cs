using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        public static bool SequenceEquals<T, TEnumerator, TOtherEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, T> other)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TOtherEnumerator : unmanaged, IEnumerator<T>
        {
            return source.SequenceEquals(other.GetEnumerator(), new NativeEqualityComparer<T>());
        }
    }

    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public bool SequenceEquals<TOtherEnumerator, TEqualityComparer>(TOtherEnumerator other, TEqualityComparer comparer)
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
}
