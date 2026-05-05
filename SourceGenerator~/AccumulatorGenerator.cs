namespace FireAlt.BLinq.Generators
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using FireAlt.BLinq.Generators.Templates;
    using Microsoft.CodeAnalysis;

    [Generator]
    public sealed class AccumulatorGenerator : IIncrementalGenerator
    {
        private const string ATTRIBUTE_METADATA_NAME = "FireAlt.BLinq.GenerateAccumulatorForAttribute";

        private static readonly SymbolDisplayFormat FullyQualifiedFormat = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var accumulators = context.CompilationProvider
                .Select((compilation, _) => GetAccumulators(compilation));

            context.RegisterSourceOutput(accumulators, (sourceProductionContext, accumulatorData) =>
            {
                foreach (var accumulator in accumulatorData)
                {
                    sourceProductionContext.AddSource($"{accumulator.StructName}.g.cs", GenerateSource(accumulator));
                }
            });
        }

        private static ImmutableArray<AccumulatorData> GetAccumulators(Compilation compilation)
        {
            var attributes = compilation.Assembly.GetAttributes();
            var accumulators = ImmutableArray.CreateBuilder<AccumulatorData>();
            var emittedStructs = new HashSet<string>();

            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass?.ToDisplayString() != ATTRIBUTE_METADATA_NAME)
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length != 2 ||
                    attribute.ConstructorArguments[0].Value is not ITypeSymbol typeSymbol)
                {
                    continue;
                }

                var structName = $"{typeSymbol.Name}Accumulator";
                if (!emittedStructs.Add(structName))
                {
                    continue;
                }

                accumulators.Add(new AccumulatorData(
                    typeSymbol.ToDisplayString(FullyQualifiedFormat),
                    structName,
                    GetCountExpression(attribute.ConstructorArguments[1])));
            }

            return accumulators.ToImmutable();
        }

        private static string GenerateSource(AccumulatorData accumulator)
        {
            return new AccumulatorTemplate(accumulator).TransformText();
        }

        private static string GetCountExpression(TypedConstant divisorType)
        {
            return divisorType.Value is int value && value == 0 ? "(int)count" : "count";
        }
    }
}
