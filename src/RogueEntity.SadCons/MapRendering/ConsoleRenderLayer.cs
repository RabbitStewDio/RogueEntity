using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;

namespace RogueEntity.SadCons.MapRendering
{
    public class ConsoleRenderLayer<TMapData> : IConsoleRenderLayer
        where TMapData : struct, IEntityKey
    {
        readonly IGridMapDataContext<TMapData> layer;
        readonly IItemResolver<TMapData> itemResolver;
        readonly Dictionary<WorldEntityTag, ConsoleRenderData> rendererForTags;
        Optional<(int z, IReadOnlyDynamicDataView2D<TMapData> view)> cachedView;

        public ConsoleRenderLayer(IGridMapDataContext<TMapData> layer,
                                  IItemResolver<TMapData> itemResolver)
        {
            this.layer = layer;
            this.itemResolver = itemResolver;
            this.rendererForTags = new Dictionary<WorldEntityTag, ConsoleRenderData>();
            this.layer.ViewExpired += OnViewExpired;
        }

        void OnViewExpired(object sender, DynamicDataView3DEventArgs<TMapData> e)
        {
            cachedView = default;
        }

        public ConsoleRenderLayer<TMapData> WithRenderTemplate(WorldEntityTag tag, ConsoleRenderData cell)
        {
            this.rendererForTags[tag] = cell;
            return this;
        }

        public Optional<ConsoleRenderData> Get(Position p)
        {
            if (!RefreshCachedMap(p, out var mapView))
            {
                return default;
            }

            if (!mapView.TryGet(p.GridX, p.GridY, out var item) || item.IsEmpty)
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

        bool RefreshCachedMap(Position p, out IReadOnlyDynamicDataView2D<TMapData> view)
        {
            if (!cachedView.TryGetValue(out var x) || x.z != p.GridZ)
            {
                if (!layer.TryGetView(p.GridZ, out view))
                {
                    {
                        return false;
                    }
                }

                x = (p.GridZ, view);
                cachedView = x;
            }

            view = default;
            return true;
        }
    }
}
