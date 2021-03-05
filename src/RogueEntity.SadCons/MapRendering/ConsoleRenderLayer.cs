using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;

namespace RogueEntity.SadCons.MapRendering
{
    public class ConsoleRenderLayer<TMapData> : IConsoleRenderLayer
        where TMapData : IEntityKey
    {
        readonly IGridMapDataContext<TMapData> layer;
        readonly IItemResolver<TMapData> itemResolver;
        readonly Dictionary<string, ConsoleRenderData> rendererForTags;

        public ConsoleRenderLayer(IGridMapDataContext<TMapData> layer,
                                  IItemResolver<TMapData> itemResolver)
        {
            this.layer = layer;
            this.itemResolver = itemResolver;
            this.rendererForTags = new Dictionary<string, ConsoleRenderData>();
            this.layer.ViewExpired += OnViewExpired;
        }

        void OnViewExpired(object sender, DynamicDataView3DEventArgs<TMapData> e)
        {
            cachedView = default;
        }

        public ConsoleRenderLayer<TMapData> WithRenderTemplate(string tag, ConsoleRenderData cell)
        {
            this.rendererForTags[tag] = cell;
            return this;
        }

        Optional<(int z, IReadOnlyDynamicDataView2D<TMapData> view)> cachedView;

        public Optional<ConsoleRenderData> Get(Position p)
        {
            if (!cachedView.TryGetValue(out var x) || x.z != p.GridZ)
            {
                if (!layer.TryGetView(p.GridZ, out var view))
                {
                    return default;
                }

                x = (p.GridZ, view);
                cachedView = x;
            }

            var (_, mapView) = x;
            var item = mapView[p.GridX, p.GridY];
            if (item.IsEmpty)
            {
                return default;
            }

            if (!itemResolver.TryResolve(item, out var itemDeclaration) ||
                !rendererForTags.TryGetValue(itemDeclaration.Tag, out var cellTemplate))
            {
                return default;
            }

            return cellTemplate;
        }
    }
}
