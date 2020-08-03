using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    [MessagePackObject]
    [DataContract]
    public class PackedBoolMap3D : IMapData3D<bool>
    {
        [Key(2)]
        [DataMember(Order = 2)]
        readonly PackedBoolMap[] layers;

        public PackedBoolMap3D(int width, int height, int depth)
        {
            this.Width = width;
            this.Height = height;
            this.layers = new PackedBoolMap[depth];
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

        bool TryGetLayer(int z, out PackedBoolMap layerData)
        {
            if (z < 0 || z >= layers.Length)
            {
                layerData = default;
                return false;
            }

            layerData = layers[z];
            return true;
        }

        public bool this[int x, int y, int z]
        {
            get
            {
                return TryGetLayer(z, out var map) && map[x, y];
            }
            set
            {
                if (TryGetLayer(z, out var map))
                {
                    map[x, y] = value;
                    return;
                }

                if (value)
                {
                    layers[z] = new PackedBoolMap(Width, Height) {[x, y] = true};
                }
                
            }
        }
    }
}