using System;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns whether the query contains at least one element.
        /// </summary>
        /// <returns><c>true</c> when the sequence contains at least one element; otherwise <c>false</c>.</returns>
        public bool Any()
        {
            var enumerator = GetEnumerator();
            if (enumerator.TryGetNonEnumeratedCount(out var length))
            {
                return length != 0;
            }

            return BLinqUtilities.Any<T, TEnumerator>(enumerator);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns whether the query contains at least one element.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns><c>true</c> when the sequence contains at least one element; otherwise <c>false</c>.</returns>
        public static bool Any<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Any();
        }

        /// <summary>
        /// Returns whether any element in the query matches <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns><c>true</c> when any element matches; otherwise <c>false</c>.</returns>
        public static bool Any<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            var enumerator = source.GetEnumerator();
            if (enumerator.TryGetNonEnumeratedCount(out var length) && length == 0)
            {
                return false;
            }

            return BLinqUtilities.Any<T, TEnumerator, TPredicate>(enumerator, predicate);
        }

        /// <summary>
        /// Returns whether every element in the query matches <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns><c>true</c> when every element matches, or when the sequence is empty; otherwise <c>false</c>.</returns>
        public static bool All<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            var enumerator = source.GetEnumerator();
            if (enumerator.TryGetNonEnumeratedCount(out var length) && length == 0)
            {
                return true;
            }

            return BLinqUtilities.All<T, TEnumerator, TPredicate>(enumerator, predicate);
        }

        /// <summary>
        /// Returns whether any element in the query matches <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns><c>true</c> when any element matches; otherwise <c>false</c>.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Any<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<bool>();
        }

        /// <summary>
        /// Returns whether every element in the query matches <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns><c>true</c> when every element matches, or when the sequence is empty; otherwise <c>false</c>.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool All<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<bool>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static bool Any<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            if (enumerator.MoveNext())
            {
                enumerator.Dispose();
                return true;
            }

            enumerator.Dispose();
            return false;
        }

        public static bool Any<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (predicate.Match(in value))
                {
                    enumerator.Dispose();
                    return true;
                }
            }

            enumerator.Dispose();
            return false;
        }

        public static bool All<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (!predicate.Match(in value))
                {
                    enumerator.Dispose();
                    return false;
                }
            }

            enumerator.Dispose();
            return true;
        }
    }
}
