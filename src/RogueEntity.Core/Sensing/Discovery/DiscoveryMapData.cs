using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;

namespace RogueEntity.Core.Sensing.Discovery
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class DiscoveryMapData : DynamicBoolDataView3D, IDiscoveryMap
    {
        public DiscoveryMapData()
        { }

        public DiscoveryMapData(DynamicDataViewConfiguration config) : base(config)
        { }

        public DiscoveryMapData(int tileSizeX, int tileSizeY) : base(tileSizeX, tileSizeY)
        { }

        public DiscoveryMapData(int offsetX, int offsetY, int tileSizeX, int tileSizeY) : base(offsetX, offsetY, tileSizeX, tileSizeY)
        { }

        [SerializationConstructor]
        public DiscoveryMapData(int tileSizeX,
                                int tileSizeY,
                                int offsetX,
                                int offsetY,
                                Dictionary<int, DynamicBoolDataView2D> index) : base(tileSizeX, tileSizeY, offsetX, offsetY, index)
        { }
    }
}
