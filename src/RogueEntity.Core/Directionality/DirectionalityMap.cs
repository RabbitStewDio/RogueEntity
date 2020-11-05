using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Directionality
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public class DirectionalityMap<TStorageQualifier>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly int offsetX;
        [Key(1)]
        [DataMember(Order = 1)]
        readonly int offsetY;
        [Key(2)]
        [DataMember(Order = 2)]
        readonly int tileSizeX;
        [Key(3)]
        [DataMember(Order = 3)]
        readonly int tileSizeY;
        [Key(4)]
        [DataMember(Order = 4)]
        readonly Dictionary<int, DynamicDataView<DirectionalityInformation>> backend;

        [SerializationConstructor]
        internal DirectionalityMap(int offsetX, int offsetY, int tileSizeX, int tileSizeY, Dictionary<int, DynamicDataView<DirectionalityInformation>> backend)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.backend = backend;
        }

        public DirectionalityMap(int offsetX, int offsetY, int tileSizeX, int tileSizeY) 
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.backend = new Dictionary<int, DynamicDataView<DirectionalityInformation>>();
        }

        public bool TryGetMap(int z, out IReadOnlyDynamicDataView2D<DirectionalityInformation> data)
        {
            if (backend.TryGetValue(z, out var dataRaw))
            {
                data = dataRaw;
                return true;
            }

            data = default;
            return false;
        }

        public IDynamicDataView2D<DirectionalityInformation> GetWritableMap(int z)
        {
            if (!backend.TryGetValue(z, out var dataRaw))
            {
                dataRaw = new DynamicDataView<DirectionalityInformation>(offsetX, offsetY, tileSizeX, tileSizeY);
                backend[z] = dataRaw;
            }
            
            return dataRaw;
        }
    }
}