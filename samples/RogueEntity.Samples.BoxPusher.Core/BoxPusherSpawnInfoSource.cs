using JetBrains.Annotations;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public class BoxPusherSpawnInfoSource : IPlayerSpawnInformationSource
    {
        readonly IPlayerProfileManager<BoxPusherPlayerProfile> profileManager;

        public BoxPusherSpawnInfoSource([NotNull] IPlayerProfileManager<BoxPusherPlayerProfile> profileManager)
        {
            this.profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
        }

        public bool TryCreateInitialLevelRequest(in PlayerTag player, out int level)
        {
            if (!profileManager.TryLoadPlayerData(player.Id, out var profile))
            {
                level = default;
                return false;
            }

            level = profile.CurrentLevel;
            return true;
        }

    }
}
