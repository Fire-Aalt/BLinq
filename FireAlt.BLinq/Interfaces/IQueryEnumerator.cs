using System;
using System.Collections.Generic;

namespace FireAlt.BLinq
{
    public interface IQueryEnumerator<T> : IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the length when processing time is not necessary.
        /// Always returns true if TryGetSpan returns true.
        /// </summary>
        bool TryGetNonEnumeratedCount(out int count);

        /// <summary>
        /// Returns true if it can return a Span.
        /// Used for SIMD and loop processing optimizations.
        /// </summary>
        bool TryGetSpan(out ReadOnlySpan<T> span);

        bool TryGetElementAt(int index, out T value);
    }
}
