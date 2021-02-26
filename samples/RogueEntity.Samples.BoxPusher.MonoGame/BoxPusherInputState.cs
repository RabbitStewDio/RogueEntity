using Microsoft.Xna.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherInputState
    {
        public Optional<EntityGridPosition> HoverPosition { get; set; }
        public Optional<PlayerObserver> PlayerObserver { get; set; }
    }
}
