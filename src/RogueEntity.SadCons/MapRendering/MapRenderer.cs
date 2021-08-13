using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using SadConsole;
using Serilog;
using System;
using System.Collections.Generic;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons.MapRendering
{
    public class MapRenderer
    {
        readonly List<IConsoleRenderLayer> layers;
        static readonly ILogger Logger = SLog.ForContext<MapRenderer>();

        public MapRenderer()
        {
            layers = new List<IConsoleRenderLayer>();
        }

        public void AddLayer([NotNull] IConsoleRenderLayer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            this.layers.Add(layer);
        }

        public bool Render(Position observerAtCenter, Console console)
        {
            if (observerAtCenter.IsInvalid)
            {
                return false;
            }
            
            var screenCenter = new Position2D(console.Width / 2, console.Height / 2);
            var mapOrigin = observerAtCenter.ToGridXY() - screenCenter;
            foreach (var (x, y) in new RectangleContents(0, 0, console.Width, console.Height))
            {
                var pos = Position.Of(MapLayer.Indeterminate, mapOrigin.X + x, mapOrigin.Y + y, observerAtCenter.Z);
                ConsoleRenderData cr = default;
                foreach (var l in layers)
                {
                    if (l.Get(pos).TryGetValue(out var r))
                    {
                        cr = cr.MergeWith(r);
                    }
                }

                cr.ApplyTo(console.GetCellAt(x, y));
            }

            return true;
        }
   }
}
