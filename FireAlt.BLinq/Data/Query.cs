using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _enumerator;

        public Query(TEnumerator enumerator)
        {
            _enumerator = enumerator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEnumerator GetEnumerator()
        {
            return _enumerator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetElementAt(int index, out T value)
        {
            return _enumerator.TryGetElementAt(index, out value);
        }
    }

}
