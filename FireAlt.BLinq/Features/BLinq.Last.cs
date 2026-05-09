using System;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the last element of the query, or throws if the query is empty.
        /// </summary>
        /// <returns>The last element in the sequence.</returns>
        public T Last()
        {
            var enumerator = GetEnumerator();
            if (enumerator.TryGetNonEnumeratedCount(out var length))
            {
                if (length == 0)
                {
                    throw new InvalidOperationException("The BLinq source contains no elements.");
                }

                if (enumerator.TryGetElementAt(length - 1, out var lastValue) ||
                    enumerator.TryGetSpan(out var span) && TryGetLastFromSpan(span, out lastValue) ||
                    BLinqUtilities.TryElementAt<T, TEnumerator>(enumerator, length - 1, out lastValue))
                {
                    return lastValue;
                }
            }

            if (BLinqUtilities.TryLast<T, TEnumerator>(enumerator, out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no elements.");
        }

        /// <summary>
        /// Returns the last element of the query, or the default value if the query is empty.
        /// </summary>
        /// <returns>The last element in the sequence, or default when the sequence is empty.</returns>
        public T LastOrDefault()
        {
            var enumerator = GetEnumerator();
            if (enumerator.TryGetNonEnumeratedCount(out var length))
            {
                if (length == 0)
                {
                    return default;
                }

                return enumerator.TryGetElementAt(length - 1, out var lastValue) ||
                       enumerator.TryGetSpan(out var span) && TryGetLastFromSpan(span, out lastValue) ||
                       BLinqUtilities.TryElementAt<T, TEnumerator>(enumerator, length - 1, out lastValue)
                    ? lastValue
                    : default;
            }

            return BLinqUtilities.TryLast<T, TEnumerator>(enumerator, out var value) ? value : default;
        }

        private static bool TryGetLastFromSpan(ReadOnlySpan<T> span, out T value)
        {
            if (span.Length == 0)
            {
                value = default;
                return false;
            }

            value = span[span.Length - 1];
            return true;
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the last element of the query, or throws if the query is empty.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The last element in the sequence.</returns>
        public static T Last<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Last();
        }

        /// <summary>
        /// Returns the last element of the query, or the default value if the query is empty.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The last element in the sequence, or default when the sequence is empty.</returns>
        public static T LastOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.LastOrDefault();
        }

        /// <summary>
        /// Returns the last element that matches <paramref name="predicate"/>, or throws if no element matches.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The last matching element in the sequence.</returns>
        public static T Last<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            var enumerator = source.GetEnumerator();

            if (enumerator.TryGetNonEnumeratedCount(out var length))
            {
                if (length == 0)
                {
                    throw new InvalidOperationException("The BLinq source contains no elements.");
                }
                
                if (enumerator.TryGetSpan(out var span))
                {
                    for (var i = span.Length - 1; i >= 0; i--)
                    {
                        ref readonly var element = ref span[i];
                        if (predicate.Match(in element))
                        {
                            return element;
                        }
                    }
                }
            }

            if (BLinqUtilities.TryLast<T, TEnumerator, TPredicate>(enumerator, predicate, out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no elements.");
        }

        /// <summary>
        /// Returns the last element that matches <paramref name="predicate"/>, or the default value if none match.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The last matching element, or default when no matching element exists.</returns>
        public static T LastOrDefault<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            var enumerator = source.GetEnumerator();
            if (enumerator.TryGetSpan(out var span))
            {
                for (var i = span.Length - 1; i >= 0; i--)
                {
                    ref readonly var element = ref span[i];
                    if (predicate.Match(in element))
                    {
                        return element;
                    }
                }
            }

            return BLinqUtilities.TryLast<T, TEnumerator, TPredicate>(enumerator, predicate, out var value)
                ? value
                : default;
        }

        /// <summary>
        /// Returns the last element that matches <paramref name="predicate"/>, or throws if no element matches.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The last matching element in the sequence.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T Last<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }

        /// <summary>
        /// Returns the last element that matches <paramref name="predicate"/>, or the default value if none match.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The last matching element, or default when no matching element exists.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T LastOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static bool TryLast<T, TEnumerator>(TEnumerator enumerator, out T value)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            value = default;
            var found = false;
            while (enumerator.MoveNext())
            {
                value = enumerator.Current;
                found = true;
            }

            enumerator.Dispose();
            return found;
        }

        public static bool TryLast<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate, out T value)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            value = default;
            var found = false;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (!predicate.Match(in current))
                {
                    continue;
                }

                value = current;
                found = true;
            }

            enumerator.Dispose();
            return found;
        }
    }
}
