using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Cache
{
    public class SenseStateCacheProvider: ISenseStateCacheProvider
    {
        readonly int resolution;
        readonly Dictionary<Type, SenseStateCacheViewRecord> senseCaches;

        public SenseStateCacheProvider(int resolution)
        {
            if (resolution < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "must be 1 or larger");
            }
            
            this.resolution = resolution;
            senseCaches = new Dictionary<Type, SenseStateCacheViewRecord>();
        }
        
        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache)
        {
            if (senseCaches.TryGetValue(typeof(TSense), out var raw))
            {
                senseCache = raw.Cache;
                return true;
            }

            senseCache = default;
            return false;
        }

        public void DefineSenseCacheLayer<TSense, TItemId, TGameContext>(TGameContext context)
            where TGameContext: IGridMapContext<TGameContext, TItemId>
        {
            if (!senseCaches.TryGetValue(typeof(TSense), out var record))
            {
                record = new SenseStateCacheViewRecord(new SenseStateCacheView(resolution));
            }

            foreach (var l in context.GridLayers())
            {
                record = record.WithLayer(l);
            }

            senseCaches[typeof(TSense)] = record;
        }

        public void MarkClean()
        {
            foreach (var record in senseCaches.Values)
            {
                record.Cache.MarkClean();
            }
        }

        public void MarkDirty<TSense>(MapLayer l, Position p)
        {
            if (senseCaches.TryGetValue(typeof(TSense), out var r))
            {
                r.Cache.MarkDirty(p);
            }
        }
        
        public void MarkDirty(Position p)
        {
            foreach (var record in senseCaches.Values)
            {
                if (record.Matches(p.LayerId))
                {
                    record.Cache.MarkDirty(p);
                }
            }
        }

        readonly struct SenseStateCacheViewRecord
        {
            readonly ulong layerMask;
            public readonly SenseStateCacheView Cache;

            public SenseStateCacheViewRecord(SenseStateCacheView cache, ulong layerMask = 0)
            {
                this.layerMask = layerMask;
                this.Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            }

            public bool Matches(byte layerId)
            {
                if (layerId == 0) return true;
                var layerBitMask = 1ul << layerId;
                return (layerMask & layerBitMask) != 0;
            }

            public SenseStateCacheViewRecord WithLayer(MapLayer layer)
            {
                var layerBitMask = 0ul;
                if (layer == MapLayer.Indeterminate)
                {
                    layerBitMask = ulong.MaxValue;
                }
                else
                {
                    layerBitMask = 1ul << layer.LayerId;
                }
                 
                return new SenseStateCacheViewRecord(Cache, layerMask | layerBitMask);
            }
        }
    }
}