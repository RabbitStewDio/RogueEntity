using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Discovery
{
    public class DiscoveryMapTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                             IItemComponentInformationTrait<TGameContext, TActorId, IDiscoveryMap>
        where TActorId : IEntityKey
    {
        public string Id => "Actor.Generic.DiscoveryMap";
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, new DiscoveryMapData(0, 0, 64, 64));
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IDiscoveryMap t)
        {
            if (v.GetComponent(k, out DiscoveryMapData data))
            {
                t = data;
                return true;
            }

            t = default;
            return false;
        }
    }
}