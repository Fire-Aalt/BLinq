using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Collections;

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
