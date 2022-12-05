using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RogueEntity.Core.Positioning.SpatialQueries;

public abstract class SpatialQueryBackendBase<TItemId>
    where TItemId : struct, IEntityKey
{
    protected interface ICachedEntry : IDisposable
    {
        public void RefreshIndex();
    }
    
    protected interface ICachedEntry<TComponent> : ICachedEntry
    {
        public void InvokeSphere(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                 in Position pos,
                                 float distance = 1,
                                 DistanceCalculation d = DistanceCalculation.Euclid);

        public void InvokeBox(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                              in Rectangle3D bounds);
    }

    protected readonly EntityRegistry<TItemId> Registry;
    readonly ConcurrentDictionary<CachedEntryKey, ICachedEntry> entries;

    public SpatialQueryBackendBase(EntityRegistry<TItemId> registry)
    {
        this.Registry = registry;
        this.entries = new ConcurrentDictionary<CachedEntryKey, ICachedEntry>();
    }

    public BufferList<SpatialQueryResult<TItemId, TComponent>> QuerySphere<TComponent>(in Position pos,
                                                                                       float distance = 1,
                                                                                       DistanceCalculation d = DistanceCalculation.Euclid,
                                                                                       BufferList<SpatialQueryResult<TItemId, TComponent>>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);

        if (TryGetView<EntityGridPosition, TComponent>(out var gridView))
        {
            gridView.InvokeSphere(buffer, pos, distance, d);
        }

        if (TryGetView<ContinuousMapPosition, TComponent>(out var conView))
        {
            conView.InvokeSphere(buffer, pos, distance, d);
        }

        return buffer;
    }

    public BufferList<SpatialQueryResult<TItemId, TComponent>> QueryBox<TComponent>(in Rectangle3D queryRegion,
                                                                                    BufferList<SpatialQueryResult<TItemId, TComponent>>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);

        if (TryGetView<EntityGridPosition, TComponent>(out var gridView))
        {
            gridView.InvokeBox(buffer, queryRegion);
        }

        if (TryGetView<ContinuousMapPosition, TComponent>(out var conView))
        {
            conView.InvokeBox(buffer, queryRegion);
        }

        return buffer;
    }

    bool TryGetView<TPosition, TComponent>(out ICachedEntry<TComponent> entry)
        where TPosition : struct, IPosition<TPosition>
    {
        var key = new CachedEntryKey(typeof(TPosition), typeof(TComponent));
        entry = (ICachedEntry<TComponent>)entries.GetOrAdd(key, GetEntryFactory<TPosition, TComponent>);
        return true;
    }

    protected abstract ICachedEntry GetEntryFactory<TPosition, TComponent>(CachedEntryKey arg)
        where TPosition : struct, IPosition<TPosition>;


    protected readonly struct CachedEntryKey : IEquatable<CachedEntryKey>
    {
        readonly Type positionType;
        readonly Type componentType;

        public CachedEntryKey(Type positionType,
                              Type componentType)
        {
            this.positionType = positionType ?? throw new ArgumentNullException(nameof(positionType));
            this.componentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        }

        public bool Equals(CachedEntryKey other)
        {
            return positionType == other.positionType && componentType == other.componentType;
        }

        public override bool Equals(object obj)
        {
            return obj is CachedEntryKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (positionType.GetHashCode() * 397) ^ componentType.GetHashCode();
            }
        }

        public static bool operator ==(CachedEntryKey left, CachedEntryKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CachedEntryKey left, CachedEntryKey right)
        {
            return !left.Equals(right);
        }
    }

    protected abstract class CachedEntryBase<TPosition, TComponent> : ICachedEntry<TComponent>
        where TPosition : struct, IPosition<TPosition>
    {
        protected readonly EntityRegistry<TItemId> registry;
        protected readonly IEntityView<TItemId, TPosition, TComponent>? view;
        protected readonly ViewDelegates.ApplyWithContext<TItemId, SphereContext, TPosition, TComponent> addSphereResult;
        protected readonly ViewDelegates.ApplyWithContext<TItemId, BoxContext, TPosition, TComponent> addBoxResult;

        protected CachedEntryBase(EntityRegistry<TItemId> registry)
        {
            this.registry = registry;
            if (this.registry.IsManaged<TPosition>() &&
                this.registry.IsManaged<TComponent>())
            {
                view = this.registry.PersistentView<TPosition, TComponent>();
            }

            this.addSphereResult = AddResult;
            this.addBoxResult = AddResult;
        }

        public virtual void Dispose()
        {
            view?.Dispose();
        }

        public virtual void RefreshIndex()
        {
        }

        public abstract void InvokeSphere(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver, in Position pos, float distance = 1, DistanceCalculation d = DistanceCalculation.Euclid);
        public abstract void InvokeBox(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver, in Rectangle3D bounds);

        protected static void AddResult(IEntityViewControl<TItemId> v,
                                        SphereContext context,
                                        TItemId k,
                                        in TPosition pos,
                                        in TComponent c)
        {
            context.AddResult(k, pos, c);
        }

        protected static void AddResult(IEntityViewControl<TItemId> v,
                                        BoxContext context,
                                        TItemId k,
                                        in TPosition pos,
                                        in TComponent c)
        {
            context.AddResult(k, pos, c);
        }

        protected readonly struct BoxContext
        {
            public readonly BufferList<SpatialQueryResult<TItemId, TComponent>> Receiver;
            public readonly Rectangle3D Region;

            public BoxContext(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver, Rectangle3D region)
            {
                Receiver = receiver;
                Region = region;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddResult(TItemId k,
                                  in TPosition pos,
                                  in TComponent c)
            {
                if (pos.IsInvalid)
                {
                    return;
                }

                if (Region.Contains(pos.X, pos.Y, pos.Z))
                {
                    Receiver.Add(new SpatialQueryResult<TItemId, TComponent>(k, Position.From(pos), c, 0));
                }
            }
        }

        protected readonly struct SphereContext
        {
            public readonly BufferList<SpatialQueryResult<TItemId, TComponent>> Receiver;
            public readonly Position Origin;
            public readonly DistanceCalculation DistanceCalculator;
            public readonly float Distance;

            public SphereContext(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                 in Position origin,
                                 float distance,
                                 DistanceCalculation distanceCalculator)
            {
                Receiver = receiver;
                Origin = origin;
                DistanceCalculator = distanceCalculator;
                Distance = distance;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddResult(TItemId k,
                                  in TPosition pos,
                                  in TComponent c)
            {
                if (pos.IsInvalid)
                {
                    return;
                }

                var localPos = Position.From(pos);
                var dist = DistanceCalculator.Calculate(localPos, Origin);
                if (dist <= Distance)
                {
                    Receiver.Add(new SpatialQueryResult<TItemId, TComponent>(k, localPos, c, (float)dist));
                }
            }
        }
    }
}