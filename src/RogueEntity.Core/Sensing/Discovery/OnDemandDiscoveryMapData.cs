using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Discovery
{
    public interface IDiscoveryMapData
    {
        int Width { get; }
        int Height { get; }
        
        bool TryGetMap(int z, out IMapData<bool> data);
    }
    
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public class OnDemandDiscoveryMapData: IDiscoveryMapData, IEquatable<OnDemandDiscoveryMapData>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly int width;
        [Key(1)]
        [DataMember(Order = 0)]
        readonly int height;

        [Key(2)]
        [DataMember(Order = 0)]
        readonly Dictionary<int, PackedBoolMap> mapData;
        
        public OnDemandDiscoveryMapData(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            
            this.width = width;
            this.height = height;
            mapData = new Dictionary<int, PackedBoolMap>();
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int Width => width;

        [IgnoreMember]
        [IgnoreDataMember]
        public int Height => height;
        
        [SerializationConstructor]
        internal OnDemandDiscoveryMapData(int width, int height, Dictionary<int, PackedBoolMap> mapData)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            this.width = width;
            this.height = height;
            this.mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }

        public bool TryGetMap(int z, out IMapData<bool> data)
        {
            if (mapData.TryGetValue(z, out var ddata))
            {
                data = ddata;
                return true;
            }

            var ndata = new PackedBoolMap(Width, Height);
            mapData[z] = ndata;
            data = ndata;
            return true;
        }

        public bool Equals(OnDemandDiscoveryMapData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return CoreExtensions.EqualsDictionary(mapData, other.mapData) && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((OnDemandDiscoveryMapData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (mapData != null ? mapData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }

        public static bool operator ==(OnDemandDiscoveryMapData left, OnDemandDiscoveryMapData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OnDemandDiscoveryMapData left, OnDemandDiscoveryMapData right)
        {
            return !Equals(left, right);
        }
    }
}