using MessagePack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RogueEntity.Simple.BoxPusher.ItemTraits
{
    [MessagePackObject]
    [DataContract]
    public class BoxPusherPlayerProfile
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public string PlayerName { get; set; }
        
        [Key(1)]
        [DataMember(Order = 1)]
        readonly Dictionary<int, LevelStats> levelStats;

        public BoxPusherPlayerProfile()
        {
            this.levelStats = new Dictionary<int, LevelStats>();
            this.PlayerName = "<undefined>";
        }

        public BoxPusherPlayerProfile(string playerName): this()
        {
            PlayerName = playerName ?? "<undefined>";
        }

        [SerializationConstructor]
        internal BoxPusherPlayerProfile(string playerName, Dictionary<int, LevelStats> levelStats)
        {
            this.PlayerName = playerName ?? "<undefined>";
            this.levelStats = levelStats;
        }

        public void RecordLevelComplete(int level)
        {
            if (!this.levelStats.TryGetValue(level, out var stats))
            {
                stats = new LevelStats();
            }

            this.levelStats[level] = stats.Solved();
        }
        
        public void RecordLevelProgress(int level)
        {
            if (!this.levelStats.TryGetValue(level, out var stats))
            {
                stats = new LevelStats();
            }

            this.levelStats[level] = stats.InProgress();
        }


        public bool IsComplete(int level)
        {
            if (!this.levelStats.TryGetValue(level, out var stats))
            {
                return false;
            }

            return stats.ClearedNow;
        }
    }
}
