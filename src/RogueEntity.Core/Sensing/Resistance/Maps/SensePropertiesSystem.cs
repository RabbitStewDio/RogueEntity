using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesSystem<TGameContext>: ISensePropertiesSource, ISensePropertiesSystem<TGameContext>
    {
        readonly SensePropertiesLayerStore<TGameContext> propertiesMap;
        readonly IAddByteBlitter blitter;
        readonly List<ISenseLayerFactory<TGameContext>> layerFactories2; 

        public SensePropertiesSystem(IAddByteBlitter blitter = default)
        {
            this.propertiesMap = new SensePropertiesLayerStore<TGameContext>();
            this.blitter = blitter ?? new DefaultAddByteBlitter();
            this.layerFactories2 = new List<ISenseLayerFactory<TGameContext>>();
        }

        public void OnPositionDirty(object source, PositionDirtyEventArgs args)
        {
            propertiesMap.MarkDirty(args.Layer, EntityGridPosition.From(args.Position));
        }

        public void Start(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.Start(context, this);
            }
        }
        
        public void Stop(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.Stop(context, this);
            }

            foreach (var l in propertiesMap.Layers)
            {
                propertiesMap.ClearLayer(l);
            }
        }

        public ReadOnlyListWrapper<int> DefinedLayers => propertiesMap.Layers;

        public void ProcessSenseProperties(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.PrepareLayers(context, this);
            }
            propertiesMap.Process(context);
        }

        public bool TryGet(int z, out IReadOnlyMapData<SenseProperties> data)
        {
            if (propertiesMap.TryGetLayer(z, out var layerData))
            {
                data = layerData;
                return true;
            }

            data = default;
            return false;
        }

        public void Remove(int z)
        {
            propertiesMap.ClearLayer(z);
        }
        
        public bool TryGet(int z, out SensePropertiesMap<TGameContext> data)
        {
            return propertiesMap.TryGetLayer(z, out data);
        }
        
        public bool TryGetOrCreate(int z, int width, int height, out SensePropertiesMap<TGameContext> data)
        {
            if (propertiesMap.TryGetLayer(z, out data))
            {
                return true;
            }
            
            data = new SensePropertiesMap<TGameContext>(blitter, width, height);
            propertiesMap.SetLayer(z, data);
            return true;
        }
        
        public void AddSenseLayerFactory(ISenseLayerFactory<TGameContext> layerHandler)
        {
            layerFactories2.Add(layerHandler);
        }
    }
}