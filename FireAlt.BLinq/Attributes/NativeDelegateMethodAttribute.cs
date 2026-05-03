using System;

namespace FireAlt.BLinq
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NativeDelegateMethodAttribute : Attribute
    {
        public NativeDelegateMethodAttribute(params Type[] nativeDelegateInterfaceTypes)
        {
            if (nativeDelegateInterfaceTypes == null || nativeDelegateInterfaceTypes.Length == 0)
            {
                throw new ArgumentException("At least one native delegate interface type is required.", nameof(nativeDelegateInterfaceTypes));
            }

            NativeDelegateInterfaceTypes = nativeDelegateInterfaceTypes;
        }

        public Type NativeDelegateInterfaceType => NativeDelegateInterfaceTypes[0];

        public Type[] NativeDelegateInterfaceTypes { get; }
    }
}