using System;
using System.Collections.Generic;
using System.Text;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Core.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Infrastructure.Services
{
    public class DefaultServiceResolver : IServiceResolver, IDisposable
    {
        readonly Dictionary<Type, object> backend;
        readonly Dictionary<Type, object> promisedReferences;

        public DefaultServiceResolver()
        {
            backend = new Dictionary<Type, object>();
            promisedReferences = new Dictionary<Type, object>();
        }

        public bool TryResolve<TServiceObject>([MaybeNullWhen(false)] out TServiceObject o)
        {
            if (backend.TryGetValue(typeof(TServiceObject), out var raw))
            {
                if (raw is TServiceObject to)
                {
                    o = to;
                    return true;
                }
            }

            o = default;
            return false;
        }

        public TServiceObject Resolve<TServiceObject>()
        {
            if (TryResolve<TServiceObject>(out var o))
            {
                return o;
            }

            throw new ArgumentException("Unable to resolve service of type " + typeof(TServiceObject));
        }

        public void Store(Type t, object? service)
        {
            if (service == null || !t.IsInstanceOfType(service))
            {
                throw new ArgumentException();
            }

            if (backend.ContainsKey(t))
            {
                throw new ArgumentException("Redefinition of service type " + t);
            }

            backend[t] = service;
        }

        public void Store<TServiceObject>(in TServiceObject service)
        {
            Store(typeof(TServiceObject), service);
        }

        public Lazy<T> ResolveToReference<T>()
        {
            if (promisedReferences.TryGetValue(typeof(T), out var existingReference) &&
                existingReference is Lazy<T> lazy)
            {
                return lazy;
            }
            
            var l = new TraceableLazy<T>(Resolve<T>);
            promisedReferences.Add(typeof(T), l);
            return l;
        }

        public void ValidatePromisesCanResolve()
        {
            StringBuilder b = new StringBuilder();
            foreach (var p in promisedReferences)
            {
                if (backend.ContainsKey(p.Key))
                {
                    continue;
                }

                b.Append("- ");
                b.Append(p.Key);
                b.AppendLine();

                if (p.Value is ITraceableObject to &&
                    to.TryGetTraceInfo(out var ti))
                {
                    b.Append("  first allocated at ");
                    b.Append(ti);
                }
            }

            if (b.Length == 0)
            {
                return;
            }

            throw new ModuleInitializationException("Unable to fulfil promises made during module initialization. \n" +
                                                    "The following unregistered services were requested: \n" +
                                                    b);
        }

        public void Dispose()
        {
            foreach (var d in backend.Values)
            {
                if (d is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            promisedReferences.Clear();
            backend.Clear();
        }
    }
}