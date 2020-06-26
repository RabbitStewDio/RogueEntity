using System;

namespace RogueEntity.Core.Utils.Maps
{
    public readonly struct MapLayer : IEquatable<MapLayer>
    {
        public MapLayer(byte layerId, string name)
        {
            this.LayerId = layerId;
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

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