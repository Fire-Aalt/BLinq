using System;
using System.Linq;
using Mono.Cecil;

namespace FireAlt.BLinq.CodeGen
{
    internal sealed partial class ILPostProcessor
    {
        private const string TupleElementNamesAttributeTypeName = "System.Runtime.CompilerServices.TupleElementNamesAttribute";

        private static TypeReference RewriteTypeReference(
            TypeReference type,
            Func<GenericParameter, TypeReference> resolveGenericParameter,
            Func<TypeReference, TypeReference> mapReference)
        {
            if (type is GenericParameter genericParameter)
            {
                var resolved = resolveGenericParameter(genericParameter);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            switch (type)
            {
                case GenericInstanceType genericInstance:
                    var closedInstance = new GenericInstanceType(mapReference(genericInstance.ElementType));
                    foreach (var argument in genericInstance.GenericArguments)
                    {
                        closedInstance.GenericArguments.Add(RewriteTypeReference(argument, resolveGenericParameter, mapReference));
                    }

                    return closedInstance;
                case ByReferenceType byReference:
                    return new ByReferenceType(RewriteTypeReference(byReference.ElementType, resolveGenericParameter, mapReference));
                case PointerType pointer:
                    return new PointerType(RewriteTypeReference(pointer.ElementType, resolveGenericParameter, mapReference));
                case RequiredModifierType requiredModifier:
                    return new RequiredModifierType(
                        mapReference(requiredModifier.ModifierType),
                        RewriteTypeReference(requiredModifier.ElementType, resolveGenericParameter, mapReference));
                case OptionalModifierType optionalModifier:
                    return new OptionalModifierType(
                        mapReference(optionalModifier.ModifierType),
                        RewriteTypeReference(optionalModifier.ElementType, resolveGenericParameter, mapReference));
                default:
                    return mapReference(type);
            }
        }

        private static void CopyTupleElementNamesAttributes(
            ICustomAttributeProvider source,
            ICustomAttributeProvider target,
            ModuleDefinition targetModule)
        {
            if (source == null || target == null || !source.HasCustomAttributes)
            {
                return;
            }

            foreach (var attribute in source.CustomAttributes.Where(attribute =>
                attribute.AttributeType.FullName == TupleElementNamesAttributeTypeName))
            {
                var copied = new CustomAttribute(targetModule.ImportReference(attribute.Constructor));
                foreach (var argument in attribute.ConstructorArguments)
                {
                    copied.ConstructorArguments.Add(ImportCustomAttributeArgument(targetModule, argument));
                }

                foreach (var argument in attribute.Fields)
                {
                    copied.Fields.Add(new CustomAttributeNamedArgument(
                        argument.Name,
                        ImportCustomAttributeArgument(targetModule, argument.Argument)));
                }

                foreach (var argument in attribute.Properties)
                {
                    copied.Properties.Add(new CustomAttributeNamedArgument(
                        argument.Name,
                        ImportCustomAttributeArgument(targetModule, argument.Argument)));
                }

                target.CustomAttributes.Add(copied);
            }
        }

        private static CustomAttributeArgument ImportCustomAttributeArgument(
            ModuleDefinition module,
            CustomAttributeArgument argument)
        {
            var type = module.ImportReference(argument.Type);
            switch (argument.Value)
            {
                case CustomAttributeArgument[] values:
                    return new CustomAttributeArgument(
                        type,
                        values.Select(value => ImportCustomAttributeArgument(module, value)).ToArray());
                case TypeReference typeReference:
                    return new CustomAttributeArgument(type, module.ImportReference(typeReference));
                default:
                    return new CustomAttributeArgument(type, argument.Value);
            }
        }
    }
}
