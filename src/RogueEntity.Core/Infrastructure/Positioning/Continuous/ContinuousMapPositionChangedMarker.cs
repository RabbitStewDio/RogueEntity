using System;
using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Positioning.Continuous
{
    public readonly struct ContinuousMapPositionChangedMarker
    {
        public readonly ContinuousMapPosition PreviousPosition;

        ContinuousMapPositionChangedMarker(ContinuousMapPosition previousPosition)
        {
            this.PreviousPosition = previousPosition;
        }

        public static void Update<TKey>(IEntityViewControl<TKey> v, TKey k, ContinuousMapPosition previous)
            where TKey : IEntityKey
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
            where TEntityKey : IEntityKey
        {
            var x = new PositionChangeTracker<TEntityKey>(registry);
            x.Install();
            return x;
        }

        class PositionChangeTracker<TEntityKey> : EntityChangeTracker<TEntityKey, ContinuousMapPosition>
            where TEntityKey : IEntityKey
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