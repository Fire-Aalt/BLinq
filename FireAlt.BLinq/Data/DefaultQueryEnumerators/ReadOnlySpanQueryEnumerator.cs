using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public unsafe struct ReadOnlySpanQueryEnumerator<T> : IQueryEnumerator<T>
        where T : unmanaged
    {
        private readonly T* _ptr;
        private readonly int _length;
        private int _index;

        public ReadOnlySpanQueryEnumerator(void* ptr, int length)
        {
            _ptr = (T*)ptr;
            _length = length;
            _index = -1;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ptr[_index];
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var nextIndex = _index + 1;
            if (nextIndex >= _length)
            {
                return false;
            }

            _index = nextIndex;
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = new ReadOnlySpan<T>(_ptr, _length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetElementAt(int index, out T value)
        {
            if ((uint)index >= (uint)_length)
            {
                value = default;
                return false;
            }

            value = _ptr[index];
            return true;
        }
    }

    public static partial class BLinqExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Query<ReadOnlySpanQueryEnumerator<T>, T> AsQuery<T>(this Span<T> span)
            where T : unmanaged
        {
            fixed (T* ptr = span)
            {
                return new Query<ReadOnlySpanQueryEnumerator<T>, T>(
                    new ReadOnlySpanQueryEnumerator<T>(ptr, span.Length));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Query<ReadOnlySpanQueryEnumerator<T>, T> AsQuery<T>(this ReadOnlySpan<T> span)
            where T : unmanaged
        {
            fixed (T* ptr = span)
            {
                return new Query<ReadOnlySpanQueryEnumerator<T>, T>(
                    new ReadOnlySpanQueryEnumerator<T>(ptr, span.Length));
            }
        }
    }
}
