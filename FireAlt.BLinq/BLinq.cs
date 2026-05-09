using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static class BLinq
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Query<TEnumerator, T> From<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return new Query<TEnumerator, T>(enumerator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Query<CountedQueryEnumerator<TEnumerator, T>, T> From<T, TEnumerator>(TEnumerator enumerator, int length)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return new Query<CountedQueryEnumerator<TEnumerator, T>, T>(
                new CountedQueryEnumerator<TEnumerator, T>(enumerator, length));
        }
    }

    public struct CountedQueryEnumerator<TEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private int _count;

        public CountedQueryEnumerator(TEnumerator source, int count)
        {
            _source = source;
            _count = count < 0 ? -1 : count;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.Current;
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return _source.MoveNext();
        }

        public void Reset()
        {
            _source.Reset();
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _count;
            return count >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            if (!_source.TryGetSpan(out var sourceSpan) ||
                (uint)_count > (uint)sourceSpan.Length)
            {
                span = default;
                return false;
            }

            span = sourceSpan.Slice(0, _count);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetElementAt(int index, out T value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            return _source.TryGetElementAt(index, out value);
        }
    }
    
    public static partial class BLinqExtensions
    {
        internal static void ThrowCodeGen()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
        
        internal static T ThrowCodeGen<T>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }
}
