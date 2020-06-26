using System;
using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Positioning.Grid
{
    public readonly struct EntityGridPositionChangedMarker
    {
        public readonly EntityGridPosition PreviousPosition;

        EntityGridPositionChangedMarker(EntityGridPosition previousPosition)
        {
            this.PreviousPosition = previousPosition;
        }

        public static void Update<TKey>(IEntityViewControl<TKey> v, TKey k, EntityGridPosition previous) 
            where TKey : IEntityKey
        {
            if (v.GetComponent(k, out EntityGridPositionChangedMarker marker))
            {
                if (marker.PreviousPosition == previous)
                {
                    v.RemoveComponent<EntityGridPositionChangedMarker>(k);
                }
                return;
            }
            v.AssignComponent(k, new EntityGridPositionChangedMarker(previous));
        }

        public static IDisposable InstallChangeHandler<TEntityKey>(EntityRegistry<TEntityKey> registry) 
            where TEntityKey : IEntityKey
        {
            var x = new PositionChangeTracker<TEntityKey>(registry);
            x.Install();
            return x;
        }

        class PositionChangeTracker<TEntityKey> : EntityChangeTracker<TEntityKey, EntityGridPosition> 
            where TEntityKey : IEntityKey
        {
            public PositionChangeTracker(EntityRegistry<TEntityKey> registry) : base(registry)
            {
            }

            protected override void OnPositionDestroyed(object sender, (TEntityKey key, EntityGridPosition old) e)
            {
                Update(Registry, e.key, e.old);
            }

            protected override void OnPositionUpdated(object sender, (TEntityKey key, EntityGridPosition old) e)
            {
                if (Registry.GetComponent(e.key, out EntityGridPosition current) &&
                    current != e.old)
                {
                    Update(Registry, e.key, e.old);
                }
            }

            protected override void OnPositionCreated(object sender, TEntityKey e)
            {
                Update(Registry, e, default);
            }
        }

    }
}