using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
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
    }
}