using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules.Services;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IEntitySystemFactory<TGameContext, TEntityId> where TEntityId : IEntityKey
    {
        void Register(IServiceResolver serviceResolver,
                      IGameLoopSystemRegistration<TGameContext> game, 
                      EntityRegistry<TEntityId> entityRegistry,
                      ICommandHandlerRegistration<TGameContext, TEntityId> commandRegistration);
    }
}