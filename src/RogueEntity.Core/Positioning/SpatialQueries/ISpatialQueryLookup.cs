using EnTTSharp.Entities;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public interface ISpatialQueryLookup
    {
        bool TryGetQuery<TEntityKey>(out ISpatialQuery<TEntityKey> q)
            where TEntityKey : IEntityKey;
    }
}