using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using Serilog;
using System;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class FlatLevelPlayerSpawnRequestHandlerSystem<TActorId>
        where TActorId : struct, IEntityKey
    {
        static readonly ILogger logger = SLog.ForContext<FlatLevelPlayerSpawnRequestHandlerSystem<TActorId>>();
        readonly IFlatLevelPlayerSpawnInformationSource spawnInfoSource;

        public FlatLevelPlayerSpawnRequestHandlerSystem(IFlatLevelPlayerSpawnInformationSource spawnInfoSource)
        {
            this.spawnInfoSource = spawnInfoSource ?? throw new ArgumentNullException(nameof(spawnInfoSource));
        }

        /// <summary>
        ///   Invoked when a new player has spawned. This uses some built-in default
        ///   to place the player in the first level as determined by the map loader's new-player-spawn-level property. 
        /// </summary>
        public void RequestLoadLevelFromNewPlayer(IEntityViewControl<TActorId> v,
                                                  TActorId k,
                                                  in PlayerTag player,
                                                  in NewPlayerSpawnRequest newPlayerSpawnRequest)
        {
            if (!spawnInfoSource.TryCreateInitialLevelRequest(player, out var lvl))
            {
                logger.Error("Unable to create initial level request for player {PlayerId}", player.Id);
                return;
            }

            var cmd = new ChangeLevelRequest(lvl);
            v.AssignComponent(k, cmd);
            v.RemoveComponent<NewPlayerSpawnRequest>(k);
        }
    }
}
