using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Positioning.Grid
{
    [EntityComponent]
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct EntityGridPositionChangedMarker
    {
        [DataMember]
        [Key(0)]
        public readonly EntityGridPosition PreviousPosition;

        [SerializationConstructor]
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