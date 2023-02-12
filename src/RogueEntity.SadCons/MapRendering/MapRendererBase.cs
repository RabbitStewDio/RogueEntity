using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using SadConsole;

namespace RogueEntity.SadCons.MapRendering
{
    public abstract class MapRendererBase
    {
        public bool Render(Position observerAtCenter, Console console)
        {
            if (observerAtCenter.IsInvalid)
            {
                return false;
            }

            var screenCenter = new GridPosition2D(console.Width / 2, console.Height / 2);
            var mapOrigin = observerAtCenter.ToGridXY() - screenCenter;
            foreach (var (x, y) in new RectangleContents(0, 0, console.Width, console.Height))
            {
                var pos = Position.Of(MapLayer.Indeterminate, mapOrigin.X + x, mapOrigin.Y + y, observerAtCenter.Z);
                var cr = QueryCell(pos);
                cr.ApplyTo(console.GetCellAt(x, y));
            }

            return true;
        }

        protected abstract ConsoleRenderData QueryCell(Position pos);
    }
}
