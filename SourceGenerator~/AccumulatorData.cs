namespace FireAlt.BLinq.Generators
{
    internal readonly struct AccumulatorData
    {
        public AccumulatorData(string fullTypeName, string structName, string countExpression)
        {
            FullTypeName = fullTypeName;
            StructName = structName;
            CountExpression = countExpression;
        }

        public string FullTypeName { get; }

        public string StructName { get; }

        public string CountExpression { get; }
    }
}
