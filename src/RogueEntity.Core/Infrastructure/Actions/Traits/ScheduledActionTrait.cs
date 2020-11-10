using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Actions.Schedule;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class ScheduledActionTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                IItemComponentTrait<TGameContext, TActorId, ScheduledAction<TGameContext, TActorId>> 
        where TActorId : IEntityKey
    {
        public string Id => "Core.Action.ScheduleActionQuery";
        public int Priority => 0;

        public IReferenceItemTrait<TGameContext, TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out ScheduledAction<TGameContext, TActorId> t)
        {
            if (v.GetComponent(k, out t))

            {
                return true;
            }

            t = default;
            return false;

        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in ScheduledAction<TGameContext, TActorId> t, out TActorId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TGameContext context, TActorId k, out TActorId changedItem)
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