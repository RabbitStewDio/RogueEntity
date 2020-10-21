﻿using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    [MessagePackObject]
    [DataContract]
    public class ConstantMap<T>: IReadOnlyMapData<T>, IReadOnlyMapData3D<T>, IReadOnlyView2D<T>
    {
        [Key(3)]
        [DataMember(Order = 3)]
        readonly T value;
        [Key(0)]
        [DataMember(Order = 0)]
        public int Width { get; }
        [Key(1)]
        [DataMember(Order = 1)]
        public int Height { get; }
        [Key(2)]
        [DataMember(Order = 2)]
        public int Depth { get; }

        public ConstantMap(int width, int height, T value)
        {
            this.value = value;
            Width = width;
            Height = height;
        }

        public T this[int x, int y] => value;

        public T this[int x, int y, int z] => value;
    }
}