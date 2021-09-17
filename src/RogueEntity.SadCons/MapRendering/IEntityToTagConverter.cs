using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.SadCons.MapRendering
{
    public interface IEntityToTagConverter
    {
        public MapLayer Layer { get; }
        public bool TryFetchTag(Position p, out WorldEntityTag t);
    }
}
