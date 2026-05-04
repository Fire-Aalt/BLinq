using System;

namespace FireAlt.BLinq
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class GenerateAccumulatorForAttribute : Attribute
    {
        public GenerateAccumulatorForAttribute(Type type, DivisorType divisorType)
        {
            Type = type;
            DivisorType = divisorType;
        }

        public Type Type { get; }

        public DivisorType DivisorType { get; }
    }

    public enum DivisorType
    {
        Int,
        UInt,
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class GenerateQueryExtensionForAttribute : Attribute
    {
        public GenerateQueryExtensionForAttribute(Type collectionType, Type enumeratorType,
            QueryExtensionEnumeratorSource enumeratorSource = QueryExtensionEnumeratorSource.Collection)
        {
            CollectionType = collectionType;
            EnumeratorType = enumeratorType;
            EnumeratorSource = enumeratorSource;
        }

        public Type CollectionType { get; }

        public Type EnumeratorType { get; }

        public QueryExtensionEnumeratorSource EnumeratorSource { get; }
    }

    public enum QueryExtensionEnumeratorSource
    {
        Collection,
        AsReadOnly,
    }
}
