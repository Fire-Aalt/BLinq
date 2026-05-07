using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _enumerator;
        private int _length;
        private bool _hasKnownLength;

        public Query(TEnumerator enumerator)
            : this(enumerator, -1)
        {
        }

        public Query(TEnumerator enumerator, int length)
        {
            _enumerator = enumerator;
            _length = length >= 0 ? length : 0;
            _hasKnownLength = length >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEnumerator GetEnumerator()
        {
            return _enumerator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetLength(out int length)
        {
            length = _length;
            return _hasKnownLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int KnownLengthOrUnknown(bool known, int length)
        {
            return known && length >= 0 ? length : -1;
        }
    }
}
