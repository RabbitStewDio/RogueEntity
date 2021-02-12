using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Inputs.Commands
{
    public interface ICommandHandler<TActorId, TCommand>
    {
        public bool IsCommandValidForState(TActorId actor, Optional<TCommand> cmd);
    }
}
