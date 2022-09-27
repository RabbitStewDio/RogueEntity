using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.Base
{
    /// <summary>
    ///    A helper trait that is able to query multiple different ContainerEntityMarker traits regardless
    ///    of their generics. 
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public sealed class ContainerEntityMarkerResolverTrait<TItemId> : IReferenceItemTrait<TItemId>,
                                                                      IItemComponentInformationTrait<TItemId, IContainerEntityMarker>
        where TItemId : struct, IEntityKey
    {
        readonly BufferList<IItemComponentInformationTrait<TItemId, IContainerEntityMarker>> traits;
        public ItemTraitId Id => "Core.Common.ContainerEntityMarkerQuery";
        public int Priority => 0;

        public ContainerEntityMarkerResolverTrait()
        {
            traits = new BufferList<IItemComponentInformationTrait<TItemId, IContainerEntityMarker>>();
        }

        public IReferenceItemTrait<TItemId> CreateInstance()
        {
            return new ContainerEntityMarkerResolverTrait<TItemId>();
        }

        public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
            item.QueryAll(traits);
        }

        public void Apply(IEntityViewControl<TItemId> v,  TItemId k, IItemDeclaration item)
        { }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, [MaybeNullWhen(false)] out IContainerEntityMarker t)
        {
            foreach (var trait in traits)
            {
                if (trait == this)
                {
                    continue;
                }

                if (trait.TryQuery(v, k, out t))
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
