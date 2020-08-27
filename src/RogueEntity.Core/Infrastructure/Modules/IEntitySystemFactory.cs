using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IEntitySystemFactory<TGameContext, TEntityId> where TEntityId : IEntityKey
    {
        string DeclaringModule { get; }
        EntitySystemId Id { get; }
        int Priority { get; }
        void Register(IGameLoopSystemRegistration<TGameContext> game, EntityRegistry<TEntityId> entityRegistry,
                      ICommandHandlerRegistration<TGameContext, TEntityId> commandRegistration);
    }
}