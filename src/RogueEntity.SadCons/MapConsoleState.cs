using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.SadCons
{
    public class MapConsoleState
    {
        public PlayerTag PlayerId { get; set; }
        public int MapLevel { get; set; }
        public Rectangle MapViewPort { get; set; }
        public Position MouseHoverPosition { get; set; }
        
        public event EventHandler MouseSelection;
        public event EventHandler MouseContextMenu;
        public event EventHandler KeyboardPolling;

        public void FireMouseSelection() => MouseSelection?.Invoke(this, EventArgs.Empty);
        public void FireMouseContextMenu() => MouseContextMenu?.Invoke(this, EventArgs.Empty);
    }
}