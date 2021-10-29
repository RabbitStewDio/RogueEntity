using EnTTSharp.Entities;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Inputs.Commands
{
    /// <summary>
    ///   A command handler trait that is added to an actor. This trait only accepts a single
    ///   instance of a command (thus does not allow command queues etc).
    /// </summary>
    /// <typeparam name="TActorId"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    public class BasicCommandTrait<TActorId, TCommand> : CommandTraitBase<TActorId, TCommand>
        where TActorId : IEntityKey
    {
        readonly ICommandHandler<TActorId, TCommand> handler;

        public BasicCommandTrait(ICommandHandler<TActorId, TCommand> handler = null,
                                 int priority = 100) : base(priority)
        {
            this.handler = handler ?? AllowAllCommandHandler<TActorId, TCommand>.Instance;
        }

        public override bool IsCommandValidForState(TActorId actor)
        {
            return handler.IsCommandValidForState(actor, Optional.Empty());
        }

        public override bool IsCommandValidForState(TActorId actor, TCommand cmd)
        {
            return handler.IsCommandValidForState(actor, cmd);
        }
    }
}
