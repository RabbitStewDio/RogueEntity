using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Chunks;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Simple.BoxPusher.ItemTraits;
using System;
using System.Linq;

namespace RogueEntity.Simple.BoxPusher
{
    public class BoxPusherLevelSystem<TActor, TItem>
        where TActor : IEntityKey
        where TItem : IEntityKey
    {
        readonly IGridMapDataContext<TActor> playerLayerMapData;
        readonly MapLayer playerLayer;
        readonly IItemResolver<TActor> playerItemResolver;
        readonly IItemResolver<TItem> spawnPointItemResolver;
        readonly IItemPlacementService<TActor> actorPlacementService;
        readonly Lazy<FullLevelChunkManager> levelManager;

        public BoxPusherLevelSystem(IGridMapContext<TActor> playerMapData,
                                    MapLayer layer,
                                    IItemResolver<TActor> playerItemResolver,
                                    IItemResolver<TItem> spawnPointItemResolver,
                                    IItemPlacementServiceContext<TActor> actorPlacementService,
                                    Lazy<FullLevelChunkManager> levelManager)
        {
            if (!playerMapData.TryGetGridDataFor(layer, out playerLayerMapData))
            {
                throw new ArgumentException($"Require a map layer {layer} that can contain {typeof(TActor)} entities");
            }

            if (!actorPlacementService.TryGetItemPlacementService(layer, out this.actorPlacementService))
            {
                throw new ArgumentException($"Require a configured placement service for map layer {layer} that can contain {typeof(TActor)} entities");
            }

            this.playerLayer = layer;
            this.playerItemResolver = playerItemResolver;
            this.spawnPointItemResolver = spawnPointItemResolver;
            this.levelManager = levelManager;
        }

        public MapChunkLoadingResult InitialPlayerSpawnSystem(TActor player)
        {
            return TryMovePlayerToLevel(player, 0);
        }

        public MapChunkLoadingResult TryMovePlayerToLevel(TActor player, int level)
        {
            playerItemResolver.TryQueryData(player, out Position playerPosition);

            if (!levelManager.Value.CanCreateLevel(level))
            {
                // no progress possible.
                return MapChunkLoadingResult.NoSuchLevel;
            }

            // Maybe the level has not yet been loaded?
            if (!playerLayerMapData.TryGetView(level, out _))
            {
                var result = levelManager.Value.TryLoadMap(level);
                if (result != MapChunkLoadingResult.NoSuchLevel &&
                    !playerPosition.IsInvalid)
                {
                    actorPlacementService.TryRemoveItem(player, playerPosition);
                }

                return result;
            }

            // So the level has been loaded, so lets place the player at one of the spawn points.
            var spawnPointsOnLevel = spawnPointItemResolver.QueryProvider.QueryByTrait<PlayerSpawnLocation, EntityGridPosition>()
                                                           .Where(x => !x.Item3.IsInvalid &&
                                                                       x.Item3.GridZ == level)
                                                           .ToList();

            foreach (var (_, _, pos) in spawnPointsOnLevel)
            {
                if (playerPosition.IsInvalid)
                {
                    if (actorPlacementService.TryPlaceItem(player, Position.From(pos.WithLayer(playerLayer))))
                    {
                        return MapChunkLoadingResult.Success;
                    }
                }
                else
                {
                    if (actorPlacementService.TryMoveItem(player, playerPosition, Position.From(pos.WithLayer(playerLayer))))
                    {
                        return MapChunkLoadingResult.Success;
                    }
                }
            }

            return MapChunkLoadingResult.NoSuchLevel;
        }

        public bool TryFindNextUnsolvedLevel(TActor player, out int level)
        {
            if (!playerItemResolver.TryQueryData(player, out EntityGridPosition pos) ||
                !playerItemResolver.TryQueryData(player, out BoxPusherPlayerProfile record))
            {
                level = default;
                return false;
            }

            level = 0;
            if (!pos.IsInvalid)
            {
                level = pos.GridZ;
            }

            level = FindFirstUnsolvedLevel(level, record);
            if (!levelManager.Value.CanCreateLevel(level))
            {
                return true;
            }

            level = default;
            return false;
        }

        static int FindFirstUnsolvedLevel(int level, BoxPusherPlayerProfile record)
        {
            for (; record.IsComplete(level); level += 1)
            {
                // skipping all completed levels.
            }

            return level;
        }

        public static BoxPusherLevelSystem<TActor, TItem> Create(IServiceResolver r, MapLayer mapLayer)
        {
            return new BoxPusherLevelSystem<TActor, TItem>(r.Resolve<IGridMapContext<TActor>>(), mapLayer,
                                                           r.Resolve<IItemResolver<TActor>>(), r.Resolve<IItemResolver<TItem>>(),
                                                           r.Resolve<IItemPlacementServiceContext<TActor>>(),
                                                           r.ResolveToReference<FullLevelChunkManager>()
            );
        }
    }
}
