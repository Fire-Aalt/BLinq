using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static class Feature
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Query<T, TEnumerator> From<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return new Query<T, TEnumerator>(enumerator);
        }
    }
}
