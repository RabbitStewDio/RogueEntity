using System;

namespace RogueEntity.Core.Infrastructure.Services
{
    public interface IServiceResolver
    {
        bool TryResolve<TServiceObject>(out TServiceObject o);
        TServiceObject Resolve<TServiceObject>();
        void Store(Type t, object service);
        void Store<TServiceObject>(in TServiceObject service);
        Lazy<T> ResolveToReference<T>();

        void ValidatePromisesCanResolve();
    }
}