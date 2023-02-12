using EnTTSharp.Entities;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public interface ISpatialQueryLookup
    {
        bool TryGetQuery<TEntityKey, TComponent>([MaybeNullWhen(false)] out ISpatialQuery<TEntityKey, TComponent> q)
            where TEntityKey : struct, IEntityKey;
    }
}