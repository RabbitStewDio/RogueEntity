using System;

namespace RogueEntity.Core.Infrastructure.Services
{
    public static class ServiceResolverExtensions
    {
        public static IServiceResolver WithService<TServiceObject>(this IServiceResolver r)
            where TServiceObject : new()
        {
            r.Store<TServiceObject>(new TServiceObject());
            return r;
        }

        public static IServiceResolver WithService<TServiceObject>(this IServiceResolver r, in TServiceObject service)
        {
            r.Store(service);
            return r;
        }

        public static IServiceResolver WithService<TServiceObject>(this IServiceResolver r, in TServiceObject service, params Type[] alternativeTypes)
        {
            r.Store(service);
            foreach (var t in alternativeTypes)
            {
                r.Store(t, service);
            }

            return r;
        }

    }
}