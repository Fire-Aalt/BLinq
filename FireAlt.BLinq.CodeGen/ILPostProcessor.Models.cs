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
            private readonly Dictionary<string, long> _elapsedTicks = new();
            private readonly Dictionary<string, int> _counts = new();

            private string AssemblyName { get; }

            public IDisposable Measure(string stage)
            {
                return new Scope(this, stage);
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

                foreach (var pair in _elapsedTicks)
                {
                    builder.Append("  ");
                    builder.Append(pair.Key);
                    builder.Append(": ");
                    builder.Append(FormatMilliseconds(pair.Value));
                    builder.Append("ms");
                    if (_counts.TryGetValue(pair.Key, out var count) && count > 1)
                    {
                        builder.Append(" (");
                        builder.Append(count);
                        builder.Append("x)");
                    }

                    builder.AppendLine();
                }

                Console.Write(builder.ToString());
            }

            private void Add(string stage, long ticks)
            {
                if (_elapsedTicks.TryGetValue(stage, out var existing))
                {
                    _elapsedTicks[stage] = existing + ticks;
                    _counts[stage]++;
                    return;
                }

                _elapsedTicks.Add(stage, ticks);
                _counts.Add(stage, 1);
            }

            private static string FormatMilliseconds(long ticks)
            {
                var milliseconds = ticks * 1000.0 / Stopwatch.Frequency;
                return milliseconds.ToString("0.###");
            }

            private readonly struct Scope : IDisposable
            {
                private readonly ProfilingSession _session;
                private readonly string _stage;
                private readonly long _start;

                public Scope(ProfilingSession session, string stage)
                {
                    _session = session;
                    _stage = stage;
                    _start = Stopwatch.GetTimestamp();
                }

                public void Dispose()
                {
                    _session.Add(_stage, Stopwatch.GetTimestamp() - _start);
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
