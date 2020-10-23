using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class DynamicSenseLayerFactory<TGameContext, TItemId> : ISenseLayerFactory<TGameContext>
        where TItemId : IEntityKey
        where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>
    {
        readonly MapLayer layer;
        readonly List<int> cachedZLevels;
        
        public DynamicSenseLayerFactory(MapLayer layer)
        {
            this.layer = layer;
            this.cachedZLevels = new List<int>();
        }

        public void Start(TGameContext context, ISensePropertiesSystem<TGameContext> system)
        {
            if (!context.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty += system.OnPositionDirty;
        }

        public void PrepareLayers(TGameContext context, ISensePropertiesSystem<TGameContext> system)
        {
            if (!context.TryGetGridDataFor(layer, out var gridMapDataContext))
            {
                return;
            }

            gridMapDataContext.QueryActiveZLevels(cachedZLevels);

            foreach (var z in cachedZLevels)
            {
                if (!gridMapDataContext.TryGetMap(z, out _))
                {
                    // If the map no longer contains the z-layer we previously seen,
                    // kick it out from the system for good.
                    if (system.TryGetData(z, out var mlx))
                    {
                        mlx.RemoveLayer(layer);
                    }
                    continue;
                }

                var ml = system.GetOrCreate(z);
                if (!ml.IsDefined(layer))
                {
                    var proc = new SensePropertiesDataProcessor<TGameContext, TItemId>(layer,
                                                                                       z,
                                                                                       system.OffsetX,
                                                                                       system.OffsetY,
                                                                                       system.TileWidth, 
                                                                                       system.TileHeight);
                    ml.AddProcess(layer, proc);
                }
            }
        }

        public void Stop(TGameContext context, ISensePropertiesSystem<TGameContext> system)
        {
            if (!context.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty -= system.OnPositionDirty;
        }
    }
}