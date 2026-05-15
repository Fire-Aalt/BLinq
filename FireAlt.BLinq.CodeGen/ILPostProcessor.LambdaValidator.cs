using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace FireAlt.BLinq.CodeGen
{
    internal sealed partial class ILPostProcessor
    {
        private IReadOnlyList<FieldDefinition> GetCapturedFields(
            MethodDefinition lambda,
            List<DiagnosticMessage> diagnostics,
            MethodDefinition owner,
            Instruction diagnosticInstruction)
        {
            if (lambda.IsStatic)
            {
                return Array.Empty<FieldDefinition>();
            }

            var declaringType = lambda.DeclaringType;
            if (!IsCompilerGeneratedLambdaContainer(declaringType))
            {
                if (IsUnmanaged(declaringType))
                {
                    return Array.Empty<FieldDefinition>();
                }

                AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate target '{lambda.FullName}' is not a compiler-generated lambda.");
                return null;
            }

            var fields = new List<FieldDefinition>();
            foreach (var field in declaringType.Fields)
            {
                if (!field.IsStatic)
                {
                    fields.Add(field);
                }
            }

            foreach (var field in fields)
            {
                if (!IsUnmanaged(field.FieldType))
                {
                    AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate capture '{field.Name}' has managed type '{field.FieldType.FullName}'.");
                    return null;
                }
            }

            return fields;
        }

        private static bool IsCompilerGeneratedLambdaContainer(TypeDefinition type)
        {
            return type.Name.StartsWith("<>c", StringComparison.Ordinal);
        }

        private bool ValidateLambdaBodyUsesOnlyUnmanagedTypes(
            MethodDefinition lambda,
            IReadOnlyList<FieldDefinition> capturedFields,
            List<DiagnosticMessage> diagnostics,
            MethodDefinition owner,
            Instruction diagnosticInstruction)
        {
            foreach (var parameter in lambda.Parameters)
            {
                if (!IsUnmanaged(parameter.ParameterType))
                {
                    AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate parameter '{parameter.Name}' has managed type '{parameter.ParameterType.FullName}'.");
                    return false;
                }
            }

            if (lambda.ReturnType.MetadataType != MetadataType.Void &&
                !IsUnmanaged(lambda.ReturnType))
            {
                AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate return type '{lambda.ReturnType.FullName}' is managed.");
                return false;
            }

            foreach (var variable in lambda.Body.Variables)
            {
                if (!IsUnmanaged(variable.VariableType))
                {
                    AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate local '{lambda.Body.Variables.IndexOf(variable)}' has managed type '{variable.VariableType.FullName}'.");
                    return false;
                }
            }

            foreach (var handler in lambda.Body.ExceptionHandlers)
            {
                if (handler.CatchType != null)
                {
                    AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate body cannot catch managed exception type '{handler.CatchType.FullName}'.");
                    return false;
                }
            }

            var capturedFieldNames = new HashSet<string>();
            foreach (var field in capturedFields)
            {
                capturedFieldNames.Add(field.FullName);
            }
            foreach (var instruction in lambda.Body.Instructions)
            {
                if (TryGetManagedTypeUsage(instruction, capturedFieldNames, out var message))
                {
                    AddError(diagnostics, lambda, instruction, owner, diagnosticInstruction, message);
                    return false;
                }
            }

            return true;
        }

        private bool TryGetManagedTypeUsage(
            Instruction instruction,
            ISet<string> capturedFieldNames,
            out string message)
        {
            message = null;

            switch (instruction.OpCode.Code)
            {
                case Code.Ldstr:
                    message = "BLinq delegate body cannot use string literals.";
                    return true;
                case Code.Newarr:
                    if (instruction.Operand is TypeReference arrayElementType)
                    {
                        message = $"BLinq delegate body cannot create managed array '{arrayElementType.FullName}[]'.";
                    }
                    else
                    {
                        message = "BLinq delegate body cannot create managed arrays.";
                    }

                    return true;
                case Code.Box:
                    if (instruction.Operand is TypeReference boxedType)
                    {
                        message = $"BLinq delegate body cannot box '{boxedType.FullName}'.";
                    }
                    else
                    {
                        message = "BLinq delegate body cannot box values.";
                    }

                    return true;
            }

            if (instruction.OpCode == OpCodes.Newobj &&
                instruction.Operand is MethodReference constructor)
            {
                if (IsFuncOrActionDelegateType(constructor.DeclaringType))
                {
                    return false;
                }

                if (!IsUnmanaged(constructor.DeclaringType))
                {
                    message = $"BLinq delegate body cannot create managed type '{constructor.DeclaringType.FullName}'.";
                    return true;
                }
            }

            if (instruction.Operand is FieldReference fieldReference)
            {
                var fieldName = fieldReference.FullName;
                var fieldType = CloseFieldReferenceType(fieldReference.FieldType, fieldReference.DeclaringType);
                if (IsFuncOrActionDelegateType(fieldType))
                {
                    return false;
                }

                if (!capturedFieldNames.Contains(fieldName) &&
                    !IsUnmanaged(fieldType))
                {
                    message = $"BLinq delegate body cannot use managed field type '{fieldType.FullName}'.";
                    return true;
                }

                if (TryGetManagedGenericArgument(fieldReference.DeclaringType, out var managedDeclaringGenericArgument))
                {
                    message = $"BLinq delegate body cannot use managed generic type '{managedDeclaringGenericArgument.FullName}'.";
                    return true;
                }

                return false;
            }

            if (instruction.Operand is MethodReference methodReference)
            {
                var isNativeDelegateMethod = IsPotentialNativeDelegateMethodReference(methodReference) &&
                    HasNativeDelegateMethodAttribute(ResolveMethod(methodReference));
                if (methodReference.HasThis && !IsUnmanaged(methodReference.DeclaringType))
                {
                    message = $"BLinq delegate body cannot call instance method on managed type '{methodReference.DeclaringType.FullName}'.";
                    return true;
                }

                var returnType = CloseMethodReferenceType(methodReference.ReturnType, methodReference);
                if (returnType.MetadataType != MetadataType.Void &&
                    !IsUnmanaged(returnType))
                {
                    message = $"BLinq delegate body cannot use managed return type '{returnType.FullName}'.";
                    return true;
                }

                foreach (var parameter in methodReference.Parameters)
                {
                    var parameterType = CloseMethodReferenceType(parameter.ParameterType, methodReference);
                    if (isNativeDelegateMethod && IsFuncOrActionDelegateType(parameterType))
                    {
                        continue;
                    }

                    if (!IsUnmanaged(parameterType))
                    {
                        message = $"BLinq delegate body cannot call method '{methodReference.FullName}' because parameter type '{parameterType.FullName}' is managed.";
                        return true;
                    }
                }

                if (methodReference is GenericInstanceMethod genericMethod)
                {
                    foreach (var genericArgument in genericMethod.GenericArguments)
                    {
                        if (!IsUnmanaged(genericArgument))
                        {
                            message = $"BLinq delegate body cannot use managed generic argument '{genericArgument.FullName}'.";
                            return true;
                        }
                    }
                }

                if (TryGetManagedGenericArgument(methodReference.DeclaringType, out var managedMethodDeclaringGenericArgument))
                {
                    message = $"BLinq delegate body cannot use managed generic type '{managedMethodDeclaringGenericArgument.FullName}'.";
                    return true;
                }

                return false;
            }

            if (instruction.Operand is CallSite callSite)
            {
                if (callSite.ReturnType.MetadataType != MetadataType.Void &&
                    !IsUnmanaged(callSite.ReturnType))
                {
                    message = $"BLinq delegate body cannot use managed return type '{callSite.ReturnType.FullName}'.";
                    return true;
                }

                foreach (var parameter in callSite.Parameters)
                {
                    if (!IsUnmanaged(parameter.ParameterType))
                    {
                        message = $"BLinq delegate body cannot use managed calli parameter type '{parameter.ParameterType.FullName}'.";
                        return true;
                    }
                }

                return false;
            }

            if (instruction.Operand is TypeReference typeReference &&
                IsManagedTypeOperand(instruction.OpCode.Code, typeReference))
            {
                message = $"BLinq delegate body cannot use managed type '{typeReference.FullName}'.";
                return true;
            }

            return false;
        }

        private bool IsManagedTypeOperand(Code opcode, TypeReference typeReference)
        {
            switch (opcode)
            {
                case Code.Castclass:
                case Code.Isinst:
                case Code.Ldtoken:
                case Code.Unbox:
                case Code.Unbox_Any:
                case Code.Cpobj:
                case Code.Initobj:
                case Code.Ldobj:
                case Code.Stobj:
                case Code.Mkrefany:
                    return !IsUnmanaged(typeReference);
                default:
                    return false;
            }
        }

        private bool TryGetManagedGenericArgument(TypeReference type, out TypeReference managedType)
        {
            managedType = null;
            switch (type)
            {
                case null:
                    return false;
                case GenericInstanceType genericInstance:
                    foreach (var genericArgument in genericInstance.GenericArguments)
                    {
                        if (!IsUnmanaged(genericArgument))
                        {
                            managedType = genericArgument;
                            return true;
                        }

                        if (TryGetManagedGenericArgument(genericArgument, out managedType))
                        {
                            return true;
                        }
                    }

                    return false;
                case ByReferenceType byReference:
                    return TryGetManagedGenericArgument(byReference.ElementType, out managedType);
                case PointerType pointer:
                    return TryGetManagedGenericArgument(pointer.ElementType, out managedType);
                case RequiredModifierType requiredModifier:
                    return TryGetManagedGenericArgument(requiredModifier.ElementType, out managedType);
                case OptionalModifierType optionalModifier:
                    return TryGetManagedGenericArgument(optionalModifier.ElementType, out managedType);
                default:
                    return false;
            }
        }

        private bool IsUnmanaged(TypeReference type)
        {
            if (type == null)
            {
                return false;
            }

            if (_unmanagedTypeCache.TryGetValue(type.FullName, out var cached))
            {
                return cached;
            }

            var unmanaged = IsUnmanaged(type, new HashSet<string>());
            _unmanagedTypeCache[type.FullName] = unmanaged;
            return unmanaged;
        }

        private bool IsUnmanaged(TypeReference type, ISet<string> visited)
        {
            switch (type)
            {
                case null:
                    return false;
                case RequiredModifierType requiredModifier:
                    return IsUnmanaged(requiredModifier.ElementType, visited);
                case OptionalModifierType optionalModifier:
                    return IsUnmanaged(optionalModifier.ElementType, visited);
                case ByReferenceType byReference:
                    return IsUnmanaged(byReference.ElementType, visited);
                case PointerType:
                    return true;
                case ArrayType:
                    return false;
                case GenericParameter genericParameter:
                    return genericParameter.HasNotNullableValueTypeConstraint;
            }

            var closedType = type;
            var elementType = type.GetElementType();
            if (elementType.MetadataType == MetadataType.Void || elementType.IsPrimitive)
            {
                return true;
            }

            if (elementType.MetadataType == MetadataType.IntPtr || elementType.MetadataType == MetadataType.UIntPtr)
            {
                return true;
            }

            var definition = ResolveType(elementType);
            if (definition == null || !definition.IsValueType)
            {
                return false;
            }

            if (definition.IsEnum)
            {
                return true;
            }

            var visitKey = closedType.FullName;
            if (!visited.Add(visitKey))
            {
                return true;
            }

            try
            {
                foreach (var field in definition.Fields)
                {
                    if (!field.IsStatic &&
                        !IsUnmanaged(CloseTypeGenericType(closedType, field.FieldType), visited))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                visited.Remove(visitKey);
            }
        }

        private static TypeReference CloseTypeGenericType(TypeReference declaringType, TypeReference fieldType)
        {
            var declaringInstance = declaringType as GenericInstanceType;
            return RewriteTypeReference(
                fieldType,
                genericParameter => genericParameter.Type == GenericParameterType.Type && declaringInstance != null
                    ? declaringInstance.GenericArguments[genericParameter.Position]
                    : null,
                typeReference => typeReference);
        }

        private static TypeReference CloseMethodReferenceType(TypeReference type, MethodReference methodReference)
        {
            var declaringInstance = methodReference.DeclaringType as GenericInstanceType;
            var methodInstance = methodReference as GenericInstanceMethod;

            return RewriteTypeReference(
                type,
                genericParameter =>
                {
                    if (genericParameter.Type == GenericParameterType.Type &&
                        declaringInstance != null)
                    {
                        return declaringInstance.GenericArguments[genericParameter.Position];
                    }

                    if (genericParameter.Type == GenericParameterType.Method &&
                        methodInstance != null)
                    {
                        return methodInstance.GenericArguments[genericParameter.Position];
                    }

                    return null;
                },
                typeReference => typeReference);
        }

        private static TypeReference CloseFieldReferenceType(TypeReference type, TypeReference declaringType)
        {
            var declaringInstance = declaringType as GenericInstanceType;
            return RewriteTypeReference(
                type,
                genericParameter => genericParameter.Type == GenericParameterType.Type && declaringInstance != null
                    ? declaringInstance.GenericArguments[genericParameter.Position]
                    : null,
                typeReference => typeReference);
        }
    }
}
