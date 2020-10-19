using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Discovery
{
    public abstract class DiscoveryMapTrait<TGameContext, TActorId, TDiscoveryMapData> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                                         IItemComponentInformationTrait<TGameContext, TActorId, IDiscoveryMap>
        where TActorId : IEntityKey
        where TDiscoveryMapData: IDiscoveryMap
    {
        public string Id => "Actor.Generic.DiscoveryMap";
        public int Priority => 100;

        protected abstract TDiscoveryMapData CreateValue(TGameContext context, TActorId k, IItemDeclaration item);

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, CreateValue(context, k, item));
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IDiscoveryMap t)
        {
            if (v.GetComponent(k, out TDiscoveryMapData data))
            {
                t = data;
                return true;
            }

            t = default;
            return false;
        }
    }
}