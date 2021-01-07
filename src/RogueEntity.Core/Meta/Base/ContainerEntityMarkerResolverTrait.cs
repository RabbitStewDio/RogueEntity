using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.Base
{
    /// <summary>
    ///    A helper trait that is able to query multiple different ContainerEntityMarker traits regardless
    ///    of their generics. 
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public sealed class ContainerEntityMarkerResolverTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                                    IItemComponentInformationTrait<TGameContext, TItemId, IContainerEntityMarker>
        where TItemId : IEntityKey
    {
        readonly BufferList<IItemComponentInformationTrait<TGameContext, TItemId, IContainerEntityMarker>> traits;
        public ItemTraitId Id => "Core.Common.ContainerEntityMarkerQuery";
        public int Priority => 0;

        public ContainerEntityMarkerResolverTrait()
        {
            traits = new BufferList<IItemComponentInformationTrait<TGameContext, TItemId, IContainerEntityMarker>>();
        }

        public IReferenceItemTrait<TGameContext, TItemId> CreateInstance()
        {
            return new ContainerEntityMarkerResolverTrait<TGameContext, TItemId>();
        }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            item.QueryAll(traits);
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out IContainerEntityMarker t)
        {
            foreach (var trait in traits)
            {
                if (trait == this)
                {
                    continue;
                }

                if (trait.TryQuery(v, context, k, out t))
                {
                    return true;
                }
            }

            t = default;
            return false;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}