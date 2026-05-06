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
        /// Returns the only element of the query, or throws if the query is empty or contains more than one element.
        /// </summary>
        /// <returns>The only element in the sequence.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements or more than one element.</exception>
        public T Single()
        {
            return BLinqUtilities.Single<T, TEnumerator>(GetEnumerator());
        }

        /// <summary>
        /// Returns the only element of the query, or the default value if the query is empty.
        /// </summary>
        /// <returns>The only element in the sequence, or default when the sequence is empty.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains more than one element.</exception>
        public T SingleOrDefault()
        {
            return BLinqUtilities.SingleOrDefault<T, TEnumerator>(GetEnumerator());
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the only element of the query, or throws if the query is empty or contains more than one element.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The only element in the sequence.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements or more than one element.</exception>
        public static T Single<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Single();
        }

        /// <summary>
        /// Returns the only element that matches <paramref name="predicate"/>, or throws if no element or more than one element matches.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The only matching element in the sequence.</returns>
        /// <exception cref="InvalidOperationException">No element or more than one element matches <paramref name="predicate"/>.</exception>
        public static T Single<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.Single<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate);
        }

        /// <summary>
        /// Returns the only element of the query, or the default value if the query is empty.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The only element in the sequence, or default when the sequence is empty.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains more than one element.</exception>
        public static T SingleOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.SingleOrDefault();
        }

        /// <summary>
        /// Returns the only element that matches <paramref name="predicate"/>, or the default value if none match.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The only matching element, or default when no matching element exists.</returns>
        /// <exception cref="InvalidOperationException">More than one element matches <paramref name="predicate"/>.</exception>
        public static T SingleOrDefault<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.SingleOrDefault<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate);
        }

        /// <summary>
        /// Returns the only element that matches <paramref name="predicate"/>, or throws if no element or more than one element matches.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The only matching element in the sequence.</returns>
        /// <exception cref="InvalidOperationException">No element or more than one element matches <paramref name="predicate"/>.</exception>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T Single<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }

        /// <summary>
        /// Returns the only element that matches <paramref name="predicate"/>, or the default value if none match.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The only matching element, or default when no matching element exists.</returns>
        /// <exception cref="InvalidOperationException">More than one element matches <paramref name="predicate"/>.</exception>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T SingleOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static T Single<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var value = enumerator.Current;
            if (enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains more than one element.");
            }

            enumerator.Dispose();
            return value;
        }

        public static T Single<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (TrySingleMatching<T, TEnumerator, TPredicate>(enumerator, predicate, out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no matching elements.");
        }

        public static T SingleOrDefault<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                return default;
            }

            var value = enumerator.Current;
            if (enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains more than one element.");
            }

            enumerator.Dispose();
            return value;
        }

        public static T SingleOrDefault<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return TrySingleMatching<T, TEnumerator, TPredicate>(enumerator, predicate, out var value) ? value : default;
        }

        private static bool TrySingleMatching<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate, out T value)
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

                if (found)
                {
                    enumerator.Dispose();
                    throw new InvalidOperationException("The BLinq source contains more than one matching element.");
                }

                value = current;
                found = true;
            }

            enumerator.Dispose();
            return found;
        }
    }
}
