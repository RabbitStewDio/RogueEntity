using System;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Api.Services
{
    public interface IServiceResolver
    {
        bool TryResolve<TServiceObject>([MaybeNullWhen(false)] out TServiceObject o);
        TServiceObject Resolve<TServiceObject>();
        void Store(Type t, object service);
        void Store<TServiceObject>(in TServiceObject service);
        Lazy<T> ResolveToReference<T>();

        void ValidatePromisesCanResolve();
    }
}