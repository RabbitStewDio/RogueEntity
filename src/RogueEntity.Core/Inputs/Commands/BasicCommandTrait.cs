using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Inputs.Commands
{
    /// <summary>
    ///   A command handler trait that is added to an actor. This trait only accepts a single
    ///   instance of a command (thus does not allow command queues etc).
    /// </summary>
    /// <typeparam name="TActorId"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    public class BasicCommandTrait<TActorId, TCommand> : SimpleReferenceItemComponentTraitBase<TActorId, TCommand>, ICommandTrait<TActorId, TCommand>
        where TActorId : IEntityKey
    {
        readonly ICommandHandler<TActorId, TCommand> handler;

        public BasicCommandTrait(ICommandHandler<TActorId, TCommand> handler = null, 
                                 int priority = 100) : base(CreateDefaultId(), priority)
        {
            this.handler = handler ?? AllowAllCommandHandler<TActorId, TCommand>.Instance;
        }

        static ItemTraitId CreateDefaultId()
        {
            return "Traits.Commands." + typeof(TCommand).FullName;
        }

        public BasicCommandTrait(ItemTraitId id, int priority = 100) : base(id, priority)
        {
        }

        protected override Optional<TCommand> CreateInitialValue(TActorId reference)
        {
            return Optional.Empty();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield break; // todo: Return command receiver roles
        }

        public bool CanHandle<TQueryCommand>()
        {
            return typeof(TCommand) == typeof(TQueryCommand);
        }

        public bool IsCommandValidForState(TActorId actor)
        {
            return handler.IsCommandValidForState(actor, Optional.Empty());
        }

        public bool IsCommandValidForState(TActorId actor, TCommand cmd)
        {
            return handler.IsCommandValidForState(actor, cmd);
        }
    }
}
