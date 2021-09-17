using System;
using System.Runtime.Serialization;
using MessagePack;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.MapLayers
{
    [Serializable]
    [DataContract]
    [MessagePackObject]
    [MessagePackFormatter(typeof(MapLayerMessagePackFormatter))]
    public readonly struct MapLayer : IEquatable<MapLayer>
    {
        public static MapLayer Indeterminate => new MapLayer(0, "Indeterminate");

        [SerializationConstructor]
        public MapLayer(byte layerId, string name)
        {
            this.LayerId = layerId;
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [Key(1)]
        public string Name { get; }

        [Key(0)]
        public byte LayerId { get; }

        public override string ToString()
        {
            return $"[{LayerId}]{Name}";
        }

        public bool Equals(MapLayer other)
        {
            return LayerId == other.LayerId && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is MapLayer other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (LayerId * 397) ^ (Name.GetHashCode());
            }
        }

        public static bool operator ==(MapLayer left, MapLayer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapLayer left, MapLayer right)
        {
            return !left.Equals(right);
        }

        sealed class LayerIdRelationalComparer : IComparer<MapLayer>
        {
            public int Compare(MapLayer x, MapLayer y)
            {
                return x.LayerId.CompareTo(y.LayerId);
            }
        }

        public static IComparer<MapLayer> LayerIdComparer { get; } = new LayerIdRelationalComparer();
    }
}