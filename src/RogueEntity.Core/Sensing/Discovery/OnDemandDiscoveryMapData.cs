using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Discovery
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public class OnDemandDiscoveryMap: IDiscoveryMap
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly int width;
        [Key(1)]
        [DataMember(Order = 0)]
        readonly int height;

        [Key(2)]
        [DataMember(Order = 0)]
        readonly Dictionary<int, DynamicBoolDataView> mapData;
        
        public OnDemandDiscoveryMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            
            this.width = width;
            this.height = height;
            mapData = new Dictionary<int, DynamicBoolDataView>();
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int Width => width;

        [IgnoreMember]
        [IgnoreDataMember]
        public int Height => height;
        
        [SerializationConstructor]
        internal OnDemandDiscoveryMap(int width, int height, Dictionary<int, DynamicBoolDataView> mapData)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            this.width = width;
            this.height = height;
            this.mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }


        public bool TryGetMap(int z, out IReadOnlyView2D<bool> data)
        {
            if (mapData.TryGetValue(z, out var ddata))
            {
                data = ddata;
                return true;
            }

            var ndata = new DynamicBoolDataView(Width, Height);
            mapData[z] = ndata;
            data = ndata;
            return true;
        }
    }
}