using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    [MessagePackObject]
    [DataContract]
    public class DenseMapData3D<T> : IMapData3D<T>
    {
        [Key(2)]
        [DataMember(Order = 2)]
        readonly DenseMapData<T>[] layers;

        public DenseMapData3D(int width, int height, int depth)
        {
            this.Width = width;
            this.Height = height;
            this.layers = new DenseMapData<T>[depth];
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

        bool TryGetLayer(int z, out DenseMapData<T> layerData)
        {
            if (z < 0 || z >= layers.Length)
            {
                layerData = default;
                return false;
            }

            layerData = layers[z];
            return true;
        }

        public T this[int x, int y, int z]
        {
            get
            {
                if (TryGetLayer(z, out var map))
                {
                    return map[x, y];
                }

                return default;
            }
            set
            {
                if (TryGetLayer(z, out var map))
                {
                    map[x, y] = value;
                    return;
                }

                layers[z] = new DenseMapData<T>(Width, Height) { [x, y] = value };
            }
        }
    }
}