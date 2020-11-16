using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using RogueEntity.Api.Modules.Helpers;

namespace RogueEntity.Api.Services
{
    public interface IServiceResolverDelegate
    {
        bool TryResolve<TServiceObject>(out TServiceObject o);
    }
    
    public class DelegatingServiceResolver : IServiceResolver, IDisposable
    {
        readonly Dictionary<Type, object> backend;
        readonly HashSet<Type> promisedReferences;
        readonly IServiceResolverDelegate resolveDelegate;

        public DelegatingServiceResolver([NotNull] IServiceResolverDelegate resolveDelegate)
        {
            this.resolveDelegate = resolveDelegate ?? throw new ArgumentNullException(nameof(resolveDelegate));
            backend = new Dictionary<Type, object>();
            promisedReferences = new HashSet<Type>();
        }

        public bool TryResolve<TServiceObject>(out TServiceObject o)
        {
            if (backend.TryGetValue(typeof(TServiceObject), out var raw) && 
                raw is TServiceObject to)
            {
                o = to;
                return true;
            }

            return resolveDelegate.TryResolve(out o);
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
            promisedReferences.Add(typeof(T));
            return new Lazy<T>(Resolve<T>);
        }

        public void ValidatePromisesCanResolve()
        {
            StringBuilder b = new StringBuilder();
            foreach (var p in promisedReferences)
            {
                if (!backend.ContainsKey(p))
                {
                    b.Append("- ");
                    b.Append(p);
                    b.AppendLine();
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