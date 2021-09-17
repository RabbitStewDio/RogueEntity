using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    /// <summary>
    ///   A spawn system that expects a map organized in z-layers.
    ///   Each z-position defines a separate region that might be loaded
    ///   or unloaded as needed.
    ///
    ///   This system works in tandem with the FlatMapRegionLoaderService,
    ///   which is suitable for rogue-like or 2D role-playing games of
    ///   the early years.
    /// </summary>
    /// <typeparam name="TActorId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public class FlatLevelPlayerSpawnSystem<TActorId, TItemId>
        where TActorId : IEntityKey
        where TItemId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<FlatLevelPlayerSpawnSystem<TItemId, TActorId>>();

        readonly IItemResolver<TActorId> actorResolver;
        readonly IPlayerSpawnInformationSource spawnInfoSource;
        readonly IItemPlacementService<TActorId> placementService;
        readonly IItemPlacementLocationService<TActorId> spatialQuery;
        readonly Dictionary<int, List<(Position pos, TItemId entity)>> spawnPointsPerLevel;
        readonly Optional<IEntityRandomGeneratorSource> randomSource;
        readonly Lazy<IMapAvailabilityService> mapLoaderService;
        readonly List<(Position pos, TItemId entity)> filterBuffer;

        public FlatLevelPlayerSpawnSystem(IItemPlacementService<TActorId> placementService,
                                          IItemPlacementLocationService<TActorId> spatialQuery,
                                          IItemResolver<TActorId> actorResolver,
                                          IPlayerSpawnInformationSource spawnInfoSource,
                                          Lazy<IMapAvailabilityService> mapLoaderService,
                                          Optional<IEntityRandomGeneratorSource> randomSource = default)
        {
            this.mapLoaderService = mapLoaderService;
            this.spatialQuery = spatialQuery;
            this.actorResolver = actorResolver;
            this.spawnInfoSource = spawnInfoSource;
            this.placementService = placementService;
            this.randomSource = randomSource;
            this.spawnPointsPerLevel = new Dictionary<int, List<(Position, TItemId)>>();
            this.filterBuffer = new List<(Position pos, TItemId entity)>();
        }

        /// <summary>
        ///   Invoked when a new player has spawned. This uses some built-in default
        ///   to place the player in the first level as determined by the map loader's new-player-spawn-level property. 
        /// </summary>
        public void RequestLoadLevelFromNewPlayer(IEntityViewControl<TActorId> v,
                                                  TActorId k,
                                                  in PlayerTag player,
                                                  in NewPlayerTag newPlayerTag)
        {
            if (!spawnInfoSource.TryCreateInitialLevelRequest(player, out var lvl))
            {
                Logger.Error("Unable to create initial level request for player {PlayerId}", player.Id);
                return;
            }
            
            var cmd = new ChangeLevelCommand(lvl);
            v.AssignComponent(k, cmd);
            v.RemoveComponent<NewPlayerTag>(k);
        }

        public void StartCollectSpawnLocations()
        {
            foreach (var d in spawnPointsPerLevel)
            {
                d.Value.Clear();
            }
        }

        public void CollectSpawnLocations<TPosition>(IEntityViewControl<TItemId> v,
                                                     TItemId k,
                                                     in TPosition pos,
                                                     in PlayerSpawnLocation spawnLocation)
            where TPosition : IPosition<TPosition>
        {
            if (pos.IsInvalid)
            {
                return;
            }

            if (!spawnPointsPerLevel.TryGetValue(pos.GridZ, out var positions))
            {
                positions = new List<(Position, TItemId)>();
                spawnPointsPerLevel[pos.GridZ] = positions;
            }

            var spawnPos = Position.Of(MapLayer.Indeterminate, pos.X, pos.Y, pos.Z);
            if (IsValidSpawnLocation(k, spawnPos, spawnLocation))
            {
                positions.Add((spawnPos, k));
            }
        }

        /// <summary>
        ///   Checks whether the given location is valid for the player. The default implementation
        ///   makes a simple placement check with a radius of 1.
        /// </summary>
        protected virtual bool IsValidSpawnLocation(TItemId k,
                                                    in Position pos,
                                                    in PlayerSpawnLocation spawnLocation)
        {
            return true;
        }

        /// <summary>
        ///   Spawn a player somewhere in a given level using existing spawn points as target locations.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="player"></param>
        /// <param name="cmd"></param>
        public void SpawnPlayer(IEntityViewControl<TActorId> v,
                                TActorId k,
                                in PlayerTag player,
                                in ChangeLevelCommand cmd)
        {
            var level = cmd.Level;
            if (!mapLoaderService.Value.IsLevelReadyForSpawning(level))
            {
                return;
            }

            // find spawn points in current level
            if (!spawnPointsPerLevel.TryGetValue(level, out var levelData) ||
                levelData.Count == 0)
            {
                Logger.Warning("After loading map data for z-level {Level} no spawn points were detected", level);
                // Error: Did not actually find a spawn location.
                return;
            }

            if (!actorResolver.TryQueryData(k, out MapLayerPreference mapLayerPref))
            {
                Logger.Warning("Unable to determine map layer preference for player actor");
                return;
            }

            var l = FilterByAvailableSpace(levelData, mapLayerPref);
            if (randomSource.TryGetValue(out var value))
            {
                if (PlaceRandomly(l, value, k, mapLayerPref.PreferredLayer))
                {
                    v.RemoveComponent<ChangeLevelCommand>(k);
                    return;
                }
            }
            else if (PlaceLinearly(l, k, mapLayerPref.PreferredLayer))
            {
                v.RemoveComponent<ChangeLevelCommand>(k);
                return;
            }

            v.RemoveComponent<ChangeLevelCommand>(k);
            Logger.Warning("Unable to find any placement for player actor");
        }

        List<(Position pos, TItemId entity)> FilterByAvailableSpace(List<(Position pos, TItemId entity)> raw,
                                                                    in MapLayerPreference mapLayerPref)
        {
            filterBuffer.Clear();
            foreach (var valueTuple in raw)
            {
                var pos = valueTuple.pos;
                if (spatialQuery.TryFindEmptySpace(pos.WithLayer(mapLayerPref.PreferredLayer), out _, 1))
                {
                    filterBuffer.Add(valueTuple);
                }
            }

            return filterBuffer;
        }

        bool PlaceLinearly(List<(Position pos, TItemId entity)> levelData,
                           TActorId k,
                           MapLayer preferredLayer)
        {
            while (levelData.Count > 0)
            {
                var index = levelData.Count - 1;
                var spawnPosition = levelData[index].pos;
                spawnPosition = spawnPosition.WithLayer(preferredLayer);
                if (placementService.TryPlaceItem(k, spawnPosition))
                {
                    Logger.Warning("Spawned player actor at position {SpawnPosition}", spawnPosition);
                    levelData.RemoveAt(index);
                    return true;
                }

                Logger.Warning("Unable to place actor at position {SpawnLocation}", spawnPosition);
            }

            return false;
        }

        bool PlaceRandomly(List<(Position pos, TItemId entity)> levelData,
                           IEntityRandomGeneratorSource value,
                           TActorId k,
                           MapLayer preferredLayer)
        {
            while (levelData.Count > 0)
            {
                var x = value.FromEntity(k, 50);
                var rnd = x.Next(0, levelData.Count);
                var spawnPosition = levelData[rnd].pos;
                spawnPosition = spawnPosition.WithLayer(preferredLayer);
                if (placementService.TryPlaceItem(k, spawnPosition))
                {
                    Logger.Warning("Spawned player actor at random position {SpawnPosition}", spawnPosition);
                    levelData.RemoveAt(rnd);
                    return true;
                }

                Logger.Warning("Unable to place actor at position {SpawnLocation}", spawnPosition);
            }

            return false;
        }

        /// <summary>
        ///   Transports a player to a predefined location in the new level.
        ///   Use this for teleportation devices that transport players to a known point. 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="player"></param>
        /// <param name="cmd"></param>
        public void PlacePlayerAfterLevelChange(IEntityViewControl<TActorId> v,
                                                TActorId k,
                                                in PlayerTag player,
                                                in ChangeLevelPositionCommand cmd)
        {
            var level = cmd.Position;
            if (cmd.Position.IsInvalid)
            {
                Logger.Warning("Unable to place actor at position marked as invalid");
                v.RemoveComponent<ChangeLevelPositionCommand>(k);
                return;
            }

            if (!mapLoaderService.Value.IsLevelPositionAvailable(level))
            {
                // Wait until the level has fully loaded. This is controlled elsewhere.
                return;
            }

            if (!placementService.TryPlaceItem(k, level))
            {
                Logger.Warning("Unable to place actor at position {SpawnLocation}", level);
                return;
            }

            v.RemoveComponent<ChangeLevelPositionCommand>(k);
        }
    }
}
