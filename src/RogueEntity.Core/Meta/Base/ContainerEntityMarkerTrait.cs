using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.Base
{
    /// <summary>
    ///   This trait places a marker on whether an item has been placed in an inventory or container.
    ///   This can be used to look up the container that holds this particular item.
    /// </summary>
    public class ContainerEntityMarkerTrait<TItemId, TOwnerId> : IReferenceItemTrait<TItemId>,
                                                                 IItemComponentTrait<TItemId, ContainerEntityMarker<TOwnerId>>,
                                                                 IItemComponentInformationTrait<TItemId, IContainerEntityMarker>
        where TItemId : struct, IEntityKey
    {
        public ItemTraitId Id { get; } = $"Core.Item.ParentContainerMarker[{typeof(TOwnerId)}]";

        public int Priority => 100;

        public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        public IReferenceItemTrait<TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, [MaybeNullWhen(false)] out IContainerEntityMarker t)
        {
            if (TryQuery(v, k, out ContainerEntityMarker<TOwnerId> tt))
            {
                t = tt;
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out ContainerEntityMarker<TOwnerId> t)
        {
            t = default;
            return v.IsValid(k) && v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in ContainerEntityMarker<TOwnerId> t, out TItemId changedK)
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

        public bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedK)
        {
            changedK = k;
            v.RemoveComponent<ContainerEntityMarker<TOwnerId>>(k);
            return true;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ContainedItemRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
