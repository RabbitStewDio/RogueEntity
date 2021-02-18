using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Samples.BoxPusher.Core.ItemTraits
{
    [MessagePackObject]
    [DataContract]
    public readonly struct LevelStats
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly int Steps;

        [Key(1)]
        [DataMember(Order = 1)]
        public readonly bool ClearedOnce;

        [Key(2)]
        [DataMember(Order = 2)]
        public readonly bool ClearedNow;

        public LevelStats(int steps, bool clearedOnce, bool clearedNow)
        {
            Steps = steps;
            ClearedOnce = clearedOnce;
            ClearedNow = clearedNow;
        }

        public LevelStats InProgress()
        {
            return new LevelStats(this.Steps + 1, ClearedOnce, false);
        }

        public LevelStats Solved()
        {
            return new LevelStats(this.Steps + 1, true, true);
        }
    }
}
