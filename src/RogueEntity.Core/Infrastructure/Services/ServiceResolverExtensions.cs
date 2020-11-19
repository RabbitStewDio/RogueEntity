using System;

namespace RogueEntity.Api.Services
{
    public static class ServiceResolverExtensions
    {
        public static IServiceResolver WithService<TServiceObject>(this IServiceResolver r)
            where TServiceObject : new()
        {
            r.Store(new TServiceObject());
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

        public static Lazy<TTarget> As<TTarget, TSource>(this Lazy<TSource> l) where TSource: TTarget
        {
            return new Lazy<TTarget>(() => l.Value);
        }
        
        public static Lazy<TTarget> Map<TTarget, TSource>(this Lazy<TSource> l, Func<TSource, TTarget> fn)
        {
            return new Lazy<TTarget>(() => fn(l.Value));
        }
    }
}