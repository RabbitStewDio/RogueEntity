using EnTTSharp.Entities;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Utils.SpatialIndex;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.SpatialQueries;

public class QuadTreeSpatialQueryBackend<TEntityKey, TComponent> : SpatialQueryBackendBase<TEntityKey, TComponent>
    where TEntityKey : struct, IEntityKey
{
    readonly IItemResolver<TEntityKey> itemRegistry;
    readonly DynamicDataViewConfiguration config;
    readonly ObjectPool<List<FreeListIndex>> sharedPool;

    public QuadTreeSpatialQueryBackend(IItemResolver<TEntityKey> itemRegistry,
                                       EntityRegistry<TEntityKey> registry,
                                       DynamicDataViewConfiguration config) : base(registry)
    {
        this.config = config;
        this.itemRegistry = itemRegistry;
        this.sharedPool = new DefaultObjectPool<List<FreeListIndex>>(new ListObjectPoolPolicy<FreeListIndex>());
    }

    protected override ICachedEntry GetEntryFactory<TPosition>(Type arg)
    {
        return new CachedEntry<TPosition>(itemRegistry, Registry, sharedPool, config);
    }

    class CachedEntry<TPosition> : CachedEntryBase<TPosition>
        where TPosition : struct, IPosition<TPosition>
    {
        readonly IItemResolver<TEntityKey> itemRegistry;
        readonly ObjectPool<List<FreeListIndex>> sharedPool;
        readonly DynamicDataViewConfiguration config;
        readonly IEntityView<TEntityKey, TPosition> positionView;
        readonly IEntityView<TEntityKey, TPosition, MapPositionChangedMarker> positionUpdateView;
        readonly Dictionary<int, IndexEntry> spatialIndex;
        readonly Func<int, IndexEntry> valueFactory;

        public CachedEntry(IItemResolver<TEntityKey> itemRegistry,
                           EntityRegistry<TEntityKey> registry,
                           ObjectPool<List<FreeListIndex>> sharedPool,
                           DynamicDataViewConfiguration config) : base(registry)
        {
            this.itemRegistry = itemRegistry;
            this.sharedPool = sharedPool;
            this.config = config;
            this.spatialIndex = new Dictionary<int, IndexEntry>();
            this.positionView = registry.PersistentView<TPosition>();
            this.positionUpdateView = registry.PersistentView<TPosition, MapPositionChangedMarker>();
            this.valueFactory = Create;
        }

        public override void RefreshIndex()
        {
            positionUpdateView.Apply(Remove);
            positionUpdateView.Apply(AddOrUpdate);

            foreach (var p in spatialIndex)
            {
                var idx = p.Value;
                idx.RemoveObsoleteEntries();
            }
        }

        void AddOrUpdate(IEntityViewControl<TEntityKey> v, TEntityKey k, ref TPosition pos, ref MapPositionChangedMarker changeMarker)
        {
            if (pos.IsInvalid)
            {
                return;
            }

            var z = pos.GridZ;
            if (!spatialIndex.TryGetValue(z, out var idx))
            {
                idx = valueFactory(z);
                spatialIndex[z] = idx;
            }

            if (!itemRegistry.TryQueryData<BodySize>(k, out var bs))
            {
                bs = BodySize.Empty;
            }

            idx.Add(k, bs.ToBoundingBox(pos));
        }

        void Remove(IEntityViewControl<TEntityKey> v, TEntityKey k, ref TPosition pos, ref MapPositionChangedMarker changeMarker)
        {
            var oldPos = changeMarker.PreviousPosition;
            var z = oldPos.GridZ;
            if (!spatialIndex.TryGetValue(z, out var idx))
            {
                idx = valueFactory(z);
                spatialIndex[z] = idx;
            }

            if (!itemRegistry.TryQueryData<BodySize>(k, out var bs))
            {
                bs = BodySize.Empty;
            }

            idx.Add(k, bs.ToBoundingBox(oldPos));
        }

        class IndexEntry
        {
            readonly EntityRegistry<TEntityKey> entityRegistry;
            readonly QuadTree2D<TEntityKey> index;
            readonly Func<FreeListIndex, bool> elementSelector;

            public IndexEntry(EntityRegistry<TEntityKey> entityRegistry,
                              QuadTree2D<TEntityKey> index)
            {
                this.entityRegistry = entityRegistry;
                this.index = index;
                this.elementSelector = RemoveEntry;
            }

            public FreeListIndex Add(TEntityKey entity, in BoundingBox bb)
            {
                return this.index.Insert(entity, bb);
            }

            bool RemoveEntry(FreeListIndex idx)
            {
                if (!index.TryGet(idx, out var entity, out _))
                {
                    // there is no entry for this index position, so its probably obsolete.
                    return true;
                }

                if (!entityRegistry.GetComponent<MapPositionChangedMarker>(entity, out _))
                {
                    // there is no change marker, so this position has not changed this frame
                    return false;
                }

                if (!entityRegistry.GetComponent<TPosition>(entity, out var pos))
                {
                    // has a change marker, but no position? should not happen, but it means the entity no longer
                    // has a spatial position and can be removed from the index.
                    return true;
                }

                return pos.IsInvalid;
            }

            public void Clear()
            {
                index.Clear();
            }

            public void RemoveObsoleteEntries()
            {
                index.RemoveBulk(elementSelector);
            }

            public BufferList<FreeListIndex> Query(BoundingBox boundingBox, BufferList<FreeListIndex> buffer)
            {
                return index.QueryIndex(boundingBox, buffer);
            }

            public bool TryGet(FreeListIndex idx, out TEntityKey entity, out BoundingBox bb)
            {
                return index.TryGet(idx, out entity, out bb);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var x in spatialIndex)
            {
                x.Value.Clear();
            }

            spatialIndex.Clear();
        }

        IndexEntry Create(int z)
        {
            return new IndexEntry(registry, new QuadTree2D<TEntityKey>(sharedPool, config));
        }

        public override void InvokeSphere(BufferList<SpatialQueryResult<TEntityKey, TComponent>> receiver,
                                          in Position pos,
                                          float distance = 1,
                                          DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (view == null)
            {
                return;
            }

            if (!spatialIndex.TryGetValue(pos.GridZ, out var qt))
            {
                return;
            }

            var sphereContext = new SphereContext(receiver, pos, distance, d);
            var distanceInt = (int)Math.Ceiling(Math.Max(0, distance));
            var boundingBox = BoundingBox.From(pos.GridX - distanceInt,
                                               pos.GridY - distanceInt);
            using var buffer = BufferListPool<FreeListIndex>.GetPooled();
            foreach (var idx in qt.Query(boundingBox, buffer))
            {
                if (qt.TryGet(idx, out var k, out _) &&
                    itemRegistry.TryQueryData<TComponent>(k, out var data) &&
                    positionView.GetComponent<TPosition>(k, out var position))
                {
                    sphereContext.AddResult(k, position, data);
                }
            }
        }

        public override void InvokeBox(BufferList<SpatialQueryResult<TEntityKey, TComponent>> receiver, in Rectangle3D bounds)
        {
            if (view == null)
            {
                return;
            }

            var sphereContext = new BoxContext(receiver, bounds);
            var boundingBox = BoundingBox.From(bounds.ToLayerSlice());
            using var buffer = BufferListPool<FreeListIndex>.GetPooled();
            foreach (var z in bounds.Layers)
            {
                if (!spatialIndex.TryGetValue(z, out var qt))
                {
                    continue;
                }

                foreach (var idx in qt.Query(boundingBox, buffer))
                {
                    if (qt.TryGet(idx, out var k, out _) &&
                        itemRegistry.TryQueryData<TComponent>(k, out var data) &&
                        positionView.GetComponent<TPosition>(k, out var position))
                    {
                        sphereContext.AddResult(k, position, data);
                    }
                }
            }
        }
    }
}