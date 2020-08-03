using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public class ActorPositionTrait<TGameContext>: IActorComponentTrait<TGameContext, EntityMapPosition>,
                                                   IActorComponentTrait<TGameContext, EntityMapPositionChangedMarker>
        where TGameContext: IGameContext<TGameContext>, IMapContext
    {
        public ActorPositionTrait()
        {
            Id = "Actor.Generic.Position";
            Priority = 100;
        }

        public string Id { get; }
        public int Priority { get; }

        public void Initialize(IEntityViewControl v, TGameContext context, EntityKey k, in ActorReferenceInfo<TGameContext> actorInfo)
        {
        }

        public void Apply(IEntityViewControl v, TGameContext context, EntityKey k, in ActorReferenceInfo<TGameContext> actorInfo)
        {
        }

        public bool TryQuery(IEntityViewControl v, TGameContext context, ActorReference k, out EntityMapPosition t)
        {
            if (context.ActorResolver.TryResolveReference(k, out var key))
            {
                return v.GetComponent(key, out t);
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl v, TGameContext context, ActorReference k, out EntityMapPositionChangedMarker t)
        {
            if (context.ActorResolver.TryResolveReference(k, out var key))
            {
                return v.GetComponent(key, out t);
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl v, TGameContext context, ActorReference k, in EntityMapPositionChangedMarker t)
        {
            return false;
        }

        public bool TryUpdate(IEntityViewControl v, TGameContext context, ActorReference k, in EntityMapPosition desiredPosition)
        {
            if (context.ActorResolver.TryResolveReference(k, out var key))
            {
                if (!v.GetComponent(key, out EntityMapPosition previousPosition))
                {
                    previousPosition = EntityMapPosition.Invalid;
                }

                if (previousPosition == desiredPosition)
                {
                    return true;
                }

                if (desiredPosition == EntityMapPosition.Invalid)
                {
                    // was on map before, now no longer on map.
                    context.Map.ActorData[previousPosition.X, previousPosition.Y] = default;
                    v.RemoveComponent<EntityMapPosition>(key);
                    EntityMapPositionChangedMarker.Update(v, key, previousPosition);
                    context.MapCacheControl.MarkDirty(previousPosition);
                    return true;
                }

                if (!context.Map.ActorData[desiredPosition.X, desiredPosition.Y].Empty)
                {
                    // target position is not empty. We would overwrite 
                    // an existing actor.
                    return false;
                }

                if (previousPosition != EntityMapPosition.Invalid)
                {
                    context.Map.ActorData[previousPosition.X, previousPosition.Y] = default;
                    context.MapCacheControl.MarkDirty(previousPosition);
                }

                context.Map.ActorData[desiredPosition.X, desiredPosition.Y] = k;
                EntityMapPositionChangedMarker.Update(v, key, previousPosition);
                v.AssignOrReplace(key, in desiredPosition);
                context.MapCacheControl.MarkDirty(desiredPosition);
                return true;
            }

            return false;
        }
    }
}