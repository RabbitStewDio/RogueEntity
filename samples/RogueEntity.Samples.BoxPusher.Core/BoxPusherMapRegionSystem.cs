using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.MapLoading;
using RogueEntity.Core.Players;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;

namespace RogueEntity.Samples.BoxPusher.Core
{
    /// <summary>
    ///   A dynamic map loader. This class uses player assigned commands to control the map
    ///   loading process
    /// </summary>
    public class BoxPusherMapRegionSystem : BasicMapRegionSystemBase
    {
        readonly IPlayerProfileManager<BoxPusherPlayerProfile> profileManager;

        public BoxPusherMapRegionSystem(BoxPusherMapLevelDataSource mapLoader, 
                                        TimeSpan maximumProcessingTime,
                                        [NotNull] IPlayerProfileManager<BoxPusherPlayerProfile> profileManager): base(mapLoader, maximumProcessingTime)
        {
            this.profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
        }

        protected override Optional<ChangeLevelCommand> CreateInitialLevelRequest<TItemId>(IEntityViewControl<TItemId> v,
                                                                                           TItemId k,
                                                                                           in PlayerObserverTag player)
        {
            if (!profileManager.TryLoadPlayerData(player.Id, out var profile))
            {
                return Optional.Empty();
            }

            var level = profile.CurrentLevel;
            return new ChangeLevelCommand(level);
        }
    }
}
