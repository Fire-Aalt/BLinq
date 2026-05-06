using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly Dictionary<string, IReadOnlyDictionary<FieldDefinition, VariableDefinition>> _rewrittenCaptureLocals = new();
        private readonly Dictionary<string, bool> _nativeDelegateMethodCache = new();
        private readonly Dictionary<string, IReadOnlyList<TypeReference>> _nativeDelegateInterfaceCache = new();
        private readonly Dictionary<string, IReadOnlyList<MethodDefinition>> _targetCandidateCache = new();
        private readonly Dictionary<string, bool> _unmanagedTypeCache = new();

        public override Unity.CompilationPipeline.Common.ILPostProcessing.ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            return compiledAssembly.Name == BLinqAssemblyName ||
                   compiledAssembly.References.Any(r => Path.GetFileNameWithoutExtension(r) == BLinqAssemblyName);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var diagnostics = new List<DiagnosticMessage>();
            if (!MightContainNativeDelegateCall(compiledAssembly))
            {
                return new ILPostProcessResult(null, diagnostics);
            }

            ClearProcessCaches();
            var assembly = AssemblyDefinitionFor(compiledAssembly);
            if (!HasCandidateNativeDelegateMemberReference(assembly.MainModule))
            {
                return new ILPostProcessResult(null, diagnostics);
            }

            var modified = false;

            foreach (var type in assembly.MainModule.Types)
            {
                modified |= ProcessType(type, diagnostics);
            }

            if (!modified)
            {
                return new ILPostProcessResult(null, diagnostics);
            }

            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            assembly.Write(pe, new WriterParameters
            {
                WriteSymbols = true,
                SymbolStream = pdb,
                SymbolWriterProvider = new PortablePdbWriterProvider(),
            });

            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), diagnostics);
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
            if (!ContainsCandidateNativeDelegateCall(method))
            {
                return false;
            }

            var modified = false;
            _rewrittenEnumeratorTypes.Clear();
            _ambiguousRewrittenEnumeratorTypes.Clear();
            _rewrittenLocalTypes.Clear();
            _ambiguousRewrittenLocalTypes.Clear();
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
                    if (TryRewriteNativeDelegateCall(method, instruction, genericCall, diagnostics))
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
                method.Body.OptimizeMacros();
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
        }

        private static bool MightContainNativeDelegateCall(ICompiledAssembly compiledAssembly)
        {
            var peData = compiledAssembly.InMemoryAssembly.PeData;
            return ContainsAscii(peData, "BLinqExtensions") &&
                (ContainsAscii(peData, "Func`") || ContainsAscii(peData, "Action"));
        }

        private static bool ContainsAscii(byte[] data, string value)
        {
            if (data == null || data.Length < value.Length)
            {
                return false;
            }

            for (var i = 0; i <= data.Length - value.Length; i++)
            {
                var matched = true;
                for (var j = 0; j < value.Length; j++)
                {
                    if (data[i + j] != (byte)value[j])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCandidateNativeDelegateMemberReference(ModuleDefinition module)
        {
            foreach (var memberReference in module.GetMemberReferences())
            {
                if (memberReference is MethodReference methodReference &&
                    IsPotentialNativeDelegateMethodReference(methodReference))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCandidateNativeDelegateCall(MethodDefinition method)
        {
            if (!method.HasBody)
            {
                return false;
            }

            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call &&
                    instruction.Operand is MethodReference methodReference &&
                    IsPotentialNativeDelegateMethodReference(methodReference))
                {
                    return true;
                }
            }

            return false;
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
