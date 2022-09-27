using EnTTSharp.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public interface ISpatialQueryLookup
    {
        bool TryGetQuery<TEntityKey>([MaybeNullWhen(false)] out ISpatialQuery<TEntityKey> q)
            where TEntityKey : struct, IEntityKey;
    }
}