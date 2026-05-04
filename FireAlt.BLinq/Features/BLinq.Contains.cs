using System;
using System.Collections.Generic;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public bool Contains<TEqualityComparer>(T value, TEqualityComparer comparer)
            where TEqualityComparer : unmanaged, INativeEqualityComparer<T>
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (comparer.Equals(in current, in value))
                {
                    enumerator.Dispose();
                    return true;
                }
            }

            enumerator.Dispose();
            return false;
        }
    }
    
    public static partial class BLinqExtensions
    {
        public static bool Contains<T, TEnumerator>(this Query<TEnumerator, T> source, T value)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Contains(value, new NativeEqualityComparer<T>());
        }
    }
}
