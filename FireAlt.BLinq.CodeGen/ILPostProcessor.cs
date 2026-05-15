using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace FireAlt.BLinq.CodeGen
{
    internal sealed partial class ILPostProcessor : Unity.CompilationPipeline.Common.ILPostProcessing.ILPostProcessor
    {
        private const string BLinqAssemblyName = "FireAlt.BLinq";
        private const string NativeDelegateMethodAttributeTypeName = "FireAlt.BLinq.NativeDelegateMethodAttribute";

        private int _adapterIndex;
        private readonly Dictionary<string, TypeReference> _rewrittenEnumeratorTypes = new();
        private readonly HashSet<string> _ambiguousRewrittenEnumeratorTypes = new();
        private readonly Dictionary<VariableDefinition, TypeReference> _rewrittenLocalTypes = new();
        private readonly HashSet<VariableDefinition> _ambiguousRewrittenLocalTypes = new();
        private readonly Dictionary<VariableDefinition, VariableDefinition> _activeLocalAliases = new();
        private readonly Dictionary<string, IReadOnlyDictionary<FieldDefinition, VariableDefinition>> _rewrittenCaptureLocals = new();
        private readonly Dictionary<string, bool> _nativeDelegateMethodCache = new();
        private readonly Dictionary<string, IReadOnlyList<TypeReference>> _nativeDelegateInterfaceCache = new();
        private readonly Dictionary<string, IReadOnlyList<MethodDefinition>> _targetCandidateCache = new();
        private readonly Dictionary<string, bool> _unmanagedTypeCache = new();
        private readonly Dictionary<string, MethodDefinition> _methodResolveCache = new();
        private readonly Dictionary<string, TypeDefinition> _typeResolveCache = new();
        private readonly Dictionary<string, MethodDefinition> _interfaceMethodCache = new();
        private ProfilingSession _profile;

        public override Unity.CompilationPipeline.Common.ILPostProcessing.ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            if (compiledAssembly.Name == BLinqAssemblyName)
            {
                return true;
            }

            foreach (var reference in compiledAssembly.References)
            {
                if (Path.GetFileNameWithoutExtension(reference) == BLinqAssemblyName)
                {
                    return true;
                }
            }

            return false;
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var diagnostics = new List<DiagnosticMessage>();
            var profile = new ProfilingSession(compiledAssembly.Name);
            _profile = profile;
            var reportProfile = false;
            var modified = false;

            try
            {
                using (profile.Measure("PE prefilter"))
                {
                    if (!MightContainNativeDelegateCall(compiledAssembly))
                    {
                        return new ILPostProcessResult(null, diagnostics);
                    }
                }

                reportProfile = true;
                ClearProcessCaches();

                AssemblyDefinition assembly;
                using (profile.Measure("Read assembly"))
                {
                    assembly = AssemblyDefinitionFor(compiledAssembly);
                }

                using (profile.Measure("Process methods"))
                {
                    foreach (var type in assembly.MainModule.Types)
                    {
                        modified |= ProcessType(type, diagnostics);
                    }
                }

                if (!modified)
                {
                    return new ILPostProcessResult(null, diagnostics);
                }

                var pe = new MemoryStream();
                var pdb = new MemoryStream();
                using (profile.Measure("Write assembly"))
                {
                    assembly.Write(pe, new WriterParameters
                    {
                        WriteSymbols = true,
                        SymbolStream = pdb,
                        SymbolWriterProvider = new PortablePdbWriterProvider(),
                    });
                }

                return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), diagnostics);
            }
            finally
            {
                if (reportProfile)
                {
                    profile.Report(modified);
                }

                _profile = null;
            }
        }

        private bool ProcessType(TypeDefinition type, List<DiagnosticMessage> diagnostics)
        {
            if (IsGeneratedAdapterType(type))
            {
                return false;
            }

            var modified = false;

            foreach (var method in type.Methods)
            {
                if (method.HasBody)
                {
                    modified |= ProcessMethod(method, diagnostics);
                }
            }

            foreach (var nestedType in type.NestedTypes)
            {
                modified |= ProcessType(nestedType, diagnostics);
            }

            return modified;
        }

        private bool ProcessMethod(MethodDefinition method, List<DiagnosticMessage> diagnostics)
        {
            var modified = false;
            _rewrittenEnumeratorTypes.Clear();
            _ambiguousRewrittenEnumeratorTypes.Clear();
            _rewrittenLocalTypes.Clear();
            _ambiguousRewrittenLocalTypes.Clear();
            _activeLocalAliases.Clear();
            _rewrittenCaptureLocals.Clear();
            var instructions = method.Body.Instructions;

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction.OpCode != OpCodes.Call || instruction.Operand is not MethodReference call)
                {
                    continue;
                }

                if (call is GenericInstanceMethod genericCall &&
                    IsPotentialNativeDelegateMethodReference(genericCall))
                {
                    bool rewritten;
                    using (MeasureStage("Rewrite delegate call"))
                    {
                        rewritten = TryRewriteNativeDelegateCall(method, instruction, genericCall, diagnostics);
                    }

                    if (rewritten)
                    {
                        modified = true;
                    }

                    continue;
                }

                modified |= TryRewriteMethodReference(method.Module, method, instruction, call);
            }

            if (_rewrittenEnumeratorTypes.Count != 0 ||
                _rewrittenLocalTypes.Count != 0)
            {
                modified |= TryRewriteVariableTypes(method.Module, method.Body.Variables);
            }

            if (modified)
            {
                using (MeasureStage("Optimize method macros"))
                {
                    method.Body.OptimizeMacros();
                }
            }

            return modified;
        }

        private static bool IsGeneratedAdapterType(TypeDefinition type)
        {
            return type.Namespace == "FireAlt.BLinq.Generated" ||
                type.Name.StartsWith("__BLinqDelegateAdapter_");
        }

        private bool ProcessMethodPreservingRewriteState(MethodDefinition method, List<DiagnosticMessage> diagnostics)
        {
            var rewrittenEnumeratorTypes = new Dictionary<string, TypeReference>(_rewrittenEnumeratorTypes);
            var ambiguousRewrittenEnumeratorTypes = new HashSet<string>(_ambiguousRewrittenEnumeratorTypes);
            var rewrittenLocalTypes = new Dictionary<VariableDefinition, TypeReference>(_rewrittenLocalTypes);
            var ambiguousRewrittenLocalTypes = new HashSet<VariableDefinition>(_ambiguousRewrittenLocalTypes);
            var activeLocalAliases = new Dictionary<VariableDefinition, VariableDefinition>(_activeLocalAliases);
            var rewrittenCaptureLocals = new Dictionary<string, IReadOnlyDictionary<FieldDefinition, VariableDefinition>>(_rewrittenCaptureLocals);

            try
            {
                return ProcessMethod(method, diagnostics);
            }
            finally
            {
                _rewrittenEnumeratorTypes.Clear();
                foreach (var pair in rewrittenEnumeratorTypes)
                {
                    _rewrittenEnumeratorTypes.Add(pair.Key, pair.Value);
                }

                _ambiguousRewrittenEnumeratorTypes.Clear();
                foreach (var value in ambiguousRewrittenEnumeratorTypes)
                {
                    _ambiguousRewrittenEnumeratorTypes.Add(value);
                }

                _rewrittenLocalTypes.Clear();
                foreach (var pair in rewrittenLocalTypes)
                {
                    _rewrittenLocalTypes.Add(pair.Key, pair.Value);
                }

                _ambiguousRewrittenLocalTypes.Clear();
                foreach (var value in ambiguousRewrittenLocalTypes)
                {
                    _ambiguousRewrittenLocalTypes.Add(value);
                }

                _activeLocalAliases.Clear();
                foreach (var pair in activeLocalAliases)
                {
                    _activeLocalAliases.Add(pair.Key, pair.Value);
                }

                _rewrittenCaptureLocals.Clear();
                foreach (var pair in rewrittenCaptureLocals)
                {
                    _rewrittenCaptureLocals.Add(pair.Key, pair.Value);
                }
            }
        }

        private void ClearProcessCaches()
        {
            _nativeDelegateMethodCache.Clear();
            _nativeDelegateInterfaceCache.Clear();
            _targetCandidateCache.Clear();
            _unmanagedTypeCache.Clear();
            _methodResolveCache.Clear();
            _typeResolveCache.Clear();
            _interfaceMethodCache.Clear();
        }

        private System.IDisposable MeasureStage(string stage)
        {
            return _profile == null
                ? NoopProfileScope.Instance
                : _profile.Measure(stage);
        }

        private MethodDefinition ResolveMethod(MethodReference method)
        {
            if (method == null)
            {
                return null;
            }

            var key = $"{method.DeclaringType.Scope}|{method.FullName}";
            if (_methodResolveCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            MethodDefinition resolved;
            using (MeasureStage("Resolve methods"))
            {
                resolved = method.Resolve();
            }

            _methodResolveCache[key] = resolved;
            return resolved;
        }

        private TypeDefinition ResolveType(TypeReference type)
        {
            if (type == null)
            {
                return null;
            }

            var elementType = type.GetElementType();
            var key = $"{elementType.Scope}|{elementType.FullName}";
            if (_typeResolveCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            TypeDefinition resolved;
            using (MeasureStage("Resolve types"))
            {
                resolved = elementType.Resolve();
            }

            _typeResolveCache[key] = resolved;
            return resolved;
        }

        private MethodDefinition ResolveInterfaceMethod(TypeReference interfaceType)
        {
            var key = interfaceType.FullName;
            if (_interfaceMethodCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var definition = ResolveType(interfaceType);
            if (definition == null)
            {
                return null;
            }

            foreach (var method in definition.Methods)
            {
                if (!method.IsSpecialName && method.HasThis)
                {
                    _interfaceMethodCache[key] = method;
                    return method;
                }
            }

            return null;
        }

        private static bool MightContainNativeDelegateCall(ICompiledAssembly compiledAssembly)
        {
            var peData = compiledAssembly.InMemoryAssembly.PeData;
            return ContainsAscii(peData, "BLinqExtensions", "Func`", "Action");
        }

        private static bool ContainsAscii(byte[] data, string required, string eitherLeft, string eitherRight)
        {
            if (data == null || data.Length < required.Length)
            {
                return false;
            }

            var foundRequired = false;
            var foundEither = false;
            for (var i = 0; i < data.Length && (!foundRequired || !foundEither); i++)
            {
                if (!foundRequired && MatchesAscii(data, i, required))
                {
                    foundRequired = true;
                }

                if (!foundEither &&
                    (MatchesAscii(data, i, eitherLeft) || MatchesAscii(data, i, eitherRight)))
                {
                    foundEither = true;
                }
            }

            return foundRequired && foundEither;
        }

        private static bool MatchesAscii(byte[] data, int start, string value)
        {
            if (start + value.Length > data.Length)
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (data[start + i] != (byte)value[i])
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class NoopProfileScope : System.IDisposable
        {
            public static readonly NoopProfileScope Instance = new();

            public void Dispose()
            {
            }
        }

        private static bool IsPotentialNativeDelegateMethodReference(MethodReference methodReference)
        {
            if (methodReference.DeclaringType.FullName != "FireAlt.BLinq.BLinqExtensions")
            {
                return false;
            }

            foreach (var parameter in methodReference.Parameters)
            {
                if (IsFuncOrActionDelegateType(parameter.ParameterType))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFuncOrActionDelegateType(TypeReference type)
        {
            switch (type)
            {
                case GenericInstanceType genericInstance:
                    return IsFuncOrActionDelegateType(genericInstance.ElementType);
                case RequiredModifierType requiredModifier:
                    return IsFuncOrActionDelegateType(requiredModifier.ElementType);
                case OptionalModifierType optionalModifier:
                    return IsFuncOrActionDelegateType(optionalModifier.ElementType);
                default:
                    return type.Namespace == "System" &&
                        (type.Name == "Action" ||
                            type.Name.StartsWith("Action`") ||
                            type.Name.StartsWith("Func`"));
            }
        }
    }
}
