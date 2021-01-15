using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Actions.Schedule;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class ScheduledActionTrait<TActorId> : IReferenceItemTrait<TActorId>,
                                                  IItemComponentTrait<TActorId, ScheduledAction<TActorId>>
        where TActorId : IEntityKey
    {
        public ItemTraitId Id => "Core.Action.ScheduleActionQuery";
        public int Priority => 0;

        public IReferenceItemTrait<TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        { }

        public void Apply(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        { }

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, out ScheduledAction<TActorId> t)
        {
            if (v.GetComponent(k, out t))

            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TActorId k, in ScheduledAction<TActorId> t, out TActorId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TActorId k, out TActorId changedItem)
        {
            changedItem = k;
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
