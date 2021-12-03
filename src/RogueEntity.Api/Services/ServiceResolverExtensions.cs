using RogueEntity.Api.Utils;

namespace RogueEntity.Api.Services
{
    public static class ServiceResolverExtensions
    {
        public static T ResolveConfiguration<T>(this IServiceResolver r) 
            where T: new()
        {
            if (r.TryResolve(out T config))
            {
                return config;
            }

            var cfg = r.Resolve<IConfigurationHost>();
            return cfg.GetConfiguration<T>();
        }
        
        public static Optional<T> ResolveOptional<T>(this IServiceResolver r)
        {
            if (r.TryResolve(out T val))
            {
                return val;
            }

            return Optional.Empty();
        }
    }
}
