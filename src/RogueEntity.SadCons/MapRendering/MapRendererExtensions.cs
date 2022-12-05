using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using System;

namespace RogueEntity.SadCons.MapRendering
{
    public static class MapRendererExtensions
    {
        public static ConsoleRenderLayer<TMapData> AddGridLayer<TMapData>(this LegacyMapRenderer r, MapLayer layer, IServiceResolver serviceResolver)
            where TMapData : struct, IEntityKey
        {
            var l = CreateRendererFor<TMapData>(layer, serviceResolver);
            r.AddLayer(l);
            return l;
        }

        public static ConsoleRenderLayer<TMapData> CreateRendererFor<TMapData>(MapLayer layer, IServiceResolver serviceResolver)
            where TMapData : struct, IEntityKey
        {
            var map = serviceResolver.Resolve<IMapContext<TMapData>>();
            if (map.TryGetMapDataFor(layer, out var layerData))
            {
                return new ConsoleRenderLayer<TMapData>(layerData, serviceResolver.Resolve<IItemResolver<TMapData>>());
            }

            throw new ArgumentException("Unable to create renderer for layer " + layer);
        }
    }
}
