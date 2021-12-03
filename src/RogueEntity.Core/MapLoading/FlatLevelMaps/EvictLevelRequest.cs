using EnTTSharp.Entities.Attributes;
using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    /// <summary>
    ///   A notification/request object to signal a manual or forced unloading
    ///   of a map region.
    /// </summary>
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct EvictLevelRequest
    {
        [Key(0)]
        [DataMember(Order=0)]
        public readonly int Level;

        [SerializationConstructor]
        public EvictLevelRequest(int level)
        {
            Level = level;
        }
    }
}
