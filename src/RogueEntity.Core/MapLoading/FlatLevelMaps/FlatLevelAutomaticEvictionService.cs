using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    /// <summary>
    ///    A service system that automatically evicts levels that contain no player entities.
    /// </summary>
    public class FlatLevelAutomaticEvictionService<TActorId>
    {
        readonly Lazy<ITimeSource> timer;
        readonly IPlayerLookup<TActorId> playerLookupService;
        readonly IItemResolver<TActorId> playerItemResolver;
        readonly IMapRegionTrackerService<int> regionTracker;
        readonly MapRegionModuleConfiguration config;
        readonly BufferList<PlayerTag> playerBuffer;
        readonly BufferList<IMapRegionProcessingRequestHandle<int>> regionBuffer;
        readonly Dictionary<int, TimeSpan> populatedLevels;
        readonly List<int> regionKeyBuffer;

        public FlatLevelAutomaticEvictionService(IPlayerLookup<TActorId> playerLookupService, 
                                                 IItemResolver<TActorId> playerItemResolver, 
                                                 IMapRegionTrackerService<int> regionTracker,
                                                 MapRegionModuleConfiguration config,
                                                 Lazy<ITimeSource> timer)
        {
            this.playerLookupService = playerLookupService;
            this.playerItemResolver = playerItemResolver;
            this.regionTracker = regionTracker;
            this.config = config;
            this.timer = timer;
            this.playerBuffer = new BufferList<PlayerTag>();
            this.regionBuffer = new BufferList<IMapRegionProcessingRequestHandle<int>>();
            this.populatedLevels = new Dictionary<int, TimeSpan>();
            this.regionKeyBuffer = new List<int>();
        }

        public void ProcessLevels()
        {
            CollectPopulatedLevelData();
            RequestEviction();
            PurgeStaleRegionRecords();
        }

        void CollectPopulatedLevelData()
        {
            var currentTime = timer.Value.CurrentTime;
            foreach (var playerTag in playerLookupService.QueryPlayers(playerBuffer))
            {
                if (!playerLookupService.TryQueryPlayer(playerTag, out var playerEntity))
                {
                    continue;
                }

                if (playerItemResolver.TryQueryData(playerEntity, out Position pos) && !pos.IsInvalid)
                {
                    populatedLevels[pos.GridZ] = currentTime;
                }
            }
        }

        void RequestEviction()
        {
            var mapEvictionTimer = config.MapEvictionTimer;
            var currentTime = timer.Value.CurrentTime;

            foreach (var region in regionTracker.QueryActiveRequests(MapRegionStatus.Loaded, regionBuffer))
            {
                if (populatedLevels.TryGetValue(region.RegionKey, out var lastSeenTime))
                {
                    if (currentTime - lastSeenTime < mapEvictionTimer)
                    {
                        continue;
                    }
                }

                regionTracker.EvictRegion(region.RegionKey);
            }
        }

        void PurgeStaleRegionRecords()
        {
            var currentTime = timer.Value.CurrentTime;
            foreach (var level in populatedLevels)
            {
                var lastSeen = level.Value;
                if (currentTime - lastSeen > config.MapEvictionTimer)
                {
                    regionKeyBuffer.Add(level.Key);
                }
            }

            foreach (var key in regionKeyBuffer)
            {
                populatedLevels.Remove(key);
            }
        }
    }
}
