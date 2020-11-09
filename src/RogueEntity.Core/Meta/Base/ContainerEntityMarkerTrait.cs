using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.Base
{
    /// <summary>
    ///   This trait places a marker on whether an item has been placed in an inventory or container.
    ///   This can be used to look up the container that holds this particular item.
    /// </summary>
    public class ContainerEntityMarkerTrait<TGameContext, TItemId, TOwnerId> :
        IReferenceItemTrait<TGameContext, TItemId>,
        IItemComponentTrait<TGameContext, TItemId, ContainerEntityMarker<TOwnerId>>,
        IItemComponentInformationTrait<TGameContext, TItemId, IContainerEntityMarker>
        where TItemId : IBulkDataStorageKey<TItemId>
    {

        public string Id { get; } = $"Core.Item.ParentContainerMarker[{typeof(TOwnerId)}]";

        public int Priority => 100;

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public IReferenceItemTrait<TGameContext, TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out IContainerEntityMarker t)
        {
            t = default;
            if (TryQuery(v, context, k, out ContainerEntityMarker<TOwnerId> tt))
            {
                t = tt;
                return true;
            }

            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out ContainerEntityMarker<TOwnerId> t)
        {
            t = default;
            return v.IsValid(k) && v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in ContainerEntityMarker<TOwnerId> t, out TItemId changedK)
        {
            changedK = k;
            if (v.GetComponent(k, out ContainerEntityMarker<TOwnerId> _))
            {
                // This entity is already contained in another container. 
                return false;
            }

            v.AssignComponent(k, in t);
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedK)
        {
            changedK = k;
            v.RemoveComponent<ContainerEntityMarker<TOwnerId>>(k);
            return true;
        }
    }
}