using System.Collections;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace FireAlt.BLinq
{
    public struct NativeArrayQueryEnumerator<T> : IQueryEnumerator<T>
        where T : unmanaged
    {
        private NativeArray<T> _values;
        private NativeArray<T>.Enumerator _enumerator;

        public NativeArrayQueryEnumerator(NativeArray<T> values)
        {
            _values = values;
            _enumerator = values.GetEnumerator();
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerator.Current;
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _values.Length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = new ReadOnlySpan<T>(NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(_values), _values.Length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetElementAt(int index, out T value)
        {
            if ((uint)index >= (uint)_values.Length)
            {
                value = default;
                return false;
            }

            value = _values[index];
            return true;
        }
    }
}
