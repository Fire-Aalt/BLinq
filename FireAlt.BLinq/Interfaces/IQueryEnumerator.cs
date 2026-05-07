using System.Collections.Generic;

namespace FireAlt.BLinq
{
    public interface IQueryEnumerator<T> : IEnumerator<T>
        where T : unmanaged
    {
        bool TryGetElementAt(int index, out T value);
    }
}
