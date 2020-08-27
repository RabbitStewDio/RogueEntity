using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing
{
    public class DiscoveryMapTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                             IItemComponentTrait<TGameContext, TActorId, DiscoveryMapData> 
        where TActorId : IEntityKey
    {
        public string Id => "Actor.Generic.DiscoveryMap";
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out DiscoveryMapData t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in DiscoveryMapData t, out TActorId changedK)
        {
            v.AssignOrReplace(k, in t);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TGameContext context, TActorId k, out TActorId changedItem)
        {
            changedItem = k;
            entityRegistry.RemoveComponent<DiscoveryMapData>(k);
            return true;
        }
    }
}