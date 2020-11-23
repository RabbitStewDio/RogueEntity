using System;
using System.Collections.Generic;
using System.Text;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Core.Utils;

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

        public bool TryResolve<TServiceObject>(out TServiceObject o)
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

        public void Store(Type t, object service)
        {
            if (!t.IsInstanceOfType(service))
            {
                throw new ArgumentException();
            }

            backend[t] = service;
        }

        public void Store<TServiceObject>(in TServiceObject service)
        {
            backend[typeof(TServiceObject)] = service;
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
                if (!backend.ContainsKey(p.Key))
                {
                    b.Append("- ");
                    b.Append(p.Key);
                    b.AppendLine();

                    if (p.Value is ITraceableObject to)
                    {
                        b.Append("  first allocated at ");
                        b.Append(to.TraceInfo);
                    }
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