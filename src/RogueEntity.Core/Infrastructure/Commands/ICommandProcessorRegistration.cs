using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public interface ICommandProcessorRegistration<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        void Register(ICommandHandler<TGameContext, TActorId> p);
        ICommandProcessor<TGameContext, TActorId> Processor { get; }
    }
}