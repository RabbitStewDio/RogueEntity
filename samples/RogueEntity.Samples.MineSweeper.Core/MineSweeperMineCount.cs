using EnTTSharp.Entities.Attributes;
using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Simple.MineSweeper
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct MineSweeperMineCount
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly int Count;

        public MineSweeperMineCount(int count)
        {
            Count = count;
        }
    }
}
