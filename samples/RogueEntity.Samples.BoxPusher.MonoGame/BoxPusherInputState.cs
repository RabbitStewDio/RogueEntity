using EnTTSharp;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherInputState
    {
        public Optional<EntityGridPosition> HoverPosition { get; set; }
        public Optional<PlayerObserver> PlayerObserver { get; set; }
        public Action QuitInitiated;

        public void NotifyQuitInitiated()
        {
            QuitInitiated?.Invoke();
        }
    }
}
