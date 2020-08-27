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

        public void Add(MapLayer l)
        {
            layersById[l.LayerId] = l;
        }

        public bool TryGetValue(byte id, out MapLayer l)
        {
            return layersById.TryGetValue(id, out l);
        }
    }
}