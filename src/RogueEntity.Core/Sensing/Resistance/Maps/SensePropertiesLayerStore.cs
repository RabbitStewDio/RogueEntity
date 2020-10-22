using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesLayerStore<TGameContext>
    {
        readonly Dictionary<int, SensePropertiesMap<TGameContext>> layers;

        public SensePropertiesLayerStore()
        {
            this.layers = new Dictionary<int, SensePropertiesMap<TGameContext>>();
        }

        public ReadOnlyListWrapper<int> Layers => layers.Keys.ToList();
        
        public void ClearLayer(int z)
        {
            layers.Remove(z);
        }
        
        public void SetLayer(int z, SensePropertiesMap<TGameContext> layerData)
        {
            if (z < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            layers[z] = layerData;
        }

        public bool TryGetLayer(int z, out SensePropertiesMap<TGameContext> layerData)
        {
            return layers.TryGetValue(z, out layerData);
        }

        public bool MarkDirty(MapLayer l, EntityGridPosition pos)
        {
            if (TryGetLayer(pos.GridZ, out var d))
            {
                d.MarkDirty(l, pos);
                return true;
            }

            return false;
        }

        public void ResetDirtyFlags()
        {
            foreach (var d in layers.Values)
            {
                d?.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            foreach (var d in layers.Values)
            {
                d?.Process(c);
            }
        }
    }
}