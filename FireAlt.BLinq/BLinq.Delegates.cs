using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public struct DelegateWhereQuery<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public T Current => Throw<T>();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return Throw<bool>();
        }

        public void Reset()
        {
            Throw();
        }

        public void Dispose()
        {
        }

        private static void Throw()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }

        private static TResult Throw<TResult>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }

    public struct DelegateSelectQuery<TEnumerator, TSource, TResult> : IEnumerator<TResult>
        where TEnumerator : unmanaged, IEnumerator<TSource>
        where TSource : unmanaged
        where TResult : unmanaged
    {
        public TResult Current => Throw<TResult>();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return Throw<bool>();
        }

        public void Reset()
        {
            Throw();
        }

        public void Dispose()
        {
        }

        private static void Throw()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }

        private static T Throw<T>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }

    public static partial class BLinqExtensions
    {
        private static T Throw<T>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }
}
