namespace FireAlt.BLinq.Generators
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using FireAlt.BLinq.Generators.Templates;
    using Microsoft.CodeAnalysis;

    [Generator]
    public sealed class QueryExtensionGenerator : IIncrementalGenerator
    {
        private const string ATTRIBUTE_METADATA_NAME = "FireAlt.BLinq.GenerateQueryExtensionForAttribute";
        private const string ENUMERATOR_METADATA_NAME = "System.Collections.Generic.IEnumerator<T>";

        private static readonly SymbolDisplayFormat FullyQualifiedFormat = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var queryExtensions = context.CompilationProvider
                .Select((compilation, _) => GetQueryExtensions(compilation));

            context.RegisterSourceOutput(queryExtensions, (sourceProductionContext, extensionData) =>
            {
                if (extensionData.Length == 0)
                {
                    return;
                }

                sourceProductionContext.AddSource("BLinq.QueryCollectionExtensions.g.cs", GenerateSource(extensionData));
            });
        }

        private static ImmutableArray<QueryExtensionData> GetQueryExtensions(Compilation compilation)
        {
            var attributes = compilation.Assembly.GetAttributes();
            var queryExtensions = ImmutableArray.CreateBuilder<QueryExtensionData>();
            var emittedExtensions = new HashSet<string>();

            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass?.ToDisplayString() != ATTRIBUTE_METADATA_NAME)
                {
                    continue;
                }

                if ((attribute.ConstructorArguments.Length != 2 && attribute.ConstructorArguments.Length != 3) ||
                    attribute.ConstructorArguments[0].Value is not INamedTypeSymbol collectionType ||
                    attribute.ConstructorArguments[1].Value is not INamedTypeSymbol enumeratorType)
                {
                    continue;
                }

                var itemType = GetItemType(enumeratorType);
                if (itemType == null)
                {
                    continue;
                }

                var collectionTypeName = GetTypeName(collectionType);
                var enumeratorTypeName = GetTypeName(enumeratorType);
                var key = $"{collectionTypeName}|{enumeratorTypeName}";
                if (!emittedExtensions.Add(key))
                {
                    continue;
                }

                var typeParameters = GetTypeParameters(collectionType, enumeratorType, itemType);
                var typeParameterNames = typeParameters.Select(parameter => parameter.Name).ToImmutableArray();
                var unmanagedTypeParameterNames = GetTypeParameterNames(itemType);
                var constraintClauses = typeParameters
                    .Select(parameter => GetConstraintClause(parameter, unmanagedTypeParameterNames))
                    .Where(clause => clause.Length != 0)
                    .ToImmutableArray();

                queryExtensions.Add(new QueryExtensionData(
                    collectionTypeName,
                    enumeratorTypeName,
                    GetTypeName(itemType),
                    "collection.GetEnumerator()",
                    typeParameterNames,
                    constraintClauses));
            }

            return queryExtensions.ToImmutable();
        }

        private static string GenerateSource(ImmutableArray<QueryExtensionData> queryExtensions)
        {
            return new QueryCollectionExtensionsTemplate(queryExtensions).TransformText();
        }

        private static ITypeSymbol? GetItemType(INamedTypeSymbol enumeratorType)
        {
            return GetItemTypeFromInterfaces(enumeratorType) ?? GetItemTypeFromInterfaces(enumeratorType.OriginalDefinition);
        }

        private static ITypeSymbol? GetItemTypeFromInterfaces(INamedTypeSymbol enumeratorType)
        {
            foreach (var interfaceType in enumeratorType.AllInterfaces)
            {
                if (interfaceType.OriginalDefinition.ToDisplayString() == ENUMERATOR_METADATA_NAME &&
                    interfaceType.TypeArguments.Length == 1)
                {
                    return interfaceType.TypeArguments[0];
                }
            }

            return null;
        }

        private static ImmutableArray<ITypeParameterSymbol> GetTypeParameters(
            INamedTypeSymbol collectionType,
            INamedTypeSymbol enumeratorType,
            ITypeSymbol itemType)
        {
            var typeParameters = ImmutableArray.CreateBuilder<ITypeParameterSymbol>();
            var emittedNames = new HashSet<string>();

            AddTypeParameters(collectionType, typeParameters, emittedNames);
            AddTypeParameters(enumeratorType, typeParameters, emittedNames);
            AddTypeParameters(itemType, typeParameters, emittedNames);

            return typeParameters.ToImmutable();
        }

        private static void AddTypeParameters(
            ITypeSymbol type,
            ImmutableArray<ITypeParameterSymbol>.Builder typeParameters,
            HashSet<string> emittedNames)
        {
            if (type is ITypeParameterSymbol typeParameter)
            {
                if (emittedNames.Add(typeParameter.Name))
                {
                    typeParameters.Add(typeParameter);
                }

                return;
            }

            if (type is not INamedTypeSymbol namedType)
            {
                return;
            }

            if (namedType.ContainingType != null)
            {
                AddTypeParameters(namedType.ContainingType, typeParameters, emittedNames);
            }

            if (namedType.IsUnboundGenericType)
            {
                foreach (var typeParameterSymbol in namedType.TypeParameters)
                {
                    AddTypeParameters(typeParameterSymbol, typeParameters, emittedNames);
                }

                return;
            }

            foreach (var typeArgument in namedType.TypeArguments)
            {
                AddTypeParameters(typeArgument, typeParameters, emittedNames);
            }
        }

        private static ISet<string> GetTypeParameterNames(ITypeSymbol type)
        {
            var typeParameterNames = new HashSet<string>();
            AddTypeParameterNames(type, typeParameterNames);
            return typeParameterNames;
        }

        private static void AddTypeParameterNames(ITypeSymbol type, ISet<string> typeParameterNames)
        {
            if (type is ITypeParameterSymbol typeParameter)
            {
                typeParameterNames.Add(typeParameter.Name);
                return;
            }

            if (type is not INamedTypeSymbol namedType)
            {
                return;
            }

            if (namedType.ContainingType != null)
            {
                AddTypeParameterNames(namedType.ContainingType, typeParameterNames);
            }

            foreach (var typeArgument in namedType.TypeArguments)
            {
                AddTypeParameterNames(typeArgument, typeParameterNames);
            }
        }

        private static string GetConstraintClause(ITypeParameterSymbol typeParameter, ISet<string> unmanagedTypeParameterNames)
        {
            var constraints = new List<string>();
            var requiresUnmanaged = unmanagedTypeParameterNames.Contains(typeParameter.Name);

            if (requiresUnmanaged || typeParameter.HasUnmanagedTypeConstraint)
            {
                constraints.Add("unmanaged");
            }
            else if (typeParameter.HasValueTypeConstraint)
            {
                constraints.Add("struct");
            }
            else if (typeParameter.HasReferenceTypeConstraint)
            {
                constraints.Add(typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                    ? "class?"
                    : "class");
            }

            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                var constraintTypeName = GetTypeName(constraintType);
                if (!constraints.Contains(constraintTypeName))
                {
                    constraints.Add(constraintTypeName);
                }
            }

            if (typeParameter.HasConstructorConstraint && !typeParameter.HasValueTypeConstraint && !requiresUnmanaged)
            {
                constraints.Add("new()");
            }

            return constraints.Count == 0
                ? string.Empty
                : $"where {typeParameter.Name} : {string.Join(", ", constraints)}";
        }

        private static string GetTypeName(ITypeSymbol type)
        {
            if (type is ITypeParameterSymbol typeParameter)
            {
                return typeParameter.Name;
            }

            if (type is INamedTypeSymbol namedType)
            {
                return GetNamedTypeName(namedType);
            }

            return type.ToDisplayString(FullyQualifiedFormat);
        }

        private static string GetNamedTypeName(INamedTypeSymbol type)
        {
            var name = type.ContainingType == null
                ? GetNamespacePrefix(type) + type.Name
                : GetNamedTypeName(type.ContainingType) + "." + type.Name;

            var typeArguments = GetTypeArguments(type);
            if (typeArguments.Length == 0)
            {
                return name;
            }

            return $"{name}<{string.Join(", ", typeArguments.Select(GetTypeName))}>";
        }

        private static ImmutableArray<ITypeSymbol> GetTypeArguments(INamedTypeSymbol type)
        {
            if (type.TypeParameters.Length == 0)
            {
                return ImmutableArray<ITypeSymbol>.Empty;
            }

            if (type.IsUnboundGenericType)
            {
                return type.TypeParameters.Cast<ITypeSymbol>().ToImmutableArray();
            }

            return type.TypeArguments;
        }

        private static string GetNamespacePrefix(INamedTypeSymbol type)
        {
            var namespaceName = type.ContainingNamespace?.ToDisplayString();
            return string.IsNullOrEmpty(namespaceName) || namespaceName == "<global namespace>"
                ? "global::"
                : $"global::{namespaceName}.";
        }
    }
}
