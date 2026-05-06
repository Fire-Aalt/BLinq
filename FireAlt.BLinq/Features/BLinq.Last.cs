using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the last element of the query, or throws if the query is empty.
        /// </summary>
        /// <returns>The last element in the sequence.</returns>
        public T Last()
        {
            if (BLinqUtilities.TryLast<T, TEnumerator>(GetEnumerator(), out var value))
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
            return BLinqUtilities.TryLast<T, TEnumerator>(GetEnumerator(), out var value) ? value : default;
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
            where TEnumerator : unmanaged, IEnumerator<T>
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
            where TEnumerator : unmanaged, IEnumerator<T>
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
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (BLinqUtilities.TryLast<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate, out var value))
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
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.TryLast<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate, out var value)
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
            where TEnumerator : unmanaged, IEnumerator<T>
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
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static bool TryLast<T, TEnumerator>(TEnumerator enumerator, out T value)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
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
            where TEnumerator : unmanaged, IEnumerator<T>
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
