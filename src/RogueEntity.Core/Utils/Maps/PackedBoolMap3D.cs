using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    [MessagePackObject]
    [DataContract]
    public class PackedBoolMap3D : IMapData3D<bool>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly int width;
        [Key(1)]
        [DataMember(Order = 0)]
        readonly int height;

        [Key(2)]
        [DataMember(Order = 2)]
        readonly PackedBoolMap[] layers;

        public PackedBoolMap3D(int width, int height, int depth)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            this.width = width;
            this.height = height;
            this.layers = new PackedBoolMap[depth];
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int Width => width;

        [IgnoreMember]
        [IgnoreDataMember]
        public int Height => height;

        public int Depth
        {
            get { return layers.Length; }
        }

        public bool TryGetLayer(int z, out PackedBoolMap layerData)
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

        public void Clear()
        {
            foreach (var l in layers)
            {
                l?.Clear();
            }
        }
        
        public void ClearLayer(int z)
        {
            if (TryGetLayer(z, out var map))
            {
                map.Clear();
            }
        }
    }
}