namespace FireAlt.BLinq.Generators
{
    using System.Collections.Immutable;

    internal readonly struct QueryExtensionData
    {
        public QueryExtensionData(
            string collectionTypeName,
            string enumeratorTypeName,
            string itemTypeName,
            string enumeratorExpression,
            string lengthExpression,
            ImmutableArray<string> typeParameterNames,
            ImmutableArray<string> constraintClauses)
        {
            CollectionTypeName = collectionTypeName;
            EnumeratorTypeName = enumeratorTypeName;
            ItemTypeName = itemTypeName;
            EnumeratorExpression = enumeratorExpression;
            LengthExpression = lengthExpression;
            TypeParameterNames = typeParameterNames;
            ConstraintClauses = constraintClauses;
        }

        public string CollectionTypeName { get; }

        public string EnumeratorTypeName { get; }

        public string ItemTypeName { get; }

        public string EnumeratorExpression { get; }

        public string LengthExpression { get; }

        public ImmutableArray<string> TypeParameterNames { get; }

        public ImmutableArray<string> ConstraintClauses { get; }
    }
}
