using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Maps
{
    public class SensoryCacheMap3D<TGameContext> : IReadOnlyMapData3D<SenseProperties>
    {
        [Key(2)]
        [DataMember(Order = 2)]
        readonly SensoryCacheMap<TGameContext>[] layers;

        public SensoryCacheMap3D(int width, int height, int depth)
        {
            this.Width = width;
            this.Height = height;
            this.layers = new SensoryCacheMap<TGameContext>[depth];
        }

        [Key(0)]
        [DataMember(Order = 0)]
        public int Width { get; }

        [Key(1)]
        [DataMember(Order = 1)]
        public int Height { get; }

        public int Depth
        {
            get { return layers.Length; }
        }

        bool TryGetLayer(int z, out SensoryCacheMap<TGameContext> layerData)
        {
            if (z < 0 || z >= layers.Length)
            {
                layerData = default;
                return false;
            }

            layerData = layers[z];
            return layerData != null;
        }

        public SenseProperties this[int x, int y, int z]
        {
            get
            {
                if (TryGetLayer(z, out var map))
                {
                    return map[x, y];
                }

                return default;
            }
        }

        public void MarkDirty(MapLayer l, EntityGridPosition pos)
        {
            if (TryGetLayer(pos.GridZ, out var d))
            {
                d?.MarkDirty(l, pos);
            }
        }

        public void ResetDirtyFlags()
        {
            foreach (var d in layers)
            {
                d?.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            foreach (var d in layers)
            {
                d?.Process(c);
            }
        }
    }
}