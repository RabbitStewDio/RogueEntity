using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Discovery
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class DiscoveryMapData : IDiscoveryMap
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly int tileWidth;

        [Key(1)]
        [DataMember(Order = 0)]
        readonly int tileHeight;

        [Key(2)]
        [DataMember(Order = 2)]
        readonly int offsetX;

        [Key(3)]
        [DataMember(Order = 3)]
        readonly int offsetY;

        [Key(4)]
        [DataMember(Order = 4)]
        readonly Dictionary<int, DynamicBoolDataView> mapData;

        public DiscoveryMapData(int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            if (tileWidth <= 0) throw new ArgumentOutOfRangeException(nameof(tileWidth));
            if (tileHeight <= 0) throw new ArgumentOutOfRangeException(nameof(tileHeight));

            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            mapData = new Dictionary<int, DynamicBoolDataView>();
        }

        [SerializationConstructor]
        internal DiscoveryMapData(int tileWidth, int tileHeight, int offsetX, int offsetY, [NotNull] Dictionary<int, DynamicBoolDataView> mapData)
        {
            if (tileWidth <= 0) throw new ArgumentOutOfRangeException(nameof(tileWidth));
            if (tileHeight <= 0) throw new ArgumentOutOfRangeException(nameof(tileHeight));
            
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int OffsetX => offsetX;

        [IgnoreMember]
        [IgnoreDataMember]
        public int OffsetY => offsetY;

        [IgnoreMember]
        [IgnoreDataMember]
        public int TileWidth => tileWidth;

        [IgnoreMember]
        [IgnoreDataMember]
        public int TileHeight => tileHeight;

        public bool TryGetWritableMap(int z, out IDynamicDataView2D<bool> data)
        {
            if (mapData.TryGetValue(z, out var dataForLevel))
            {
                data = dataForLevel;
                return true;
            }

            var d = new DynamicBoolDataView(OffsetX, OffsetY, TileWidth, TileHeight);
            mapData[z] = d;
            data = d;
            return true;
        }
        
        public bool TryGetMap(int z, out IReadOnlyView2D<bool> data)
        {
            if (mapData.TryGetValue(z, out var dataForLevel))
            {
                data = dataForLevel;
                return true;
            }

            data = default;
            return false;
        }
    }
}