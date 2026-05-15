using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace FireAlt.BLinq.CodeGen
{
    internal sealed partial class ILPostProcessor
    {
        private static AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly)
        {
            var resolver = new PostProcessorAssemblyResolver(compiledAssembly);
            var readerParameters = new ReaderParameters
            {
                SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData),
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = resolver,
                ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
                ReadingMode = ReadingMode.Deferred,
            };

            var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData);
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);
            resolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);
            return assemblyDefinition;
        }
        private sealed class PostProcessorAssemblyResolver : IAssemblyResolver
        {
            private readonly ICompiledAssembly _compiledAssembly;
            private readonly Dictionary<string, HashSet<string>> _referenceToPathMap = new();
            private readonly Dictionary<string, AssemblyDefinition> _cache = new();
            private readonly Dictionary<string, string> _assemblyFullNameCache = new();
            private readonly HashSet<string> _failedAssemblyResolutions = new();
            private readonly List<string> _referenceDirectories = new();
            private readonly HashSet<string> _referenceDirectorySet = new();
            private AssemblyDefinition _selfAssembly;

            public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
            {
                _compiledAssembly = compiledAssembly;

                foreach (var reference in compiledAssembly.References)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(reference);
                    if (!_referenceToPathMap.TryGetValue(assemblyName, out var paths))
                    {
                        paths = new HashSet<string>();
                        _referenceToPathMap.Add(assemblyName, paths);
                    }

                    paths.Add(reference);
                    var directory = Path.GetDirectoryName(reference);
                    if (directory != null && _referenceDirectorySet.Add(directory))
                    {
                        _referenceDirectories.Add(directory);
                    }
                }
            }

            public void Dispose()
            {
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, new ReaderParameters(ReadingMode.Deferred));
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                if (name.Name == _compiledAssembly.Name)
                {
                    return _selfAssembly;
                }

                if (_failedAssemblyResolutions.Contains(name.FullName))
                {
                    return null;
                }

                var fileName = FindFile(name);
                if (fileName == null)
                {
                    _failedAssemblyResolutions.Add(name.FullName);
                    return null;
                }

                if (_cache.TryGetValue(fileName, out var assembly))
                {
                    return assembly;
                }

                parameters.AssemblyResolver = this;
                var stream = MemoryStreamFor(fileName);
                var pdb = fileName + ".pdb";
                if (File.Exists(pdb))
                {
                    parameters.SymbolStream = MemoryStreamFor(pdb);
                }

                assembly = AssemblyDefinition.ReadAssembly(stream, parameters);
                _cache.Add(fileName, assembly);
                return assembly;
            }

            public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
            {
                _selfAssembly = assemblyDefinition;
            }

            private string FindFile(AssemblyNameReference name)
            {
                if (_referenceToPathMap.TryGetValue(name.Name, out var paths))
                {
                    if (paths.Count == 1)
                    {
                        foreach (var path in paths)
                        {
                            return path;
                        }
                    }

                    foreach (var path in paths)
                    {
                        if (!_assemblyFullNameCache.TryGetValue(path, out var fullName))
                        {
                            fullName = System.Reflection.AssemblyName.GetAssemblyName(path).FullName;
                            _assemblyFullNameCache.Add(path, fullName);
                        }

                        if (fullName == name.FullName)
                        {
                            return path;
                        }
                    }
                }

                foreach (var directory in _referenceDirectories)
                {
                    var candidate = Path.Combine(directory, name.Name + ".dll");
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                return null;
            }

            private static MemoryStream MemoryStreamFor(string fileName)
            {
                return new MemoryStream(File.ReadAllBytes(fileName));
            }
        }

        private sealed class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
        {
            public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
            {
                return new PostProcessorReflectionImporter(module);
            }
        }

        private sealed class PostProcessorReflectionImporter : DefaultReflectionImporter
        {
            public PostProcessorReflectionImporter(ModuleDefinition module)
                : base(module)
            {
            }

            public override AssemblyNameReference ImportReference(System.Reflection.AssemblyName reference)
            {
                var importedReference = base.ImportReference(reference);
                if (importedReference.Name == "System.Private.CoreLib")
                {
                    importedReference.Name = "mscorlib";
                }

                return importedReference;
            }
        }
    }
}
