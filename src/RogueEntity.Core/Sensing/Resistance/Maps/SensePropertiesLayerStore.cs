using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesLayerStore<TGameContext, TSense>
    {
        readonly Dictionary<int, SensePropertiesMap<TGameContext, TSense>> layers;

        public SensePropertiesLayerStore()
        {
            this.layers = new Dictionary<int, SensePropertiesMap<TGameContext, TSense>>();
        }

        public ReadOnlyListWrapper<int> ZLayers => layers.Keys.ToList();
        
        public void RemoveLayer(int z)
        {
            layers.Remove(z);
        }

        public void DefineLayer(int z, [NotNull] SensePropertiesMap<TGameContext, TSense> layerData)
        {
            if (z < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            layers[z] = layerData ?? throw new ArgumentNullException(nameof(layerData));
        }

        public bool TryGetLayer(int z, out SensePropertiesMap<TGameContext, TSense> layerData)
        {
            return layers.TryGetValue(z, out layerData);
        }

        public bool MarkDirty(EntityGridPosition pos)
        {
            if (TryGetLayer(pos.GridZ, out var d))
            {
                d.MarkDirty(pos);
                return true;
            }

            return false;
        }

        public void ResetDirtyFlags()
        {
            foreach (var d in layers.Values)
            {
                d.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            foreach (var d in layers.Values)
            {
                d.Process(c);
            }
        }
    }
}