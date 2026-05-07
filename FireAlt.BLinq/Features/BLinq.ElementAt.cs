using System;
using System.Collections.Generic;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to return.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the sequence.</exception>
        public T ElementAt(int index)
        {
            if (TryElementAt(index, out var value))
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Returns the element at <paramref name="index"/>, or the default value when the index is out of range.
        /// </summary>
        /// <param name="index">The zero-based index of the element to return.</param>
        /// <returns>The element at the specified index, or default when the index is out of range.</returns>
        public T ElementAtOrDefault(int index)
        {
            return TryElementAt(index, out var value) ? value : default;
        }

        private bool TryElementAt(int index, out T value)
        {
            if (index < 0 || TryGetLength(out var length) && index >= length)
            {
                value = default;
                return false;
            }

            return BLinqUtilities.TryElementAt<T, TEnumerator>(GetEnumerator(), index, out value);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="index">The zero-based index of the element to return.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the sequence.</exception>
        public static T ElementAt<T, TEnumerator>(this Query<TEnumerator, T> source, int index)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ElementAt(index);
        }

        /// <summary>
        /// Returns the element at <paramref name="index"/>, or the default value when the index is out of range.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="index">The zero-based index of the element to return.</param>
        /// <returns>The element at the specified index, or default when the index is out of range.</returns>
        public static T ElementAtOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source, int index)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ElementAtOrDefault(index);
        }

        /// <summary>
        /// Returns the element at <paramref name="index"/> in the ordered query.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="index">The zero-based index of the element to return.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the sequence.</exception>
        public static T ElementAt<T, TEnumerator, TComparer>(this OrderedQuery<TEnumerator, T, TComparer> source, int index)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            if (index < 0 || source.TryGetLength(out var length) && index >= length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (BLinqUtilities.TryElementAt<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>>(
                    source.GetEnumerator(),
                    index,
                    out var value))
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Returns the element at <paramref name="index"/> in the ordered query, or the default value when the index is out of range.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="index">The zero-based index of the element to return.</param>
        /// <returns>The element at the specified index, or default when the index is out of range.</returns>
        public static T ElementAtOrDefault<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            int index)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            if (index < 0 || source.TryGetLength(out var length) && index >= length)
            {
                return default;
            }

            return BLinqUtilities.TryElementAt<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>>(
                source.GetEnumerator(),
                index,
                out var value)
                ? value
                : default;
        }
    }

    internal static partial class BLinqUtilities
    {
        public static bool TryElementAt<T, TEnumerator>(TEnumerator enumerator, int index, out T value)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var currentIndex = 0;
            while (enumerator.MoveNext())
            {
                if (currentIndex == index)
                {
                    value = enumerator.Current;
                    enumerator.Dispose();
                    return true;
                }

                currentIndex++;
            }

            value = default;
            enumerator.Dispose();
            return false;
        }
    }
}
