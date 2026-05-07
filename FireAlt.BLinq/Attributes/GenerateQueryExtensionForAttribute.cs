using System;

namespace FireAlt.BLinq
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class GenerateQueryExtensionForAttribute : Attribute
    {
        public GenerateQueryExtensionForAttribute(Type collectionType, Type enumeratorType)
        {
            CollectionType = collectionType;
            EnumeratorType = enumeratorType;
        }

        public Type CollectionType { get; }

        public Type EnumeratorType { get; }

        public string LengthProperty { get; set; }
    }
}
