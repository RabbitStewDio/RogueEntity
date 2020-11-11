using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public static class ModuleEntityContextExtensions
    {
        static void EmptyInitializer<TGameContext, TEntityId>(in ModuleInitializationParameter initParameter,
                                                              IGameLoopSystemRegistration<TGameContext> context,
                                                              EntityRegistry<TEntityId> registry,
                                                              ICommandHandlerRegistration<TGameContext, TEntityId> handler)
            where TEntityId : IEntityKey
        {
        }

        public static EntitySystemRegistrationDelegate<TGameContext, TEntityId> Empty<TGameContext, TEntityId>(this IModuleEntityContext<TGameContext, TEntityId> ctx)
            where TEntityId : IEntityKey
        {
            return EmptyInitializer;
        }
    }
}