using EnTTSharp.Entities;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Positioning;

public class ItemPlacementLocationService<TItemId> : IItemPlacementLocationService<TItemId>
    where TItemId : struct, IEntityKey
{
    static readonly ILogger logger = SLog.ForContext<ItemPlacementLocationService<TItemId>>();
    readonly ObjectPool<Tuple<SortedSet<Position2D>, DistanceFromCenterComparer>> positionsPool;
    readonly IItemResolver<TItemId> itemResolver;
    readonly IBulkDataStorageMetaData<TItemId> metadata;
    readonly IMapContext<TItemId> index;

    public ItemPlacementLocationService(IItemResolver<TItemId> resolver,
                                               IMapContext<TItemId> index)
    {
        this.itemResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        this.metadata = this.itemResolver.EntityMetaData;
        this.index = index ?? throw new ArgumentNullException(nameof(index));
        this.positionsPool = new DefaultObjectPool<Tuple<SortedSet<Position2D>, DistanceFromCenterComparer>>(new SortedSetPolicy());
    }

    public bool TryFindAvailableSpace<TPosition>(in TItemId itemToBePlaced,
                                                 in TPosition origin,
                                                 out TPosition placementPos,
                                                 int searchRadius = 10)
        where TPosition : struct, IPosition<TPosition>
    {
        if (!metadata.IsReferenceEntity(itemToBePlaced))
        {
            logger.Verbose("Given entity {Item} is not a reference item and cannot be placed on a continuous position", itemToBePlaced);
            placementPos = default;
            return false;
        }

        if (!itemResolver.TryQueryData<BodySize>(itemToBePlaced, out var bodySize))
        {
            // assume item is a point source
            bodySize = BodySize.Empty;
        }

        if (origin.LayerId == MapLayer.Indeterminate.LayerId)
        {
            // unable to place ..
            logger.Verbose("Given position {Position} has an indeterminate map layer", origin);
            placementPos = default;
            return false;
        }

        if (origin.IsInvalid)
        {
            logger.Verbose("Given position {Position} is invalid", origin);
            placementPos = default;
            return false;
        }

        if (!index.TryGetMapDataFor(origin.LayerId, out var map))
        {
            // unable to place ..
            logger.Verbose("Given position {Position} has an indeterminate map layer", origin);
            placementPos = default;
            return false;
        }

        using var buffer = BufferListPool<(TItemId, TPosition)>.GetPooled();
        var positions = positionsPool.Get();
        positions.Item2.Center = origin.ToGridXY();
        var searchArea = new Rectangle(origin.ToGridXY(), searchRadius, searchRadius);
        foreach (var pos in searchArea.Contents)
        {
            positions.Item1.Add(pos);
        }

        var itemStack = itemResolver.QueryStackSize(itemToBePlaced);

        var queryResults = map.QueryItemArea<TPosition>(searchArea, origin.GridZ, buffer);
        if (queryResults.Count > 0)
        {
            foreach (var (entity, position) in queryResults)
            {
                positions.Item1.Remove(position.ToGridXY());
                if (!itemResolver.IsSameStackType(entity, itemToBePlaced))
                {
                    continue;
                }

                var stackSize = itemResolver.QueryStackSize(entity);
                if (itemStack.Count + stackSize.Count < stackSize.MaximumStackSize)
                {
                    placementPos = position;
                    return true;
                }
            }
        }

        if (positions.Item1.Count == 0)
        {
            placementPos = default;
            return false;
        }

        if (TryFindEmptySpaceForArea(positions.Item1, bodySize, out var r))
        {
            placementPos = origin.WithPosition(r.X, r.Y);
            return true;
        }


        placementPos = default;
        return false;
    }


    public bool TryFindEmptySpace<TPosition>(in TPosition origin, in BodySize bodySize, out TPosition placementPos, int searchRadius = 10) where TPosition : struct, IPosition<TPosition>
    {
        if (origin.LayerId == MapLayer.Indeterminate.LayerId)
        {
            // unable to place ..
            logger.Verbose("Given position {Position} has an indeterminate map layer", origin);
            placementPos = default;
            return false;
        }

        if (origin.IsInvalid)
        {
            logger.Verbose("Given position {Position} is invalid", origin);
            placementPos = default;
            return false;
        }

        if (!index.TryGetMapDataFor(origin.LayerId, out var map))
        {
            // unable to place ..
            logger.Verbose("Given position {Position} has an indeterminate map layer", origin);
            placementPos = default;
            return false;
        }


        using var buffer = BufferListPool<(TItemId, TPosition)>.GetPooled();
        using var positions = positionsPool.GetPooled();
        positions.Data.Item2.Center = origin.ToGridXY();
        var effectiveSearchRadius = Math.Min(1, searchRadius - 1);
        var searchArea = new Rectangle(origin.ToGridXY(), effectiveSearchRadius, effectiveSearchRadius);
        foreach (var pos in searchArea.Contents)
        {
            positions.Data.Item1.Add(pos);
        }

        var queryResults = map.QueryItemArea<TPosition>(searchArea, origin.GridZ, buffer);
        if (queryResults.Count > 0)
        {
            foreach (var (_, position) in queryResults)
            {
                positions.Data.Item1.Remove(position.ToGridXY());
            }
        }

        if (positions.Data.Item1.Count == 0)
        {
            logger.Verbose("Unable to find any empty position at {Origin} with search radius {SearchRadius}", origin, effectiveSearchRadius);
            placementPos = default;
            return false;
        }

        if (TryFindEmptySpaceForArea(positions.Data.Item1, bodySize, out var r))
        {
            placementPos = origin.WithPosition(r.X, r.Y);
            return true;
        }

        placementPos = default;
        return false;
    }

    bool TryFindEmptySpaceForArea(SortedSet<Position2D> freeSpaces, BodySize bs, out Position2D result)
    {
        
        if (bs == BodySize.Empty || bs == BodySize.OneByOne)
        {
            result = freeSpaces.First();
            return true;
        }

        foreach (var fs in freeSpaces)
        {
            if (IsBodyAreaOccupied(freeSpaces, bs, fs))
            {
                continue;
            }

            result = fs;
            return true;
        }

        result = default;
        return false;
    }

    bool IsBodyAreaOccupied(SortedSet<Position2D> freeSpaces, BodySize bs, Position2D position)
    {
        foreach (var p in bs.ToRectangle(position).Contents)
        {
            if (!freeSpaces.Contains(p))
            {
                return false;
            }
        }

        return true;
    }

    class SortedSetPolicy : IPooledObjectPolicy<Tuple<SortedSet<Position2D>, DistanceFromCenterComparer>>
    {
        public Tuple<SortedSet<Position2D>, DistanceFromCenterComparer> Create()
        {
            var cmp = new DistanceFromCenterComparer();
            var set = new SortedSet<Position2D>();
            return Tuple.Create(set, cmp);
        }

        public bool Return(Tuple<SortedSet<Position2D>, DistanceFromCenterComparer> obj)
        {
            obj.Item1.Clear();
            return true;
        }
    }

    class DistanceFromCenterComparer : IComparer<Position2D>
    {
        public Position2D Center { get; set; }

        public int Compare(Position2D x, Position2D y)
        {
            var distX = DistanceSquared(x);
            var distY = DistanceSquared(y);
            var cmp = distX.CompareTo(distY);
            if (cmp != 0)
            {
                return cmp;
            }

            return x.CompareTo(y);
        }

        float DistanceSquared(Position2D p)
        {
            var dx = p.X - Center.X;
            var dy = p.Y - Center.Y;
            return (dx * dx) + (dy * dy);
        }
    }
}