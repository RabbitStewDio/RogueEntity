using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.MapLayers
{
    public class MapLayerRegistry: IEnumerable<MapLayer>
    {
        readonly Dictionary<byte, MapLayer> layersById;

        public MapLayerRegistry()
        {
            layersById = new Dictionary<byte, MapLayer>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<MapLayer> IEnumerable<MapLayer>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Dictionary<byte, MapLayer>.ValueCollection.Enumerator GetEnumerator()
        {
            return layersById.Values.GetEnumerator();
        }

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
        }

        public bool TryGetValue(byte id, out MapLayer l)
        {
            return layersById.TryGetValue(id, out l);
        }
    }
}