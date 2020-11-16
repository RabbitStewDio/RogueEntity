using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules.Helpers;

namespace RogueEntity.Api.Modules
{
    public static class ModuleEntityContextExtensions
    {
        static void EmptyInitializer<TGameContext, TEntityId>(in ModuleInitializationParameter initParameter,
                                                              IGameLoopSystemRegistration<TGameContext> context,
                                                              EntityRegistry<TEntityId> registry)
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