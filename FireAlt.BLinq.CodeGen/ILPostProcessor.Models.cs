using System;
using System.Collections.Generic;
#if BLINQ_ILPP_PROFILE
using System.Diagnostics;
using System.Text;
#endif
using Mono.Cecil;
    
namespace FireAlt.BLinq.CodeGen
{
    internal sealed partial class ILPostProcessor
    {
        private sealed class AdapterInfo
        {
            public AdapterInfo(TypeReference adapterType, TypeReference interfaceType)
            {
                AdapterType = adapterType;
                InterfaceType = interfaceType;
            }

            public TypeReference AdapterType { get; }

            public TypeReference InterfaceType { get; }
        }

        private sealed class DelegateSignature
        {
            public DelegateSignature(IReadOnlyList<TypeReference> parameterTypes, TypeReference returnType)
            {
                ParameterTypes = parameterTypes;
                ReturnType = returnType;
            }

            public IReadOnlyList<TypeReference> ParameterTypes { get; }

            public TypeReference ReturnType { get; }
        }

        private sealed class TargetMethodInfo
        {
            public TargetMethodInfo(MethodReference call, TypeReference returnType)
            {
                Call = call;
                ReturnType = returnType;
            }

            public MethodReference Call { get; }

            public TypeReference ReturnType { get; }
        }

        private sealed class ProfilingSession
        {
            public ProfilingSession(string assemblyName)
            {
#if BLINQ_ILPP_PROFILE
                AssemblyName = assemblyName;
                _total = Stopwatch.StartNew();
#endif
            }

#if BLINQ_ILPP_PROFILE
            private readonly Stopwatch _total;
            private readonly ProfileNode _root = new("<root>");
            private readonly Stack<ProfileNode> _stack = new();

            private string AssemblyName { get; }

            public IDisposable Measure(string stage)
            {
                var parent = _stack.Count == 0 ? _root : _stack.Peek();
                var node = parent.GetOrAddChild(stage);
                _stack.Push(node);
                return new Scope(this, node);
            }

            public void Report(bool modified)
            {
                _total.Stop();
                var builder = new StringBuilder();
                builder.Append("[BLinq ILPP] ");
                builder.Append(AssemblyName);
                builder.Append(modified ? " modified" : " scanned");
                builder.Append(" total=");
                builder.Append(FormatMilliseconds(_total.ElapsedTicks));
                builder.AppendLine("ms");

                foreach (var child in _root.Children)
                {
                    AppendNode(builder, child, 1);
                }

                Console.Write(builder.ToString());
            }

            private void End(ProfileNode node, long ticks)
            {
                if (_stack.Count != 0 && ReferenceEquals(_stack.Peek(), node))
                {
                    _stack.Pop();
                }

                node.Add(ticks);
            }

            private static void AppendNode(StringBuilder builder, ProfileNode node, int depth)
            {
                for (var i = 0; i < depth; i++)
                {
                    builder.Append("  ");
                }

                builder.Append(node.Name);
                builder.Append(": ");
                builder.Append(FormatMilliseconds(node.ElapsedTicks));
                builder.Append("ms");

                var childTicks = node.ChildElapsedTicks;
                if (childTicks > 0)
                {
                    builder.Append(" self=");
                    builder.Append(FormatMilliseconds(node.ElapsedTicks - childTicks));
                    builder.Append("ms");
                }

                if (node.Count > 1)
                {
                    builder.Append(" (");
                    builder.Append(node.Count);
                    builder.Append("x)");
                }

                builder.AppendLine();

                foreach (var child in node.Children)
                {
                    AppendNode(builder, child, depth + 1);
                }
            }

            private static string FormatMilliseconds(long ticks)
            {
                var milliseconds = ticks * 1000.0 / Stopwatch.Frequency;
                return milliseconds.ToString("0.###");
            }

            private readonly struct Scope : IDisposable
            {
                private readonly ProfilingSession _session;
                private readonly ProfileNode _node;
                private readonly long _start;

                public Scope(ProfilingSession session, ProfileNode node)
                {
                    _session = session;
                    _node = node;
                    _start = Stopwatch.GetTimestamp();
                }

                public void Dispose()
                {
                    _session.End(_node, Stopwatch.GetTimestamp() - _start);
                }
            }

            private sealed class ProfileNode
            {
                private readonly List<ProfileNode> _children = new();
                private readonly Dictionary<string, ProfileNode> _childByName = new();

                public ProfileNode(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public long ElapsedTicks { get; private set; }

                public int Count { get; private set; }

                public IReadOnlyList<ProfileNode> Children => _children;

                public long ChildElapsedTicks
                {
                    get
                    {
                        long ticks = 0;
                        foreach (var child in _children)
                        {
                            ticks += child.ElapsedTicks;
                        }

                        return ticks;
                    }
                }

                public ProfileNode GetOrAddChild(string name)
                {
                    if (_childByName.TryGetValue(name, out var child))
                    {
                        return child;
                    }

                    child = new ProfileNode(name);
                    _childByName.Add(name, child);
                    _children.Add(child);
                    return child;
                }

                public void Add(long ticks)
                {
                    ElapsedTicks += ticks;
                    Count++;
                }
            }
#else
            public IDisposable Measure(string stage)
            {
                return NoopScope.Instance;
            }

            public void Report(bool modified)
            {
            }

            private sealed class NoopScope : IDisposable
            {
                public static readonly NoopScope Instance = new();

                public void Dispose()
                {
                }
            }
#endif
        }
    }
}
