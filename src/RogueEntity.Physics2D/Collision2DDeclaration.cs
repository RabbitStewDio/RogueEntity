using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Physics2D;

public readonly struct Collision2DDeclaration<TSelfId, TOtherId>
{
    public readonly ReadOnlyListWrapper<MapLayer> TargetLayers;

    public Collision2DDeclaration(ReadOnlyListWrapper<MapLayer> targetLayers)
    {
        this.TargetLayers = targetLayers;
    }
}