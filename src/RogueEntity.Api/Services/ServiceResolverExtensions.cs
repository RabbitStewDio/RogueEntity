using RogueEntity.Api.Utils;

namespace RogueEntity.Api.Services
{
    public static class ServiceResolverExtensions
    {
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
