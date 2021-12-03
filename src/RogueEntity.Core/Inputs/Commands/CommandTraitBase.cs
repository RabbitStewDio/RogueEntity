using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Inputs.Commands
{
    public abstract class CommandTraitBase<TActorId, TCommand> : SimpleReferenceItemComponentTraitBase<TActorId, TCommand>, ICommandTrait<TActorId, TCommand>
        where TActorId : IEntityKey
    {
        public CommandTypeId CommandId { get; }
        
        protected CommandTraitBase(int priority = 100) : base(CreateDefaultId(), priority)
        {
            CommandId = CommandTypeId.Create<TCommand>();
        }
        
        protected static ItemTraitId CreateDefaultId()
        {
            return "Traits.Commands." + typeof(TCommand).FullName;
        }
        
        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CreateDefaultEntityRoleInstance();
        }

        protected static EntityRoleInstance CreateDefaultEntityRoleInstance()
        {
            return CommandRoles.CreateRoleFor(CommandType.Of<TCommand>()).Instantiate<TActorId>();
        }

        public bool CanHandle<TQueryCommand>()
        {
            return typeof(TCommand) == typeof(TQueryCommand);
        }

        public virtual bool IsCommandValidForState(TActorId actor) => true;
        public abstract bool IsCommandValidForState(TActorId actor, TCommand cmd);

        public virtual bool TryRemoveCompletedCommandData(IItemResolver<TActorId> r, TActorId k)
        {
            if (r.TryQueryData(k, out TCommand cmd))
            {
                return r.TryRemoveData<TCommand>(k, out _);
            }

            return true;
        }
    }
}
