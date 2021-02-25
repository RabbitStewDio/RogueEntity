using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.MapLoading
{
    public class BasicPlayerSpawnSystem<TActorId, TItemId>
        where TActorId : IEntityKey
        where TItemId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<BasicPlayerSpawnSystem<TItemId, TActorId>>();

        readonly IItemResolver<TActorId> actorResolver;
        readonly IMapRegionLoaderService<int> mapLoaderService;
        readonly IItemPlacementService<TActorId> placementService;
        readonly IItemPlacementLocationService<TActorId> spatialQuery;
        readonly Dictionary<int, List<(Position pos, TItemId entity)>> spawnPointsPerLevel;
        readonly Optional<IEntityRandomGeneratorSource> randomSource;

        public BasicPlayerSpawnSystem(IMapRegionLoaderService<int> mapLoaderService,
                                      IItemPlacementService<TActorId> placementService,
                                      IItemPlacementLocationService<TActorId> spatialQuery,
                                      IItemResolver<TActorId> actorResolver,
                                      Optional<IEntityRandomGeneratorSource> randomSource = default)
        {
            this.mapLoaderService = mapLoaderService;
            this.spatialQuery = spatialQuery;
            this.actorResolver = actorResolver;
            this.placementService = placementService;
            this.randomSource = randomSource;
            this.spawnPointsPerLevel = new Dictionary<int, List<(Position, TItemId)>>();
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

        public void SpawnPlayer(IEntityViewControl<TActorId> v,
                                TActorId k,
                                in PlayerObserverTag player,
                                in ChangeLevelCommand cmd)
        {
            var level = cmd.Level;
            if (!mapLoaderService.IsLoaded(level))
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

            // todo: Should really use a reusable buffer list for this.
            var l = levelData.Where(x =>
                             {
                                 var (pos, _) = x;
                                 return spatialQuery.TryFindEmptySpace(pos.WithLayer(mapLayerPref.PreferredLayer), out _, 1);
                             })
                             .ToList();
            
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

        public void PlacePlayerAfterLevelChange(IEntityViewControl<TActorId> v,
                                                TActorId k,
                                                in PlayerObserverTag player,
                                                in ChangeLevelPositionCommand cmd)
        {
            var level = cmd.Position;
            if (cmd.Position.IsInvalid)
            {
                Logger.Warning("Unable to place actor at position marked as invalid");
                v.RemoveComponent<ChangeLevelPositionCommand>(k);
                return;
            }

            if (!mapLoaderService.IsLevelPositionAvailable(level))
            {
                return;
            }

            if (!placementService.TryPlaceItem(k, level))
            {
                Logger.Warning("Unable to place actor at position {SpawnLocation}", level);
                return;
            }

            v.RemoveComponent<ChangeLevelCommand>(k);
        }
    }
}
