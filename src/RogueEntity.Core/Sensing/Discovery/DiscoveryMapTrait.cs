using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Discovery
{
    public sealed class DiscoveryMapTrait<TActorId> : IReferenceItemTrait<TActorId>,
                                                      IItemComponentInformationTrait<TActorId, IDiscoveryMap>
        where TActorId : struct, IEntityKey
    {
        public ItemTraitId Id => "Actor.Generic.DiscoveryMap";
        public int Priority => 100;

        public IReferenceItemTrait<TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, new DiscoveryMapData(0, 0, 64, 64));
        }

        public void Apply(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        { }

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, [MaybeNullWhen(false)] out IDiscoveryMap t)
        {
            if (v.GetComponent(k, out DiscoveryMapData data))
            {
                t = data;
                return true;
            }

            t = default;
            return false;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return SenseDiscoveryModule.DiscoveryActorRole.Instantiate<TActorId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
