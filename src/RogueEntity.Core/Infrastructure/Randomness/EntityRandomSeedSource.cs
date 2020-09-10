using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public readonly struct EntityRandomSeedSource<TEntityKey> : IRandomSeedSource where TEntityKey : IEntityKey
    {
        readonly TEntityKey entity;

        public EntityRandomSeedSource(TEntityKey entity)
        {
            this.entity = entity;
        }

        public int AsRandomSeed()
        {
            return entity.GetHashCode();
        }
    }
}