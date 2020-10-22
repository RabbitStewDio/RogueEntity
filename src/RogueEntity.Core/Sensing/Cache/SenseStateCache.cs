using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Cache
{
    /// <summary>
    ///   The sense cache provider tracks cache invalidation information for each sense.
    ///   Changes to the map data (and thus to sense resistance data) causes cache
    ///   invalidation for all senses (as we have no easy way of tracking what a map
    ///   change might mean to each active sense source or sensor).
    ///
    ///   This cache processes invalidation requests from two different sources:
    ///
    ///    Global invalidation requests come from sense-resistance map changes,
    ///    which ultimately originate from changes to the map data.
    ///
    ///    Local invalidation requests stem from sense source updates. For instance,
    ///    when a light moves, this may not change the sense resistance information
    ///    (the light carrying actor may not actually block sense data itself), but
    ///    the change to the sense source should trigger an update of the sensors
    ///    in range.
    /// </summary>
    public class SenseStateCache : ISenseStateCacheProvider, IGlobalSenseStateCacheProvider
    {
        readonly int tileSizeX;
        readonly int tileSizeY;
        readonly int resolution;
        int globalLayerMask;
        readonly Dictionary<Type, SenseStateCacheView> senseCaches;
        readonly SenseStateCacheView globalSenseRecord;

        public SenseStateCache(int tileSizeX, int tileSizeY, int resolution)
        {
            if (resolution < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "must be 1 or larger");
            }

            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.resolution = resolution;
            senseCaches = new Dictionary<Type, SenseStateCacheView>();
            globalSenseRecord = new SenseStateCacheView(tileSizeX, tileSizeY, resolution);
        }

        public bool TryGetGlobalSenseCache(out ISenseStateCacheView senseCache)
        {
            senseCache = globalSenseRecord;
            return true;
        }

        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache)
        {
            if (senseCaches.TryGetValue(typeof(TSense), out var raw))
            {
                senseCache = raw;
                return true;
            }

            senseCache = default;
            return false;
        }

        public void ActivateGlobalCacheLayer(MapLayer layer)
        {
            if (layer == MapLayer.Indeterminate)
            {
                globalLayerMask = 0xFF;
            }
            else
            {
                globalLayerMask |= 1 << layer.LayerId;
            }
        }
        
        public void ActivateTrackedSenseSource(Type senseType)
        {
            if (!senseCaches.TryGetValue(senseType, out var record))
            {
                record = new SenseStateCacheView(tileSizeX, tileSizeY, resolution);
                senseCaches[senseType] = record;
            }
        }

        public void MarkClean()
        {
            globalSenseRecord.MarkClean();
            foreach (var record in senseCaches.Values)
            {
                record.MarkClean();
            }
        }

        /// <summary>
        ///   Called when a sense source or sense receptor has been updated.
        /// </summary>
        /// <param name="p"></param>
        /// <typeparam name="TSense"></typeparam>
        public void MarkDirty<TSense>(Position p)
        {
            if (senseCaches.TryGetValue(typeof(TSense), out var r))
            {
                r.MarkDirty(in p);
            }
        }

        /// <summary>
        ///   Called for invalidating sense data when sense resistance data has changed.
        ///   We filter out any notifications that are not in the tracked layers. This way
        ///   you can have map data layers that do not affect sense caches.
        /// </summary>
        /// <param name="p"></param>
        public void MarkAllSensesDirty(in Position p)
        {
            var lm = 1 << p.LayerId;
            if (p.LayerId == 0 || (globalLayerMask & lm) == lm)
            {
                globalSenseRecord.MarkDirty(in p);
            }
        }
    }
}