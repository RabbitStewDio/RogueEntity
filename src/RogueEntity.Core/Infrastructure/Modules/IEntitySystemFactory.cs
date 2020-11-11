using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public interface IEntitySystemFactory<TGameContext, TEntityId> where TEntityId : IEntityKey
    {
        void Register(in ModuleInitializationParameter initParam,
                      IGameLoopSystemRegistration<TGameContext> game, 
                      EntityRegistry<TEntityId> entityRegistry,
                      ICommandHandlerRegistration<TGameContext, TEntityId> commandRegistration);
    }
}