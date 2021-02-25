using System;
using System.Collections.Concurrent;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public class BruteForceSpatialQueryBackend<TItemId> : ISpatialQuery<TItemId>
        where TItemId : IEntityKey
    {
        readonly EntityRegistry<TItemId> registry;
        readonly ConcurrentDictionary<CachedEntryKey, IDisposable> entries;

        public BruteForceSpatialQueryBackend(EntityRegistry<TItemId> registry)
        {
            this.registry = registry;
            this.entries = new ConcurrentDictionary<CachedEntryKey, IDisposable>();
        }


        public BufferList<SpatialQueryResult<TItemId, TComponent>> Query2D<TComponent>(in Position pos,
                                                                                       float distance = 1,
                                                                                       DistanceCalculation d = DistanceCalculation.Euclid,
                                                                                       BufferList<SpatialQueryResult<TItemId, TComponent>> buffer = null)
        {
            BufferList.PrepareBuffer(buffer);

            if (TryGetView<EntityGridPosition, TComponent>(out var gridView))
            {
                gridView.Invoke2D(buffer, pos, distance, d);
            }

            if (TryGetView<ContinuousMapPosition, TComponent>(out var conView))
            {
                conView.Invoke2D(buffer, pos, distance, d);
            }

            return buffer;
        }

        public BufferList<SpatialQueryResult<TItemId, TComponent>> Query3D<TComponent>(in Position pos,
                                                                                       float distance = 1,
                                                                                       DistanceCalculation d = DistanceCalculation.Euclid,
                                                                                       BufferList<SpatialQueryResult<TItemId, TComponent>> buffer = null)
        {
            BufferList.PrepareBuffer(buffer);

            if (TryGetView<EntityGridPosition, TComponent>(out var gridView))
            {
                gridView.Invoke3D(buffer, pos, distance, d);
            }

            if (TryGetView<ContinuousMapPosition, TComponent>(out var conView))
            {
                conView.Invoke3D(buffer, pos, distance, d);
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
            readonly IEntityView<TItemId, TPosition, TComponent> view;
            readonly ViewDelegates.ApplyWithContext<TItemId, Context, TPosition, TComponent> add2D;
            readonly ViewDelegates.ApplyWithContext<TItemId, Context, TPosition, TComponent> add3D;

            public CachedEntry(EntityRegistry<TItemId> registry)
            {
                this.registry = registry;
                if (this.registry.IsManaged<TPosition>() &&
                    this.registry.IsManaged<TComponent>())
                {
                    view = this.registry.PersistentView<TPosition, TComponent>();
                }

                this.add2D = AddResult2D;
                this.add3D = AddResult3D;
            }

            public void Invoke2D(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                 in Position pos,
                                 float distance = 1,
                                 DistanceCalculation d = DistanceCalculation.Euclid)
            {
                view?.ApplyWithContext(new Context(receiver, pos, distance, d), add2D);
            }

            public void Invoke3D(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
                                 in Position pos,
                                 float distance = 1,
                                 DistanceCalculation d = DistanceCalculation.Euclid)
            {
                view?.ApplyWithContext(new Context(receiver, pos, distance, d), add3D);
            }

            static void AddResult2D(IEntityViewControl<TItemId> v,
                                    Context context,
                                    TItemId k,
                                    in TPosition pos,
                                    in TComponent c)
            {
                if (pos.IsInvalid)
                {
                    return;
                }

                if (pos.GridZ != context.Origin.GridZ)
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

            static void AddResult3D(IEntityViewControl<TItemId> v,
                                    Context context,
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

            readonly struct Context
            {
                public readonly BufferList<SpatialQueryResult<TItemId, TComponent>> Receiver;
                public readonly Position Origin;
                public readonly DistanceCalculation DistanceCalculator;
                public readonly float Distance;

                public Context(BufferList<SpatialQueryResult<TItemId, TComponent>> receiver,
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

            public CachedEntryKey([NotNull] Type positionType,
                                  [NotNull] Type componentType,
                                  [NotNull] EntityRegistry<TItemId> registry,
                                  [NotNull] Func<CachedEntryKey, IDisposable> factoryDelegate)
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
            static readonly Func<CachedEntryKey, IDisposable> FactoryDelegate = k => new CachedEntry<TPosition, TComponent>(k.Registry);

            public static CachedEntryKey CreateKey(EntityRegistry<TItemId> registry)
            {
                return new CachedEntryKey(typeof(TPosition), typeof(TComponent), registry, FactoryDelegate);
            }
        }
    }
}
