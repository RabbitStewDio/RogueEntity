using EnTTSharp;
using EnTTSharp.Entities;
using System;

namespace RogueEntity.Core.Positioning;

public static class MapPositionChangeTracker<TPosition>
    where TPosition: IPosition<TPosition>
{
    public static IDisposable InstallChangeHandler<TEntityKey>(EntityRegistry<TEntityKey> registry)
        where TEntityKey : struct, IEntityKey
    {
        var x = new PositionChangeTracker<TEntityKey>(registry);
        x.Install();
        return x;
    }

    class PositionChangeTracker<TEntityKey> : EntityChangeTracker<TEntityKey, TPosition>
        where TEntityKey : struct, IEntityKey
    {
        public PositionChangeTracker(EntityRegistry<TEntityKey> registry) : base(registry)
        {
        }

        protected override void OnPositionDestroyed(object sender, (TEntityKey key, Optional<TPosition> old) e)
        {
            if (e.old.TryGetValue(out var old))
            {
                MapPositionChangedMarker.Update(Registry, e.key, Position.From(old));
            }
            else
            {
                MapPositionChangedMarker.Update(Registry, e.key, Position.Invalid);
            }
        }

        protected override void OnPositionUpdated(object sender, (TEntityKey key, TPosition c) e)
        {
            MapPositionChangedMarker.Update(Registry, e.key, Position.From(e.c));
        }

        protected override void OnPositionCreated(object sender, (TEntityKey key, TPosition c) e)
        {
            MapPositionChangedMarker.Update(Registry, e.key, Position.From(e.c));
        }
    }
}