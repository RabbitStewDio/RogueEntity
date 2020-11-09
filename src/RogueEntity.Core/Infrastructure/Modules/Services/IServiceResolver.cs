using System;

namespace RogueEntity.Core.Infrastructure.Modules.Services
{
    public interface IServiceResolver
    {
        bool TryResolve<TServiceObject>(out TServiceObject o);
        TServiceObject Resolve<TServiceObject>();
        void Store<TServiceObject>(in TServiceObject service);
        Lazy<T> ResolveToReference<T>();

        void ValidatePromisesCanResolve();
    }
}