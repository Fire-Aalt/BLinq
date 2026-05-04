using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        public static T Min<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Min(new AscendingComparer<T>());
        }

        public static T Max<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Max(new AscendingComparer<T>());
        }
    }

    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public T Min<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            var enumerator = GetEnumerator();
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var best = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (comparer.Compare(value, best) < 0)
                {
                    best = value;
                }
            }

            enumerator.Dispose();
            return best;
        }

        public T Max<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            var enumerator = GetEnumerator();
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var best = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (comparer.Compare(value, best) > 0)
                {
                    best = value;
                }
            }

            enumerator.Dispose();
            return best;
        }
    }
}
