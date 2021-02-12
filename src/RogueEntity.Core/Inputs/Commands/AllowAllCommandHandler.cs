using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Inputs.Commands
{
    public class AllowAllCommandHandler<TActorId, TCommand>: ICommandHandler<TActorId, TCommand>
    {
        public static readonly AllowAllCommandHandler<TActorId, TCommand> Instance = new AllowAllCommandHandler<TActorId, TCommand>();
        
        public bool IsCommandValidForState(TActorId actor, Optional<TCommand> cmd)
        {
            return true;
        }
    }
}
