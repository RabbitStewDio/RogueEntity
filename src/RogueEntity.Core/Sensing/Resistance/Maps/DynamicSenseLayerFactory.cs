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
            if (!context.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.QueryActiveZLevels(cachedZLevels);

            foreach (var z in cachedZLevels)
            {
                if (!gdc.TryGetMap(z, out var map))
                {
                    if (system.TryGet(z, out var mlx))
                    {
                        mlx.RemoveProcess(layer, z);
                    }
                    continue;
                }
                
                if (!TryGetOrCreate(system, z, map.Width, map.Height, out var ml))
                {
                    continue;
                }
                
                if (!ml.IsDefined(layer, z))
                {
                    var proc = new SensePropertiesDataProcessor<TGameContext, TItemId>(ml.Width,
                                                                                       ml.Height,
                                                                                       layer,
                                                                                       z,
                                                                                       64, 64);
                    ml.AddProcess(layer, z, proc);
                }
            }
        }

        bool TryGetOrCreate(ISensePropertiesSystem<TGameContext> system,
                            int z,
                            int width,
                            int height,
                            out SensePropertiesMap<TGameContext> ml)
        {
            if (system.TryGet(z, out ml))
            {
                if (ml.Width == width || ml.Height == height)
                {
                    return true;
                }

                system.Remove(z);
                ml = null;
            }

            return system.TryGetOrCreate(z, width, height, out ml);
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