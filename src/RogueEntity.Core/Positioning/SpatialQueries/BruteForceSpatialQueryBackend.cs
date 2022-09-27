using System;
using System.Collections.Concurrent;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public class BruteForceSpatialQueryBackend<TItemId> : ISpatialQuery<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly EntityRegistry<TItemId> registry;
        readonly ConcurrentDictionary<CachedEntryKey, IDisposable> entries;

        public BruteForceSpatialQueryBackend(EntityRegistry<TItemId> registry)
        {
            this.registry = registry;
            this.entries = new ConcurrentDictionary<CachedEntryKey, IDisposable>();
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

        bool TryGetView<TPosition, TComponent>(out CachedEntry<TPosition, TComponent> entry)
            where TPosition : IPosition<TPosition>
        {
            var key = CachedEntryKey<TPosition, TComponent>.CreateKey(registry);
            entry = (CachedEntry<TPosition, TComponent>)entries.GetOrAdd(key, key.FactoryDelegate);
            return true;
        }

        class CachedEntry<TPosition, TComponent> : IDisposable
            where TPosition : IPosition<TPosition>
        {
            readonly EntityRegistry<TItemId> registry;
            readonly IEntityView<TItemId, TPosition, TComponent>? view;
            readonly ViewDelegates.ApplyWithContext<TItemId, SphereContext, TPosition, TComponent> addSphereResult;
            readonly ViewDelegates.ApplyWithContext<TItemId, BoxContext, TPosition, TComponent> addBoxResult;

            public CachedEntry(EntityRegistry<TItemId> registry)
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

            public void InvokeSphere(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                     in Position pos,
                                     float distance = 1,
                                     DistanceCalculation d = DistanceCalculation.Euclid)
            {
                view?.ApplyWithContext(new SphereContext(receiver, pos, distance, d), addSphereResult);
            }

            public void InvokeBox(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                     in Rectangle3D bounds)
            {
                view?.ApplyWithContext(new BoxContext(receiver, bounds), addBoxResult);
            }

            static void AddResult(IEntityViewControl<TItemId> v,
                                  SphereContext context,
                                  TItemId k,
                                  in TPosition pos,
                                  in TComponent c)
            {
                if (pos.IsInvalid)
                {
                    return;
                }

                var localPos = Position.From(pos);
                var dist = context.DistanceCalculator.Calculate(localPos, context.Origin);
                if (dist <= context.Distance)
                {
                    context.Receiver.Add(new SpatialQueryResult<TItemId, TComponent>(k, localPos, c, (float)dist));
                }
            }

            static void AddResult(IEntityViewControl<TItemId> v,
                                  BoxContext context,
                                  TItemId k,
                                  in TPosition pos,
                                  in TComponent c)
            {
                if (pos.IsInvalid)
                {
                    return;
                }

                if (context.Region.Contains(pos.X, pos.Y, pos.Z))
                {
                    context.Receiver.Add(new SpatialQueryResult<TItemId, TComponent>(k, Position.From(pos), c, 0));
                }
            }

            readonly struct BoxContext
            {
                public readonly BufferList<SpatialQueryResult<TItemId, TComponent>> Receiver;
                public readonly Rectangle3D Region;

                public BoxContext(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver, Rectangle3D region)
                {
                    Receiver = receiver;
                    Region = region;
                }
            }

            readonly struct SphereContext
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
            }


            public void Dispose()
            {
                view?.Dispose();
            }
        }

        readonly struct CachedEntryKey : IEquatable<CachedEntryKey>
        {
            readonly Type positionType;
            readonly Type componentType;
            internal readonly EntityRegistry<TItemId> Registry;
            public readonly Func<CachedEntryKey, IDisposable> FactoryDelegate;

            public CachedEntryKey(Type positionType,
                                  Type componentType,
                                  EntityRegistry<TItemId> registry,
                                  Func<CachedEntryKey, IDisposable> factoryDelegate)
            {
                this.positionType = positionType ?? throw new ArgumentNullException(nameof(positionType));
                this.componentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
                this.FactoryDelegate = factoryDelegate ?? throw new ArgumentNullException(nameof(factoryDelegate));
                this.Registry = registry ?? throw new ArgumentNullException(nameof(registry));
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

        /// <summary>
        ///   A factory struct to hold the static but generically typed factory instance. 
        /// </summary>
        /// <typeparam name="TPosition"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        readonly struct CachedEntryKey<TPosition, TComponent>
            where TPosition : IPosition<TPosition>
        {
            static readonly Func<CachedEntryKey, IDisposable> factoryDelegate = k => new CachedEntry<TPosition, TComponent>(k.Registry);

            public static CachedEntryKey CreateKey(EntityRegistry<TItemId> registry)
            {
                return new CachedEntryKey(typeof(TPosition), typeof(TComponent), registry, factoryDelegate);
            }
        }
    }
}
