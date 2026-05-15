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

            var originalInstructions = owner.Body.Instructions.ToArray();
            var originalInstructionState = originalInstructions.ToDictionary(
                instruction => instruction,
                instruction => (instruction.OpCode, instruction.Operand));
            var originalVariableCount = owner.Body.Variables.Count;
            var originalNestedTypeCount = owner.DeclaringType.NestedTypes.Count;
            var originalAdapterIndex = _adapterIndex;
            var originalRewrittenEnumeratorTypes = new Dictionary<string, TypeReference>(_rewrittenEnumeratorTypes);
            var originalAmbiguousRewrittenEnumeratorTypes = new HashSet<string>(_ambiguousRewrittenEnumeratorTypes);
            var originalRewrittenLocalTypes = new Dictionary<VariableDefinition, TypeReference>(_rewrittenLocalTypes);
            var originalAmbiguousRewrittenLocalTypes = new HashSet<VariableDefinition>(_ambiguousRewrittenLocalTypes);
            var originalActiveLocalAliases = new Dictionary<VariableDefinition, VariableDefinition>(_activeLocalAliases);
            var originalRewrittenCaptureLocals = new Dictionary<string, IReadOnlyDictionary<FieldDefinition, VariableDefinition>>(_rewrittenCaptureLocals);

            bool Fail()
            {
                foreach (var instruction in owner.Body.Instructions.Where(instruction => !originalInstructionState.ContainsKey(instruction)).ToArray())
                {
                    owner.Body.Instructions.Remove(instruction);
                }

                foreach (var pair in originalInstructionState)
                {
                    pair.Key.OpCode = pair.Value.OpCode;
                    pair.Key.Operand = pair.Value.Operand;
                }

                while (owner.Body.Variables.Count > originalVariableCount)
                {
                    owner.Body.Variables.RemoveAt(owner.Body.Variables.Count - 1);
                }

                while (owner.DeclaringType.NestedTypes.Count > originalNestedTypeCount)
                {
                    owner.DeclaringType.NestedTypes.RemoveAt(owner.DeclaringType.NestedTypes.Count - 1);
                }

                _adapterIndex = originalAdapterIndex;
                _rewrittenEnumeratorTypes.Clear();
                foreach (var pair in originalRewrittenEnumeratorTypes)
                {
                    _rewrittenEnumeratorTypes.Add(pair.Key, pair.Value);
                }

                _ambiguousRewrittenEnumeratorTypes.Clear();
                foreach (var value in originalAmbiguousRewrittenEnumeratorTypes)
                {
                    _ambiguousRewrittenEnumeratorTypes.Add(value);
                }

                _rewrittenLocalTypes.Clear();
                foreach (var pair in originalRewrittenLocalTypes)
                {
                    _rewrittenLocalTypes.Add(pair.Key, pair.Value);
                }

                _ambiguousRewrittenLocalTypes.Clear();
                foreach (var value in originalAmbiguousRewrittenLocalTypes)
                {
                    _ambiguousRewrittenLocalTypes.Add(value);
                }

                _activeLocalAliases.Clear();
                foreach (var pair in originalActiveLocalAliases)
                {
                    _activeLocalAliases.Add(pair.Key, pair.Value);
                }

                _rewrittenCaptureLocals.Clear();
                foreach (var pair in originalRewrittenCaptureLocals)
                {
                    _rewrittenCaptureLocals.Add(pair.Key, pair.Value);
                }

                return false;
            }

            var adapters = new Dictionary<int, AdapterInfo>();
            for (var i = delegateParameters.Length - 1; i >= 0; i--)
            {
                var parameter = delegateParameters[i];
                var trailingArguments = MoveTrailingArgumentsAfterDelegate(owner, callInstruction, placeholder, parameter, diagnostics);
                if (trailingArguments == null)
                {
                    return Fail();
                }

                var delegateType = CloseMethodGenericType(module, parameter.ParameterType, placeholderCall);
                var signature = ResolveDelegateSignature(module, delegateType, diagnostics, owner, callInstruction);
                if (signature == null)
                {
                    return Fail();
                }

                var interfaceType = CreateNativeDelegateInterfaceType(module, interfaceDefinitions[i], signature);
                var adapter = CreateAdapter(owner, callInstruction, signature, interfaceType, diagnostics);
                if (adapter == null)
                {
                    return Fail();
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
                return Fail();
            }

            callInstruction.Operand = target.Call;
            var placeholderReturnType = CloseMethodGenericType(module, placeholderCall.ReturnType, placeholderCall);
            MapRewrittenReturnType(placeholderReturnType, target.ReturnType);
            MapRewrittenStoredLocal(owner, callInstruction, placeholderReturnType, target.ReturnType);

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
            var placeholderGenericArguments = CreatePlaceholderGenericArguments(
                module,
                owner,
                diagnosticInstruction,
                placeholderCall,
                placeholder);

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
                var placeholderParameterType = SubstitutePlaceholderGenericArguments(
                    module,
                    placeholder.Parameters[i].ParameterType,
                    placeholderGenericArguments);
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
            left = UnwrapComparableType(left);
            right = UnwrapComparableType(right);

            if (left is GenericInstanceType leftGeneric && right is GenericInstanceType rightGeneric)
            {
                return leftGeneric.ElementType.FullName == rightGeneric.ElementType.FullName &&
                    leftGeneric.GenericArguments.Count == rightGeneric.GenericArguments.Count &&
                    leftGeneric.GenericArguments.Zip(rightGeneric.GenericArguments, TypeReferencesMatch).All(match => match);
            }

            return left.FullName == right.FullName;
        }

        private static TypeReference UnwrapComparableType(TypeReference type)
        {
            while (true)
            {
                switch (type)
                {
                    case RequiredModifierType requiredModifier:
                        type = requiredModifier.ElementType;
                        continue;
                    case OptionalModifierType optionalModifier:
                        type = optionalModifier.ElementType;
                        continue;
                    default:
                        return type;
                }
            }
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
            CopyTupleElementNamesAttributes(methodDefinition.MethodReturnType, methodReference.MethodReturnType, module);
            foreach (var parameter in methodDefinition.Parameters)
            {
                var parameterReference = new ParameterDefinition(
                    parameter.Name,
                    parameter.Attributes,
                    ImportMethodReferenceSignatureType(module, parameter.ParameterType, methodReference));
                CopyTupleElementNamesAttributes(parameter, parameterReference, module);
                methodReference.Parameters.Add(parameterReference);
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

            AddRewrittenTypeMapping(placeholderQueryType.GenericArguments[0].FullName, realQueryType.GenericArguments[0]);
        }

        private void MapRewrittenStoredLocal(
            MethodDefinition owner,
            Instruction callInstruction,
            TypeReference placeholderReturnType,
            TypeReference realReturnType)
        {
            if (TypeReferencesMatch(placeholderReturnType, realReturnType))
            {
                return;
            }

            var store = NextMeaningful(callInstruction);
            if (store == null ||
                !TryGetStoredLocal(owner, store, out var local))
            {
                return;
            }

            AddRewrittenLocalTypeMapping(owner, store, local, realReturnType);
        }

        private void MapRewrittenType(TypeReference placeholderType, TypeReference realType)
        {
            if (!TypeReferencesMatch(placeholderType, realType))
            {
                AddRewrittenTypeMapping(placeholderType.FullName, realType);
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

        private void AddRewrittenTypeMapping(string placeholderTypeName, TypeReference realType)
        {
            if (_ambiguousRewrittenEnumeratorTypes.Contains(placeholderTypeName))
            {
                return;
            }

            if (_rewrittenEnumeratorTypes.TryGetValue(placeholderTypeName, out var existing))
            {
                if (TypeReferencesMatch(existing, realType))
                {
                    return;
                }

                _rewrittenEnumeratorTypes.Remove(placeholderTypeName);
                _ambiguousRewrittenEnumeratorTypes.Add(placeholderTypeName);
                return;
            }

            _rewrittenEnumeratorTypes.Add(placeholderTypeName, realType);
        }

        private void AddRewrittenLocalTypeMapping(
            MethodDefinition owner,
            Instruction storeInstruction,
            VariableDefinition local,
            TypeReference realType)
        {
            if (_ambiguousRewrittenLocalTypes.Contains(local))
            {
                return;
            }

            var activeLocal = ResolveActiveLocalAlias(local);
            if (_rewrittenLocalTypes.TryGetValue(activeLocal, out var existing))
            {
                if (TypeReferencesMatch(existing, realType))
                {
                    if (activeLocal != local)
                    {
                        RewriteStoreLocalInstruction(storeInstruction, activeLocal);
                    }

                    return;
                }

                var alias = new VariableDefinition(owner.Module.ImportReference(realType));
                owner.Body.Variables.Add(alias);
                owner.Body.InitLocals = true;
                _rewrittenLocalTypes.Add(alias, realType);
                _activeLocalAliases[local] = alias;
                RewriteStoreLocalInstruction(storeInstruction, alias);
                return;
            }

            _rewrittenLocalTypes.Add(activeLocal, realType);
            if (activeLocal != local)
            {
                RewriteStoreLocalInstruction(storeInstruction, activeLocal);
            }
        }

        private VariableDefinition ResolveActiveLocalAlias(VariableDefinition local)
        {
            return _activeLocalAliases.TryGetValue(local, out var alias)
                ? alias
                : local;
        }

        private static void RewriteStoreLocalInstruction(Instruction instruction, VariableDefinition local)
        {
            instruction.OpCode = OpCodes.Stloc;
            instruction.Operand = local;
        }

        private static void RewriteLoadLocalInstruction(Instruction instruction, VariableDefinition local)
        {
            instruction.OpCode = OpCodes.Ldloc;
            instruction.Operand = local;
        }

        private static void RewriteLoadLocalAddressInstruction(Instruction instruction, VariableDefinition local)
        {
            instruction.OpCode = OpCodes.Ldloca;
            instruction.Operand = local;
        }

        private bool TryRewriteMethodReference(
            ModuleDefinition module,
            MethodDefinition owner,
            Instruction callInstruction,
            MethodReference call)
        {
            if (_rewrittenEnumeratorTypes.Count == 0 &&
                _ambiguousRewrittenEnumeratorTypes.Count == 0 &&
                _rewrittenLocalTypes.Count == 0)
            {
                return false;
            }

            if (!ContainsRewrittenType(call) &&
                call is not GenericInstanceMethod &&
                !call.HasThis)
            {
                return false;
            }

            var placeholderReturnType = CloseMethodReturnType(module, call, call.DeclaringType);
            var rewrittenCall = RewriteMethodReference(module, owner, callInstruction, call, out var modified);
            if (!modified)
            {
                return false;
            }

            callInstruction.Operand = rewrittenCall;
            var rewrittenReturnType = CloseMethodReturnType(module, rewrittenCall, rewrittenCall.DeclaringType);
            MapRewrittenReturnType(placeholderReturnType, rewrittenReturnType);
            MapRewrittenStoredLocal(owner, callInstruction, placeholderReturnType, rewrittenReturnType);
            return true;
        }

        private bool TryRewriteVariableTypes(
            ModuleDefinition module,
            IEnumerable<VariableDefinition> variables)
        {
            var modified = false;

            foreach (var variable in variables)
            {
                var hasLocalRewrite = _rewrittenLocalTypes.ContainsKey(variable);
                if (!hasLocalRewrite &&
                    !ContainsRewrittenType(variable.VariableType))
                {
                    continue;
                }

                var rewrittenType = ResolveRewrittenType(module, variable.VariableType);
                if (_rewrittenLocalTypes.TryGetValue(variable, out var localType))
                {
                    rewrittenType = module.ImportReference(localType);
                }

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
            MethodDefinition owner,
            Instruction callInstruction,
            MethodReference method,
            out bool modified)
        {
            var inferredDeclaringType = InferDeclaringTypeFromStack(module, owner, callInstruction, method);
            if (method is GenericInstanceMethod genericMethod)
            {
                var rewrittenElementMethod = RewriteOpenMethodReference(
                    module,
                    genericMethod.ElementMethod,
                    inferredDeclaringType,
                    out modified);
                var rewrittenGenericMethod = new GenericInstanceMethod(rewrittenElementMethod);
                var inferredArguments = InferMethodGenericArgumentsFromStack(module, owner, callInstruction, genericMethod);

                foreach (var genericArgument in genericMethod.GenericArguments)
                {
                    var rewrittenArgument = ResolveRewrittenType(module, genericArgument);
                    if (inferredArguments.TryGetValue(rewrittenGenericMethod.GenericArguments.Count, out var inferredArgument))
                    {
                        rewrittenArgument = inferredArgument;
                    }

                    modified |= !TypeReferencesMatch(genericArgument, rewrittenArgument);
                    rewrittenGenericMethod.GenericArguments.Add(module.ImportReference(rewrittenArgument));
                }

                return rewrittenGenericMethod;
            }

            return RewriteOpenMethodReference(module, method, inferredDeclaringType, out modified);
        }

        private MethodReference RewriteOpenMethodReference(
            ModuleDefinition module,
            MethodReference method,
            TypeReference inferredDeclaringType,
            out bool modified)
        {
            var declaringType = inferredDeclaringType == null
                ? ResolveRewrittenType(module, method.DeclaringType)
                : module.ImportReference(inferredDeclaringType);
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

            methodReference.ReturnType = RewriteMethodReferenceSignatureType(
                module,
                method.ReturnType,
                methodReference,
                ref modified);
            CopyTupleElementNamesAttributes(method.MethodReturnType, methodReference.MethodReturnType, module);
            foreach (var parameter in method.Parameters)
            {
                var parameterReference = new ParameterDefinition(
                    parameter.Name,
                    parameter.Attributes,
                    RewriteMethodReferenceSignatureType(
                        module,
                        parameter.ParameterType,
                        methodReference,
                        ref modified));
                CopyTupleElementNamesAttributes(parameter, parameterReference, module);
                methodReference.Parameters.Add(parameterReference);
            }

            return methodReference;
        }

        private TypeReference RewriteMethodReferenceSignatureType(
            ModuleDefinition module,
            TypeReference type,
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

        private static TypeReference ResolveDeclaringTypeGenericArgument(TypeReference declaringType, int position)
        {
            if (declaringType is GenericInstanceType genericDeclaringType &&
                position >= 0 &&
                position < genericDeclaringType.GenericArguments.Count)
            {
                return genericDeclaringType.GenericArguments[position];
            }

            return null;
        }

        private TypeReference ResolveRewrittenType(ModuleDefinition module, TypeReference type)
        {
            if (_ambiguousRewrittenEnumeratorTypes.Contains(type.FullName))
            {
                return module.ImportReference(type);
            }

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

        private static TypeReference SubstitutePlaceholderGenericArguments(
            ModuleDefinition module,
            TypeReference type,
            IReadOnlyDictionary<string, TypeReference> genericArguments)
        {
            return RewriteTypeReference(
                type,
                genericParameter =>
                    genericParameter.Type == GenericParameterType.Method &&
                    genericArguments.TryGetValue(genericParameter.Name, out var argument)
                        ? module.ImportReference(argument)
                        : null,
                module.ImportReference);
        }

        private Dictionary<string, TypeReference> CreatePlaceholderGenericArguments(
            ModuleDefinition module,
            MethodDefinition owner,
            Instruction callInstruction,
            GenericInstanceMethod placeholderCall,
            MethodDefinition placeholder)
        {
            var genericArguments = new Dictionary<string, TypeReference>();
            var inferredArguments = InferMethodGenericArgumentsFromStack(module, owner, callInstruction, placeholderCall);

            for (var i = 0; i < placeholder.GenericParameters.Count; i++)
            {
                genericArguments[placeholder.GenericParameters[i].Name] =
                    inferredArguments.TryGetValue(i, out var inferredArgument)
                        ? inferredArgument
                        : ResolveRewrittenType(module, placeholderCall.GenericArguments[i]);
            }

            return genericArguments;
        }

        private Dictionary<int, TypeReference> InferMethodGenericArgumentsFromStack(
            ModuleDefinition module,
            MethodDefinition owner,
            Instruction callInstruction,
            GenericInstanceMethod call)
        {
            var inferredArguments = new Dictionary<int, TypeReference>();
            var methodDefinition = call.Resolve();
            if (methodDefinition == null ||
                !TryGetArgumentProducerEnds(owner, callInstruction, call, out var producerEnds))
            {
                return inferredArguments;
            }

            var argumentOffset = call.HasThis ? 1 : 0;
            if (call.HasThis &&
                TryGetProducedType(module, owner, producerEnds[0], out var instanceType))
            {
                InferGenericArguments(module, methodDefinition.DeclaringType, instanceType, inferredArguments);
            }

            for (var i = 0; i < methodDefinition.Parameters.Count; i++)
            {
                if (!TryGetProducedType(module, owner, producerEnds[i + argumentOffset], out var actualType))
                {
                    continue;
                }

                InferGenericArguments(module, methodDefinition.Parameters[i].ParameterType, actualType, inferredArguments);
            }

            return inferredArguments;
        }

        private TypeReference InferDeclaringTypeFromStack(
            ModuleDefinition module,
            MethodDefinition owner,
            Instruction callInstruction,
            MethodReference call)
        {
            if (!call.HasThis ||
                !TryGetArgumentProducerEnds(owner, callInstruction, call, out var producerEnds) ||
                !TryGetProducedType(module, owner, producerEnds[0], out var actualType))
            {
                return null;
            }

            if (actualType is ByReferenceType byReference)
            {
                actualType = byReference.ElementType;
            }

            return TypeDefinitionsMatch(actualType, call.DeclaringType)
                ? module.ImportReference(actualType)
                : null;
        }

        private static bool TryGetArgumentProducerEnds(
            MethodDefinition owner,
            Instruction callInstruction,
            MethodReference call,
            out Instruction[] producerEnds)
        {
            var argumentCount = call.Parameters.Count + (call.HasThis ? 1 : 0);
            producerEnds = new Instruction[argumentCount];
            var current = PreviousMeaningful(callInstruction);

            for (var i = argumentCount - 1; i >= 0; i--)
            {
                if (current == null)
                {
                    return false;
                }

                var producerStart = FindStackProducerStart(current, 1);
                if (producerStart == null)
                {
                    return false;
                }

                producerEnds[i] = current;
                current = PreviousMeaningful(producerStart);
            }

            return true;
        }

        private bool TryGetProducedType(
            ModuleDefinition module,
            MethodDefinition owner,
            Instruction instruction,
            out TypeReference type)
        {
            type = null;
            switch (instruction.OpCode.Code)
            {
                case Code.Call:
                case Code.Callvirt:
                    if (instruction.Operand is MethodReference method)
                    {
                        type = CloseMethodReturnType(
                            module,
                            method,
                            InferDeclaringTypeFromStack(module, owner, instruction, method));
                        return type.MetadataType != MetadataType.Void;
                    }

                    return false;
                case Code.Newobj:
                    if (instruction.Operand is MethodReference constructor)
                    {
                        type = module.ImportReference(constructor.DeclaringType);
                        return true;
                    }

                    return false;
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    if (TryGetLoadedLocal(owner, instruction, out var local))
                    {
                        var activeLocal = ResolveActiveLocalAlias(local);
                        if (activeLocal != local)
                        {
                            RewriteLoadLocalInstruction(instruction, activeLocal);
                        }

                        type = GetLocalType(module, activeLocal);
                        return true;
                    }

                    return false;
                case Code.Ldloca:
                case Code.Ldloca_S:
                    if (instruction.Operand is VariableDefinition addressLocal)
                    {
                        var activeLocal = ResolveActiveLocalAlias(addressLocal);
                        if (activeLocal != addressLocal)
                        {
                            RewriteLoadLocalAddressInstruction(instruction, activeLocal);
                        }

                        type = new ByReferenceType(GetLocalType(module, activeLocal));
                        return true;
                    }

                    return false;
                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return TryGetLoadedArgumentType(module, owner, instruction, out type);
                case Code.Ldarga:
                case Code.Ldarga_S:
                    if (instruction.Operand is ParameterDefinition addressParameter)
                    {
                        type = new ByReferenceType(module.ImportReference(addressParameter.ParameterType));
                        return true;
                    }

                    return false;
                case Code.Ldfld:
                case Code.Ldsfld:
                    if (instruction.Operand is FieldReference field)
                    {
                        type = module.ImportReference(field.FieldType);
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        private TypeReference GetLocalType(ModuleDefinition module, VariableDefinition local)
        {
            return _rewrittenLocalTypes.TryGetValue(local, out var localType)
                ? module.ImportReference(localType)
                : module.ImportReference(local.VariableType);
        }

        private static bool TryGetLoadedArgumentType(
            ModuleDefinition module,
            MethodDefinition owner,
            Instruction instruction,
            out TypeReference type)
        {
            type = null;
            if (!TryGetArgumentIndex(instruction, out var argumentIndex, out var parameter))
            {
                return false;
            }

            if (parameter != null)
            {
                type = module.ImportReference(parameter.ParameterType);
                return true;
            }

            if (owner.HasThis)
            {
                if (argumentIndex == 0)
                {
                    type = module.ImportReference(owner.DeclaringType);
                    return true;
                }

                argumentIndex--;
            }

            if (argumentIndex < 0 || argumentIndex >= owner.Parameters.Count)
            {
                return false;
            }

            type = module.ImportReference(owner.Parameters[argumentIndex].ParameterType);
            return true;
        }

        private static bool TryGetArgumentIndex(
            Instruction instruction,
            out int argumentIndex,
            out ParameterDefinition parameter)
        {
            parameter = instruction.Operand as ParameterDefinition;
            if (parameter != null)
            {
                argumentIndex = parameter.Index;
                return true;
            }

            switch (instruction.OpCode.Code)
            {
                case Code.Ldarg_0:
                    argumentIndex = 0;
                    return true;
                case Code.Ldarg_1:
                    argumentIndex = 1;
                    return true;
                case Code.Ldarg_2:
                    argumentIndex = 2;
                    return true;
                case Code.Ldarg_3:
                    argumentIndex = 3;
                    return true;
                default:
                    argumentIndex = -1;
                    return false;
            }
        }

        private static TypeReference CloseMethodReturnType(
            ModuleDefinition module,
            MethodReference method,
            TypeReference declaringType)
        {
            if (method is GenericInstanceMethod genericMethod)
            {
                return CloseDeclaringTypeGenericType(
                    module,
                    CloseMethodGenericType(module, genericMethod.ElementMethod.ReturnType, genericMethod),
                    declaringType);
            }

            return CloseDeclaringTypeGenericType(module, method.ReturnType, declaringType);
        }

        private static TypeReference CloseDeclaringTypeGenericType(
            ModuleDefinition module,
            TypeReference type,
            TypeReference declaringType)
        {
            if (declaringType == null)
            {
                return module.ImportReference(type);
            }

            return RewriteTypeReference(
                type,
                genericParameter =>
                    genericParameter.Type == GenericParameterType.Type
                        ? ResolveDeclaringTypeGenericArgument(declaringType, genericParameter.Position)
                        : null,
                module.ImportReference);
        }

        private static void InferGenericArguments(
            ModuleDefinition module,
            TypeReference pattern,
            TypeReference actual,
            IDictionary<int, TypeReference> inferredArguments)
        {
            switch (pattern)
            {
                case GenericParameter genericParameter
                    when genericParameter.Type == GenericParameterType.Method:
                    AddInferredGenericArgument(module, genericParameter.Position, actual, inferredArguments);
                    return;
                case GenericInstanceType patternGeneric
                    when actual is ByReferenceType actualByReferenceGeneric:
                    InferGenericArguments(module, patternGeneric, actualByReferenceGeneric.ElementType, inferredArguments);
                    return;
                case GenericInstanceType patternGeneric
                    when actual is GenericInstanceType actualGeneric &&
                        patternGeneric.ElementType.FullName == actualGeneric.ElementType.FullName &&
                        patternGeneric.GenericArguments.Count == actualGeneric.GenericArguments.Count:
                    for (var i = 0; i < patternGeneric.GenericArguments.Count; i++)
                    {
                        InferGenericArguments(module, patternGeneric.GenericArguments[i], actualGeneric.GenericArguments[i], inferredArguments);
                    }

                    return;
                case ByReferenceType patternByReference:
                    if (actual is ByReferenceType actualByReference)
                    {
                        InferGenericArguments(module, patternByReference.ElementType, actualByReference.ElementType, inferredArguments);
                    }

                    return;
                case RequiredModifierType patternRequiredModifier:
                    InferGenericArguments(module, patternRequiredModifier.ElementType, actual, inferredArguments);
                    return;
                case OptionalModifierType patternOptionalModifier:
                    InferGenericArguments(module, patternOptionalModifier.ElementType, actual, inferredArguments);
                    return;
            }
        }

        private static void AddInferredGenericArgument(
            ModuleDefinition module,
            int position,
            TypeReference argument,
            IDictionary<int, TypeReference> inferredArguments)
        {
            argument = module.ImportReference(argument);
            if (argument.ContainsGenericParameter)
            {
                return;
            }

            if (!inferredArguments.TryGetValue(position, out var existing) ||
                TypeReferencesMatch(existing, argument))
            {
                inferredArguments[position] = argument;
            }
        }
    }
}
