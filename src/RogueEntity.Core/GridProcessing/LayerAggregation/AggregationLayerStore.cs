using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public class AggregationLayerStore<TAggregateType, TSourceType>
    {
        readonly Dictionary<int, AggregatePropertiesLayer<TAggregateType, TSourceType>> layers;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregateType>>? ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregateType>>? ViewExpired;

        public AggregationLayerStore()
        {
            this.layers = new Dictionary<int, AggregatePropertiesLayer<TAggregateType, TSourceType>>();
        }

        public ReadOnlyListWrapper<int> ZLayers => layers.Keys.ToList();

        public void RemoveLayer(int z)
        {
            if (layers.TryGetValue(z, out var view))
            {
                ViewExpired?.Invoke(this, new DynamicDataView3DEventArgs<TAggregateType>(z, view.AggregatedView));
                layers.Remove(z);
            }
        }

        public void DefineLayer(int z, AggregatePropertiesLayer<TAggregateType, TSourceType> layerData)
        {
            if (layers.TryGetValue(z, out var existingView))
            {
                ViewExpired?.Invoke(this, new DynamicDataView3DEventArgs<TAggregateType>(z, existingView.AggregatedView));
            }
            layers[z] = layerData ?? throw new ArgumentNullException(nameof(layerData));
            ViewCreated?.Invoke(this, new DynamicDataView3DEventArgs<TAggregateType>(z, layerData.AggregatedView));
        }

        public bool TryGetLayer(int z, out AggregatePropertiesLayer<TAggregateType, TSourceType> layerData)
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

        public void Process()
        {
            foreach (var d in layers.Values)
            {
                d.Process();
            }
        }
    }
}
