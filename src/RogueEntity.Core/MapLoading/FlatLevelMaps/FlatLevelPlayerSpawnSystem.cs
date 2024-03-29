using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.MapLoading.PlayerSpawning;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Positioning.SpatialQueries;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
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
        where TActorId : struct, IEntityKey
        where TItemId : struct, IEntityKey
    {
        static readonly ILogger logger = SLog.ForContext<FlatLevelPlayerSpawnSystem<TItemId, TActorId>>();

        readonly ISpatialQueryLookup spatialQuerySource;
        readonly IItemResolver<TActorId> actorResolver;
        readonly IItemPlacementService<TActorId> placementService;
        readonly IItemPlacementLocationService<TActorId> freePlacementQuery;
        readonly Optional<IEntityRandomGeneratorSource> randomSource;
        readonly IMapRegionTrackerService<int> mapLoaderService;
        readonly IMapRegionMetaDataService<int> mapMetadataService;

        readonly List<(Position pos, TItemId entity)> filterBuffer;
        readonly BufferList<SpatialQueryResult<TItemId, PlayerSpawnLocation>> buffer;

        public FlatLevelPlayerSpawnSystem(IItemPlacementService<TActorId> placementService,
                                          IItemPlacementLocationService<TActorId> spatialQuery,
                                          IItemResolver<TActorId> actorResolver,
                                          IMapRegionTrackerService<int> mapLoaderService,
                                          IMapRegionMetaDataService<int> mapMetadataService,
                                          ISpatialQueryLookup spatialQuerySource,
                                          Optional<IEntityRandomGeneratorSource> randomSource = default)
        {
            this.mapLoaderService = mapLoaderService ?? throw new ArgumentNullException(nameof(mapLoaderService));
            this.spatialQuerySource = spatialQuerySource ?? throw new ArgumentNullException(nameof(spatialQuerySource));
            this.mapMetadataService = mapMetadataService ?? throw new ArgumentNullException(nameof(mapMetadataService));
            this.freePlacementQuery = spatialQuery ?? throw new ArgumentNullException(nameof(spatialQuery));
            this.actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
            this.placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));
            this.randomSource = randomSource;
            this.filterBuffer = new List<(Position pos, TItemId entity)>();
            this.buffer = new BufferList<SpatialQueryResult<TItemId, PlayerSpawnLocation>>();
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
                                in ChangeLevelRequest cmd)
        {
            var level = cmd.Level;
            if (!mapLoaderService.IsRegionLoaded(level))
            {
                return;
            }

            if (!spatialQuerySource.TryGetQuery<TItemId, PlayerSpawnLocation>(out var query))
            {
                throw new ArgumentException("No query source for entity type " + typeof(TItemId).Name);
            }

            if (!mapMetadataService.TryGetRegionBounds(level, out var levelBounds))
            {
                logger.Warning("There is no map data for z-level {Level}", level);
                return;
            }

            if (!actorResolver.TryQueryData<BodySize>(k, out var bs))
            {
                bs = BodySize.OneByOne;
            }
            
            logger.Debug("Attempting to spawn players for z-level {Level}", level);
            query.QueryBox(levelBounds, buffer);
            if (buffer.Count == 0)
            {
                logger.Warning("After loading map data for z-level {Level} no spawn points were detected", level);
                return;
            }

            if (!actorResolver.TryQueryData(k, out MapLayerPreference mapLayerPref))
            {
                logger.Warning("Unable to determine map layer preference for player actor");
                return;
            }

            var l = FilterByAvailableSpace(buffer, bs, mapLayerPref);
            if (l.Count == 0)
            {
                logger.Warning("Unable to find any unoccupied spawn point for player {PlayerEntity}", k);
                return;
            }
            
            if (randomSource.TryGetValue(out var value))
            {
                if (PlaceRandomly(l, value, k, mapLayerPref.PreferredLayer))
                {
                    logger.Debug("Successfully spawned player {PlayerEntity} at random position", k);
                    v.RemoveComponent<ChangeLevelRequest>(k);
                    return;
                }
            }
            else if (PlaceLinearly(l, k, mapLayerPref.PreferredLayer))
            {
                logger.Debug("Successfully spawned player {PlayerEntity} at linear position", k);
                v.RemoveComponent<ChangeLevelRequest>(k);
                return;
            }

            logger.Warning("Unable to find any placement for player actor");
            v.RemoveComponent<ChangeLevelRequest>(k);
        }

        List<(Position pos, TItemId entity)> FilterByAvailableSpace(BufferList<SpatialQueryResult<TItemId, PlayerSpawnLocation>> raw,
                                                                    BodySize bodySize,
                                                                    in MapLayerPreference mapLayerPref)
        {
            filterBuffer.Clear();
            foreach (var valueTuple in raw)
            {
                var pos = valueTuple.Position;
                if (freePlacementQuery.TryFindEmptySpace(pos.WithLayer(mapLayerPref.PreferredLayer), 
                                                         bodySize,  
                                                         out var placementPosition, 1))
                {
                    logger.Debug("Selected {Position} as possible spawn location for Player", placementPosition);
                    filterBuffer.Add((placementPosition, valueTuple.EntityId));
                }
                else
                {
                    logger.Debug("Skipped {Position} as possible spawn location for Player; this position is occupied", valueTuple.Position);
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
                    logger.Warning("Spawned player actor at position {SpawnPosition}", spawnPosition);
                    levelData.RemoveAt(index);
                    return true;
                }

                logger.Warning("Unable to place actor at position {SpawnLocation}", spawnPosition);
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
                    logger.Warning("Spawned player actor at random position {SpawnPosition}", spawnPosition);
                    levelData.RemoveAt(rnd);
                    return true;
                }

                logger.Warning("Unable to place actor at position {SpawnLocation}", spawnPosition);
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
                                                in ChangeLevelPositionRequest cmd)
        {
            var level = cmd.Position;
            if (cmd.Position.IsInvalid)
            {
                logger.Warning("Unable to place actor at position marked as invalid");
                v.RemoveComponent<ChangeLevelPositionRequest>(k);
                return;
            }

            if (!mapLoaderService.IsRegionLoaded(level.GridZ))
            {
                return;
            }

            if (!placementService.TryPlaceItem(k, level))
            {
                logger.Warning("Unable to place actor at position {SpawnLocation}", level);
                return;
            }

            v.RemoveComponent<ChangeLevelPositionRequest>(k);
        }
    }
}