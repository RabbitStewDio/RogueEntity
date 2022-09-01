using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using System;
using System.Collections.Generic;

namespace RogueEntity.SadCons.MapRendering
{
    public class LegacyMapRenderer
    {
        readonly List<IConsoleRenderLayer> layers;

        public LegacyMapRenderer()
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

        protected virtual ConsoleRenderData QueryCell(Position pos)
        {
            ConsoleRenderData cr = default;
            foreach (var l in layers)
            {
                if (l.Get(pos).TryGetValue(out var r))
                {
                    cr = cr.MergeWith(r);
                }
            }

            return cr;
        }
    }
}
