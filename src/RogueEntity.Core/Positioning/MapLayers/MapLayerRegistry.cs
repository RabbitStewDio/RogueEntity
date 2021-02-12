using RogueEntity.Api.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.MapLayers
{
    public interface IMapLayerRegistry
    {
        public bool TryGetValue(byte id, out MapLayer l);
        public ReadOnlyListWrapper<MapLayer> Layers { get; } 
    }
    
    public class MapLayerRegistry: IMapLayerRegistry
    {
        public readonly MapLayer Indeterminate; 
        
        readonly Dictionary<byte, MapLayer> layersById;
        readonly List<MapLayer> layers;

        public MapLayerRegistry()
        {
            layers = new List<MapLayer>()
            {
                Indeterminate
            };
            
            layersById = new Dictionary<byte, MapLayer>();
            Indeterminate = new MapLayer(0, nameof(Indeterminate));
        }

        public ReadOnlyListWrapper<MapLayer> Layers => layers;

        public MapLayer Create(string name)
        {
            byte b = 0;
            foreach (var f in layersById)
            {
                b = Math.Max(b, f.Key);
            }

            if (b == 255)
            {
                throw new ArgumentException("Max layers reached");
            }

            var mapLayer = new MapLayer((byte) (b + 1), name);
            Add(mapLayer);
            return mapLayer;
        }
        
        void Add(MapLayer l)
        {
            layersById.Add(l.LayerId, l);
            layers.Add(l);
        }

        public bool TryGetValue(byte id, out MapLayer l)
        {
            return layersById.TryGetValue(id, out l);
        }
    }
}