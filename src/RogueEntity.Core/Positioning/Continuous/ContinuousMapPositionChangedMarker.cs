using System;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Positioning.Continuous
{
    public readonly struct ContinuousMapPositionChangedMarker: IPositionChangeMarker<ContinuousMapPosition>
    {
        public readonly ContinuousMapPosition PreviousPosition;

        ContinuousMapPositionChangedMarker(ContinuousMapPosition previousPosition)
        {
            this.PreviousPosition = previousPosition;
        }

        ContinuousMapPosition IPositionChangeMarker<ContinuousMapPosition>.PreviousPosition => PreviousPosition;

        public static void Update<TKey>(IEntityViewControl<TKey> v, TKey k, ContinuousMapPosition previous)
            where TKey : struct, IEntityKey
        {
            if (v.GetComponent(k, out ContinuousMapPositionChangedMarker marker))
            {
                if (marker.PreviousPosition == previous)
                {
                    v.RemoveComponent<ContinuousMapPositionChangedMarker>(k);
                }
                return;
            }
            v.AssignComponent(k, new ContinuousMapPositionChangedMarker(previous));
        }

        public static IDisposable InstallChangeHandler<TEntityKey>(EntityRegistry<TEntityKey> registry)
            where TEntityKey : struct, IEntityKey
        {
            var x = new PositionChangeTracker<TEntityKey>(registry);
            x.Install();
            return x;
        }

        class PositionChangeTracker<TEntityKey> : EntityChangeTracker<TEntityKey, ContinuousMapPosition>
            where TEntityKey : struct, IEntityKey
        {
            public PositionChangeTracker(EntityRegistry<TEntityKey> registry) : base(registry)
            {
            }

            protected override void OnPositionDestroyed(object sender, (TEntityKey key, ContinuousMapPosition old) e)
            {
                Update(Registry, e.key, e.old);
            }

            protected override void OnPositionUpdated(object sender, (TEntityKey key, ContinuousMapPosition old) e)
            {
                if (Registry.GetComponent(e.key, out ContinuousMapPosition current) &&
                    current != e.old)
                {
                    Update(Registry, e.key, e.old);
                }
            }

            protected override void OnPositionCreated(object sender, TEntityKey e)
            {
                Registry.AssignOrReplace(e, new ContinuousMapPositionChangedMarker());
            }
        }
    }
}