using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RogueEntity.Samples.BoxPusher.Core.ItemTraits
{
    [EntityComponent]
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
        
        public int CurrentLevel { get; set; } 

        public BoxPusherPlayerProfile()
        {
            this.levelStats = new Dictionary<int, LevelStats>();
            this.PlayerName = "<undefined>";
            this.CurrentLevel = 0;
        }

        public int MaxLevelComplete => levelStats.Where(x => x.Value.ClearedOnce).Select(x => x.Key).MaybeMax().GetOrElse(0);

        public BoxPusherPlayerProfile(string playerName): this()
        {
            PlayerName = playerName ?? "<undefined>";
        }

        [SerializationConstructor]
        internal BoxPusherPlayerProfile(string playerName, Dictionary<int, LevelStats> levelStats, int currentLevel)
        {
            this.CurrentLevel = currentLevel;
            this.PlayerName = playerName ?? "<undefined>";
            this.levelStats = levelStats;
        }

        public BoxPusherPlayerProfile RecordLevelComplete(int level)
        {
            if (!this.levelStats.TryGetValue(level, out var stats))
            {
                stats = new LevelStats();
            }

            this.levelStats[level] = stats.Solved();
            return this;
        }
        
        public BoxPusherPlayerProfile RecordLevelProgress(int level)
        {
            if (!this.levelStats.TryGetValue(level, out var stats))
            {
                stats = new LevelStats();
            }

            this.levelStats[level] = stats.InProgress();
            return this;
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
