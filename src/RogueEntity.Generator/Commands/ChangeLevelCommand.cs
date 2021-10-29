using EnTTSharp.Entities.Attributes;
using MessagePack;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Generator.Commands
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [Serializable]
    [DataContract]
    public readonly struct ChangeLevelCommand
    {
        [DataMember]
        [Key(0)]
        public readonly int Level;

        public ChangeLevelCommand(int level)
        {
            Level = level;
        }
    }
}
