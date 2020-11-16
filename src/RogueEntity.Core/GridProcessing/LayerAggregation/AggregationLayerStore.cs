using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public class AggregationLayerStore<TGameContext, TSense>
    {
        readonly Dictionary<int, AggregatePropertiesLayer<TGameContext, TSense>> layers;

        public AggregationLayerStore()
        {
            this.layers = new Dictionary<int, AggregatePropertiesLayer<TGameContext, TSense>>();
        }

        public ReadOnlyListWrapper<int> ZLayers => layers.Keys.ToList();

        public void RemoveLayer(int z)
        {
            layers.Remove(z);
        }

        public void DefineLayer(int z, [NotNull] AggregatePropertiesLayer<TGameContext, TSense> layerData)
        {
            if (z < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            layers[z] = layerData ?? throw new ArgumentNullException(nameof(layerData));
        }

        public bool TryGetLayer(int z, out AggregatePropertiesLayer<TGameContext, TSense> layerData)
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