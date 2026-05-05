using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace FireAlt.BLinq.CodeGen
{
    internal sealed partial class ILPostProcessor
    {
        private bool TryRewriteNativeDelegateCall(
            MethodDefinition owner,
            Instruction callInstruction,
            GenericInstanceMethod placeholderCall,
            List<DiagnosticMessage> diagnostics)
        {
            if (!IsPotentialNativeDelegateMethodReference(placeholderCall))
            {
                return false;
            }

            var placeholder = placeholderCall.Resolve();
            if (placeholder == null)
            {
                return false;
            }

            var interfaceDefinitions = GetNativeDelegateInterfaceDefinitions(placeholder);
            if (interfaceDefinitions.Count == 0)
            {
                return false;
            }

            var module = owner.Module;
            var delegateParameters = placeholder.Parameters
                .Where(parameter => IsFuncOrActionDelegateType(CloseMethodGenericType(module, parameter.ParameterType, placeholderCall)))
                .ToArray();

            if (delegateParameters.Length != interfaceDefinitions.Count)
            {
                AddError(diagnostics, owner, callInstruction, $"BLinq delegate method '{placeholder.FullName}' has {interfaceDefinitions.Count} delegate attributes but {delegateParameters.Length} delegate parameters.");
                return false;
            }

            var adapters = new Dictionary<int, AdapterInfo>();
            for (var i = delegateParameters.Length - 1; i >= 0; i--)
            {
                var parameter = delegateParameters[i];
                var trailingArguments = MoveTrailingArgumentsAfterDelegate(owner, callInstruction, placeholder, parameter, diagnostics);
                if (trailingArguments == null)
                {
                    return false;
                }

                var delegateType = CloseMethodGenericType(module, parameter.ParameterType, placeholderCall);
                var signature = ResolveDelegateSignature(module, delegateType, diagnostics, owner, callInstruction);
                if (signature == null)
                {
                    return false;
                }

                var interfaceType = CreateNativeDelegateInterfaceType(module, interfaceDefinitions[i], signature);
                var adapter = CreateAdapter(owner, callInstruction, signature, interfaceType, diagnostics);
                if (adapter == null)
                {
                    return false;
                }

                foreach (var trailingArgument in trailingArguments)
                {
                    owner.Body.GetILProcessor().InsertBefore(callInstruction, trailingArgument);
                }

                adapters.Add(parameter.Index, adapter);
            }

            var target = FindTargetMethod(module, placeholderCall, placeholder, adapters, diagnostics, owner, callInstruction);
            if (target == null)
            {
                return false;
            }

            callInstruction.Operand = target.Call;
            MapRewrittenReturnType(
                CloseMethodGenericType(module, placeholderCall.ReturnType, placeholderCall),
                target.ReturnType);

            return true;
        }

        private IReadOnlyList<TypeReference> GetNativeDelegateInterfaceDefinitions(MethodDefinition method)
        {
            if (_nativeDelegateInterfaceCache.TryGetValue(method.FullName, out var cached))
            {
                return cached;
            }

            var interfaceDefinitions = new List<TypeReference>();
            foreach (var attribute in method.CustomAttributes)
            {
                if (attribute.AttributeType.FullName != NativeDelegateMethodAttributeTypeName ||
                    attribute.ConstructorArguments.Count != 1)
                {
                    continue;
                }

                var argumentValue = attribute.ConstructorArguments[0].Value;
                if (argumentValue is TypeReference interfaceType)
                {
                    interfaceDefinitions.Add(interfaceType);
                }
                else if (argumentValue is CustomAttributeArgument[] interfaceTypes)
                {
                    foreach (var interfaceTypeArgument in interfaceTypes)
                    {
                        if (interfaceTypeArgument.Value is TypeReference arrayInterfaceType)
                        {
                            interfaceDefinitions.Add(arrayInterfaceType);
                        }
                    }
                }
            }

            _nativeDelegateInterfaceCache.Add(method.FullName, interfaceDefinitions);
            return interfaceDefinitions;
        }

        private bool HasNativeDelegateMethodAttribute(MethodDefinition method)
        {
            if (method == null)
            {
                return false;
            }

            if (_nativeDelegateMethodCache.TryGetValue(method.FullName, out var cached))
            {
                return cached;
            }

            var hasAttribute = method.CustomAttributes.Any(attribute =>
                attribute.AttributeType.FullName == NativeDelegateMethodAttributeTypeName);
            _nativeDelegateMethodCache.Add(method.FullName, hasAttribute);
            return hasAttribute;
        }

        private static IReadOnlyList<Instruction> MoveTrailingArgumentsAfterDelegate(
            MethodDefinition owner,
            Instruction callInstruction,
            MethodDefinition placeholder,
            ParameterDefinition delegateParameter,
            List<DiagnosticMessage> diagnostics)
        {
            var trailingCount = placeholder.Parameters.Count - delegateParameter.Index - 1;
            if (trailingCount == 0)
            {
                return Array.Empty<Instruction>();
            }

            var moved = new List<Instruction>[trailingCount];
            var current = PreviousMeaningful(callInstruction);
            for (var i = trailingCount - 1; i >= 0; i--)
            {
                var producerStart = FindStackProducerStart(current, 1);
                if (producerStart == null)
                {
                    AddError(diagnostics, owner, callInstruction, "BLinq delegate weaving only supports simple trailing arguments after the delegate parameter.");
                    return null;
                }

                var beforeProducer = PreviousMeaningful(producerStart);
                moved[i] = new List<Instruction>();
                foreach (var instruction in GetMeaningfulInstructionRange(producerStart, current))
                {
                    if (!CanMoveTrailingArgumentInstruction(instruction))
                    {
                        AddError(diagnostics, owner, callInstruction, "BLinq delegate weaving only supports simple trailing arguments after the delegate parameter.");
                        return null;
                    }

                    moved[i].Add(CloneSimpleInstruction(instruction));
                    instruction.OpCode = OpCodes.Nop;
                    instruction.Operand = null;
                }

                current = beforeProducer;
            }

            return moved.SelectMany(argument => argument).ToArray();
        }

        private static IEnumerable<Instruction> GetMeaningfulInstructionRange(Instruction start, Instruction end)
        {
            var current = start;
            while (current != null)
            {
                if (current.OpCode != OpCodes.Nop)
                {
                    yield return current;
                }

                if (current == end)
                {
                    yield break;
                }

                current = current.Next;
            }
        }

        private static bool CanMoveTrailingArgumentInstruction(Instruction instruction)
        {
            return instruction.OpCode.FlowControl == FlowControl.Next ||
                instruction.OpCode.FlowControl == FlowControl.Call;
        }

        private static Instruction CloneSimpleInstruction(Instruction instruction)
        {
            switch (instruction.Operand)
            {
                case null:
                    return Instruction.Create(instruction.OpCode);
                case sbyte value:
                    return Instruction.Create(instruction.OpCode, value);
                case byte value:
                    return Instruction.Create(instruction.OpCode, value);
                case int value:
                    return Instruction.Create(instruction.OpCode, value);
                case long value:
                    return Instruction.Create(instruction.OpCode, value);
                case float value:
                    return Instruction.Create(instruction.OpCode, value);
                case double value:
                    return Instruction.Create(instruction.OpCode, value);
                case string value:
                    return Instruction.Create(instruction.OpCode, value);
                case TypeReference value:
                    return Instruction.Create(instruction.OpCode, value);
                case MethodReference value:
                    return Instruction.Create(instruction.OpCode, value);
                case FieldReference value:
                    return Instruction.Create(instruction.OpCode, value);
                case ParameterDefinition value:
                    return Instruction.Create(instruction.OpCode, value);
                case VariableDefinition value:
                    return Instruction.Create(instruction.OpCode, value);
                case Instruction value:
                    return Instruction.Create(instruction.OpCode, value);
                case Instruction[] value:
                    return Instruction.Create(instruction.OpCode, value);
                default:
                    throw new InvalidOperationException($"Unsupported instruction operand '{instruction.Operand.GetType().FullName}'.");
            }
        }

        private DelegateSignature ResolveDelegateSignature(
            ModuleDefinition module,
            TypeReference delegateType,
            List<DiagnosticMessage> diagnostics,
            MethodDefinition owner,
            Instruction diagnosticInstruction)
        {
            if (TryResolveFuncSignature(module, delegateType, out var funcSignature))
            {
                return funcSignature;
            }

            if (TryResolveActionSignature(module, delegateType, out var actionSignature))
            {
                return actionSignature;
            }

            AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate type '{delegateType.FullName}' is not a supported Func or Action delegate.");
            return null;
        }

        private static bool TryResolveFuncSignature(ModuleDefinition module, TypeReference delegateType, out DelegateSignature signature)
        {
            signature = null;
            if (delegateType is not GenericInstanceType genericDelegate ||
                delegateType.Namespace != "System" ||
                !delegateType.Name.StartsWith("Func`") ||
                genericDelegate.GenericArguments.Count < 1)
            {
                return false;
            }

            var parameterTypes = genericDelegate.GenericArguments
                .Take(genericDelegate.GenericArguments.Count - 1)
                .Select(module.ImportReference)
                .ToArray();

            signature = new DelegateSignature(
                parameterTypes,
                module.ImportReference(genericDelegate.GenericArguments[genericDelegate.GenericArguments.Count - 1]));
            return true;
        }

        private static bool TryResolveActionSignature(ModuleDefinition module, TypeReference delegateType, out DelegateSignature signature)
        {
            signature = null;
            if (delegateType.Namespace != "System" || delegateType.Name == "Func")
            {
                return false;
            }

            if (delegateType.Name == "Action")
            {
                signature = new DelegateSignature(Array.Empty<TypeReference>(), module.TypeSystem.Void);
                return true;
            }

            if (delegateType is not GenericInstanceType genericDelegate ||
                !delegateType.Name.StartsWith("Action`"))
            {
                return false;
            }

            var parameterTypes = genericDelegate.GenericArguments
                .Select(module.ImportReference)
                .ToArray();

            signature = new DelegateSignature(parameterTypes, module.TypeSystem.Void);
            return true;
        }

        private static TypeReference CreateNativeDelegateInterfaceType(
            ModuleDefinition module,
            TypeReference interfaceDefinition,
            DelegateSignature signature)
        {
            var importedDefinition = module.ImportReference(interfaceDefinition);
            var genericParameterCount = importedDefinition.GenericParameters.Count;
            var arityMarker = importedDefinition.Name.LastIndexOf('`');
            if (genericParameterCount == 0 &&
                arityMarker >= 0 &&
                int.TryParse(importedDefinition.Name.Substring(arityMarker + 1), out var arity))
            {
                genericParameterCount = arity;
            }

            if (genericParameterCount == 0)
            {
                return importedDefinition;
            }

            var interfaceType = new GenericInstanceType(importedDefinition);
            foreach (var parameterType in signature.ParameterTypes)
            {
                if (interfaceType.GenericArguments.Count >= genericParameterCount)
                {
                    break;
                }

                interfaceType.GenericArguments.Add(module.ImportReference(GetNativeDelegateInterfaceArgument(parameterType)));
            }

            if (interfaceType.GenericArguments.Count < genericParameterCount &&
                signature.ReturnType.MetadataType != MetadataType.Void)
            {
                interfaceType.GenericArguments.Add(module.ImportReference(signature.ReturnType));
            }

            return interfaceType;
        }

        private static TypeReference GetNativeDelegateInterfaceArgument(TypeReference type)
        {
            switch (type)
            {
                case ByReferenceType byReference:
                    return GetNativeDelegateInterfaceArgument(byReference.ElementType);
                case RequiredModifierType requiredModifier:
                    return GetNativeDelegateInterfaceArgument(requiredModifier.ElementType);
                case OptionalModifierType optionalModifier:
                    return GetNativeDelegateInterfaceArgument(optionalModifier.ElementType);
                default:
                    return type;
            }
        }

        private TargetMethodInfo FindTargetMethod(
            ModuleDefinition module,
            GenericInstanceMethod placeholderCall,
            MethodDefinition placeholder,
            IReadOnlyDictionary<int, AdapterInfo> adapters,
            List<DiagnosticMessage> diagnostics,
            MethodDefinition owner,
            Instruction diagnosticInstruction)
        {
            var placeholderGenericArguments = new Dictionary<string, TypeReference>();
            for (var i = 0; i < placeholder.GenericParameters.Count; i++)
            {
                placeholderGenericArguments[placeholder.GenericParameters[i].Name] =
                    ResolveRewrittenType(module, placeholderCall.GenericArguments[i]);
            }

            var candidates = GetTargetCandidates(placeholder);

            foreach (var candidate in candidates)
            {
                if (TryCreateTargetMethod(module, placeholderCall, placeholder, candidate, placeholderGenericArguments, adapters, out var target))
                {
                    return target;
                }
            }

            AddError(diagnostics, owner, diagnosticInstruction, $"BLinq delegate weaving could not find unmanaged overload for '{placeholder.FullName}'.");
            return null;
        }

        private IReadOnlyList<MethodDefinition> GetTargetCandidates(MethodDefinition placeholder)
        {
            var key = $"{placeholder.DeclaringType.FullName}|{placeholder.Name}|{placeholder.Parameters.Count}";
            if (_targetCandidateCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var candidates = new List<MethodDefinition>();
            foreach (var method in placeholder.DeclaringType.Methods)
            {
                if (method.Name == placeholder.Name &&
                    method.Parameters.Count == placeholder.Parameters.Count &&
                    !HasNativeDelegateMethodAttribute(method))
                {
                    candidates.Add(method);
                }
            }

            _targetCandidateCache.Add(key, candidates);
            return candidates;
        }

        private bool TryCreateTargetMethod(
            ModuleDefinition module,
            GenericInstanceMethod placeholderCall,
            MethodDefinition placeholder,
            MethodDefinition candidate,
            IReadOnlyDictionary<string, TypeReference> placeholderGenericArguments,
            IReadOnlyDictionary<int, AdapterInfo> adapters,
            out TargetMethodInfo target)
        {
            target = null;
            var targetGenericArguments = new TypeReference[candidate.GenericParameters.Count];

            foreach (var adapter in adapters)
            {
                if (!TryAssignAdapterGenericArgument(candidate.Parameters[adapter.Key].ParameterType, adapter.Value, targetGenericArguments))
                {
                    return false;
                }
            }

            for (var i = 0; i < candidate.GenericParameters.Count; i++)
            {
                if (targetGenericArguments[i] != null)
                {
                    continue;
                }

                var genericParameter = candidate.GenericParameters[i];
                AdapterInfo matchingAdapter = null;
                var matchingAdapterCount = 0;
                foreach (var adapter in adapters.Values)
                {
                    if (!GenericParameterAcceptsInterface(genericParameter, adapter.InterfaceType))
                    {
                        continue;
                    }

                    matchingAdapter = adapter;
                    matchingAdapterCount++;
                }

                if (matchingAdapterCount == 1)
                {
                    targetGenericArguments[i] = matchingAdapter.AdapterType;
                }
                else if (placeholderGenericArguments.TryGetValue(genericParameter.Name, out var argument))
                {
                    targetGenericArguments[i] = argument;
                }
                else
                {
                    return false;
                }
            }

            foreach (var adapter in adapters)
            {
                if (!AdapterSatisfiesCandidateParameter(
                    module,
                    candidate.Parameters[adapter.Key].ParameterType,
                    candidate,
                    targetGenericArguments,
                    adapter.Value))
                {
                    return false;
                }
            }

            for (var i = 0; i < candidate.Parameters.Count; i++)
            {
                if (adapters.ContainsKey(i))
                {
                    continue;
                }

                var parameterType = SubstituteMethodGenericArguments(module, candidate.Parameters[i].ParameterType, candidate, targetGenericArguments);
                var placeholderParameterType = ResolveRewrittenType(
                    module,
                    CloseMethodGenericType(module, placeholder.Parameters[i].ParameterType, placeholderCall));
                if (parameterType.ContainsGenericParameter ||
                    !TypeReferencesMatch(parameterType, placeholderParameterType))
                {
                    return false;
                }
            }

            var methodReference = CreateMethodReference(module, candidate);
            var genericMethod = new GenericInstanceMethod(methodReference);
            foreach (var argument in targetGenericArguments)
            {
                genericMethod.GenericArguments.Add(module.ImportReference(argument));
            }

            target = new TargetMethodInfo(
                genericMethod,
                SubstituteMethodGenericArguments(module, candidate.ReturnType, candidate, targetGenericArguments));
            return true;
        }

        private static bool TryAssignAdapterGenericArgument(
            TypeReference parameterType,
            AdapterInfo adapter,
            TypeReference[] targetGenericArguments)
        {
            if (parameterType is GenericParameter genericParameter &&
                genericParameter.Type == GenericParameterType.Method)
            {
                if (!GenericParameterAcceptsInterface(genericParameter, adapter.InterfaceType))
                {
                    return false;
                }

                var existing = targetGenericArguments[genericParameter.Position];
                if (existing != null && !TypeReferencesMatch(existing, adapter.AdapterType))
                {
                    return false;
                }

                targetGenericArguments[genericParameter.Position] = adapter.AdapterType;
                return true;
            }

            if (parameterType is GenericInstanceType genericInstance)
            {
                foreach (var argument in genericInstance.GenericArguments)
                {
                    if (TryAssignAdapterGenericArgument(argument, adapter, targetGenericArguments))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AdapterSatisfiesCandidateParameter(
            ModuleDefinition module,
            TypeReference parameterType,
            MethodDefinition candidate,
            IReadOnlyList<TypeReference> targetGenericArguments,
            AdapterInfo adapter)
        {
            if (parameterType is GenericParameter genericParameter &&
                genericParameter.Type == GenericParameterType.Method)
            {
                var candidateGenericParameter = candidate.GenericParameters[genericParameter.Position];
                return candidateGenericParameter.Constraints.Any(constraint =>
                    TypeReferencesMatch(
                        SubstituteMethodGenericArguments(module, constraint.ConstraintType, candidate, targetGenericArguments),
                        adapter.InterfaceType));
            }

            if (parameterType is GenericInstanceType genericInstance)
            {
                foreach (var argument in genericInstance.GenericArguments)
                {
                    if (AdapterSatisfiesCandidateParameter(module, argument, candidate, targetGenericArguments, adapter))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TypeReferencesMatch(TypeReference left, TypeReference right)
        {
            if (left is GenericInstanceType leftGeneric && right is GenericInstanceType rightGeneric)
            {
                return leftGeneric.ElementType.FullName == rightGeneric.ElementType.FullName &&
                    leftGeneric.GenericArguments.Count == rightGeneric.GenericArguments.Count &&
                    leftGeneric.GenericArguments.Zip(rightGeneric.GenericArguments, TypeReferencesMatch).All(match => match);
            }

            return left.FullName == right.FullName;
        }

        private static bool GenericParameterAcceptsInterface(GenericParameter genericParameter, TypeReference interfaceType)
        {
            return genericParameter.Constraints.Any(constraint =>
                TypeDefinitionsMatch(constraint.ConstraintType, interfaceType));
        }

        private static bool TypeDefinitionsMatch(TypeReference left, TypeReference right)
        {
            var leftElement = left.GetElementType();
            var rightElement = right.GetElementType();
            return leftElement.FullName == rightElement.FullName;
        }

        private static MethodReference CreateMethodReference(ModuleDefinition module, MethodDefinition methodDefinition)
        {
            var methodReference = new MethodReference(
                methodDefinition.Name,
                module.TypeSystem.Void,
                module.ImportReference(methodDefinition.DeclaringType))
            {
                HasThis = methodDefinition.HasThis,
                ExplicitThis = methodDefinition.ExplicitThis,
                CallingConvention = methodDefinition.CallingConvention,
            };

            foreach (var genericParameter in methodDefinition.GenericParameters)
            {
                methodReference.GenericParameters.Add(new GenericParameter(genericParameter.Name, methodReference));
            }

            methodReference.ReturnType = ImportMethodReferenceSignatureType(module, methodDefinition.ReturnType, methodReference);
            foreach (var parameter in methodDefinition.Parameters)
            {
                methodReference.Parameters.Add(new ParameterDefinition(
                    ImportMethodReferenceSignatureType(module, parameter.ParameterType, methodReference)));
            }

            return methodReference;
        }

        private static TypeReference ImportMethodReferenceSignatureType(
            ModuleDefinition module,
            TypeReference type,
            MethodReference methodGenericOwner)
        {
            return RewriteTypeReference(
                type,
                genericParameter => genericParameter.Type == GenericParameterType.Method
                    ? methodGenericOwner.GenericParameters[genericParameter.Position]
                    : null,
                module.ImportReference);
        }

        private static TypeReference SubstituteMethodGenericArguments(
            ModuleDefinition module,
            TypeReference type,
            MethodDefinition method,
            IReadOnlyList<TypeReference> genericArguments)
        {
            return RewriteTypeReference(
                type,
                genericParameter => genericParameter.Type == GenericParameterType.Method
                    ? module.ImportReference(genericArguments[genericParameter.Position])
                    : null,
                module.ImportReference);
        }

        private static TypeReference CloseMethodGenericType(
            ModuleDefinition module,
            TypeReference type,
            GenericInstanceMethod method)
        {
            return RewriteTypeReference(
                type,
                genericParameter => genericParameter.Type == GenericParameterType.Method
                    ? module.ImportReference(method.GenericArguments[genericParameter.Position])
                    : null,
                module.ImportReference);
        }

        private void MapRewrittenReturnType(TypeReference placeholderReturnType, TypeReference realReturnType)
        {
            MapRewrittenType(placeholderReturnType, realReturnType);

            if (placeholderReturnType is not GenericInstanceType placeholderQueryType ||
                placeholderQueryType.GenericArguments.Count != 2 ||
                realReturnType is not GenericInstanceType realQueryType ||
                realQueryType.GenericArguments.Count != 2)
            {
                return;
            }

            _rewrittenEnumeratorTypes[placeholderQueryType.GenericArguments[0].FullName] = realQueryType.GenericArguments[0];
        }

        private void MapRewrittenType(TypeReference placeholderType, TypeReference realType)
        {
            if (!TypeReferencesMatch(placeholderType, realType))
            {
                _rewrittenEnumeratorTypes[placeholderType.FullName] = realType;
            }

            if (placeholderType is not GenericInstanceType placeholderGeneric ||
                realType is not GenericInstanceType realGeneric ||
                placeholderGeneric.GenericArguments.Count != realGeneric.GenericArguments.Count)
            {
                return;
            }

            for (var i = 0; i < placeholderGeneric.GenericArguments.Count; i++)
            {
                MapRewrittenType(placeholderGeneric.GenericArguments[i], realGeneric.GenericArguments[i]);
            }
        }

        private bool TryRewriteMethodReference(
            ModuleDefinition module,
            Instruction callInstruction,
            MethodReference call)
        {
            if (_rewrittenEnumeratorTypes.Count == 0)
            {
                return false;
            }

            if (!ContainsRewrittenType(call))
            {
                return false;
            }

            var rewrittenCall = RewriteMethodReference(module, call, out var modified);
            if (!modified)
            {
                return false;
            }

            callInstruction.Operand = rewrittenCall;
            return true;
        }

        private bool TryRewriteVariableTypes(
            ModuleDefinition module,
            IEnumerable<VariableDefinition> variables)
        {
            var modified = false;

            foreach (var variable in variables)
            {
                if (!ContainsRewrittenType(variable.VariableType))
                {
                    continue;
                }

                var rewrittenType = ResolveRewrittenType(module, variable.VariableType);
                if (TypeReferencesMatch(variable.VariableType, rewrittenType))
                {
                    continue;
                }

                variable.VariableType = rewrittenType;
                modified = true;
            }

            return modified;
        }

        private bool ContainsRewrittenType(MethodReference method)
        {
            if (ContainsRewrittenType(method.DeclaringType) ||
                ContainsRewrittenType(method.ReturnType))
            {
                return true;
            }

            foreach (var parameter in method.Parameters)
            {
                if (ContainsRewrittenType(parameter.ParameterType))
                {
                    return true;
                }
            }

            if (method is GenericInstanceMethod genericMethod)
            {
                foreach (var argument in genericMethod.GenericArguments)
                {
                    if (ContainsRewrittenType(argument))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ContainsRewrittenType(TypeReference type)
        {
            if (type == null)
            {
                return false;
            }

            if (_rewrittenEnumeratorTypes.ContainsKey(type.FullName))
            {
                return true;
            }

            switch (type)
            {
                case GenericInstanceType genericInstance:
                    foreach (var argument in genericInstance.GenericArguments)
                    {
                        if (ContainsRewrittenType(argument))
                        {
                            return true;
                        }
                    }

                    return ContainsRewrittenType(genericInstance.ElementType);
                case ByReferenceType byReference:
                    return ContainsRewrittenType(byReference.ElementType);
                case PointerType pointer:
                    return ContainsRewrittenType(pointer.ElementType);
                case RequiredModifierType requiredModifier:
                    return ContainsRewrittenType(requiredModifier.ModifierType) ||
                        ContainsRewrittenType(requiredModifier.ElementType);
                case OptionalModifierType optionalModifier:
                    return ContainsRewrittenType(optionalModifier.ModifierType) ||
                        ContainsRewrittenType(optionalModifier.ElementType);
                default:
                    return false;
            }
        }

        private MethodReference RewriteMethodReference(
            ModuleDefinition module,
            MethodReference method,
            out bool modified)
        {
            if (method is GenericInstanceMethod genericMethod)
            {
                var rewrittenElementMethod = RewriteOpenMethodReference(module, genericMethod.ElementMethod, out modified);
                var rewrittenGenericMethod = new GenericInstanceMethod(rewrittenElementMethod);

                foreach (var genericArgument in genericMethod.GenericArguments)
                {
                    var rewrittenArgument = ResolveRewrittenType(module, genericArgument);
                    modified |= !TypeReferencesMatch(genericArgument, rewrittenArgument);
                    rewrittenGenericMethod.GenericArguments.Add(module.ImportReference(rewrittenArgument));
                }

                return rewrittenGenericMethod;
            }

            return RewriteOpenMethodReference(module, method, out modified);
        }

        private MethodReference RewriteOpenMethodReference(
            ModuleDefinition module,
            MethodReference method,
            out bool modified)
        {
            var declaringType = ResolveRewrittenType(module, method.DeclaringType);
            modified = !TypeReferencesMatch(method.DeclaringType, declaringType);

            var methodReference = new MethodReference(
                method.Name,
                module.TypeSystem.Void,
                module.ImportReference(declaringType))
            {
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention,
            };

            foreach (var genericParameter in method.GenericParameters)
            {
                methodReference.GenericParameters.Add(new GenericParameter(genericParameter.Name, methodReference));
            }

            methodReference.ReturnType = RewriteMethodReferenceSignatureType(module, method.ReturnType, method, methodReference, ref modified);
            foreach (var parameter in method.Parameters)
            {
                methodReference.Parameters.Add(new ParameterDefinition(
                    RewriteMethodReferenceSignatureType(module, parameter.ParameterType, method, methodReference, ref modified)));
            }

            return methodReference;
        }

        private TypeReference RewriteMethodReferenceSignatureType(
            ModuleDefinition module,
            TypeReference type,
            MethodReference originalMethod,
            MethodReference rewrittenMethod,
            ref bool modified)
        {
            var rewritten = RewriteTypeReference(
                type,
                genericParameter =>
                {
                    if (genericParameter.Type == GenericParameterType.Method)
                    {
                        return rewrittenMethod.GenericParameters[genericParameter.Position];
                    }

                    return null;
                },
                typeReference => ResolveRewrittenType(module, typeReference));

            modified |= !TypeReferencesMatch(type, rewritten);
            return rewritten;
        }

        private TypeReference ResolveRewrittenType(ModuleDefinition module, TypeReference type)
        {
            if (_rewrittenEnumeratorTypes.TryGetValue(type.FullName, out var rewritten))
            {
                return module.ImportReference(rewritten);
            }

            if (type is GenericInstanceType genericInstance)
            {
                var closed = new GenericInstanceType(module.ImportReference(genericInstance.ElementType));
                foreach (var argument in genericInstance.GenericArguments)
                {
                    closed.GenericArguments.Add(ResolveRewrittenType(module, argument));
                }

                return closed;
            }

            return module.ImportReference(type);
        }
    }
}
