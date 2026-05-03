using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public struct DescendingComparer<T> : IComparer<T>
        where T : unmanaged, IComparable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y)
        {
            return y.CompareTo(x);
        }
    }
}