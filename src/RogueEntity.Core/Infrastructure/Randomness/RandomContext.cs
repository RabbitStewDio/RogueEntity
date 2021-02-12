using EnTTSharp.Entities;
using System.Runtime.CompilerServices;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public static class RandomContext
    {
        public static ulong MakeSeed<TEntity>(ulong baseSeed,
                                              TEntity entity,
                                              int seedVariance)
            where TEntity : IRandomSeedSource
        {
            unchecked
            {
                var retval = baseSeed;
                retval = Combine(retval, ((ulong)entity.AsRandomSeed()) << 32);
                retval = Combine(retval, (ulong)seedVariance);
                return retval;
            }
        }
        
        public static ulong MakeSeed(ulong baseSeed,
                                              int seedVariance)
        {
            unchecked
            {
                var retval = baseSeed;
                retval = Combine(retval, (ulong)seedVariance);
                return retval;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Combine(ulong a, ulong b)
        {
            return (a * 499) ^ b;
        }

        public static EntityRandomSeedSource<TEntityKey> ToRandomSeedSource<TEntityKey>(this TEntityKey k)
            where TEntityKey : IEntityKey
        {
            return new EntityRandomSeedSource<TEntityKey>(k);
        }
    }
}
