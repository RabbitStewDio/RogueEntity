using EnTTSharp.Entities.Attributes;
using MessagePack;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public readonly struct GoalMarker<TDiscriminator>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly float Strength;

        public GoalMarker(float strength)
        {
            Strength = strength;
        }
    }
}