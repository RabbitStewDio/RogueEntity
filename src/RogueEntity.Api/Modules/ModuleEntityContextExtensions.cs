using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules.Helpers;

namespace RogueEntity.Api.Modules
{
    public static class ModuleEntityContextExtensions
    {
        static void EmptyInitializer<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                IGameLoopSystemRegistration context,
                                                EntityRegistry<TEntityId> registry)
            where TEntityId : struct, IEntityKey
        { }

        public static EntitySystemRegistrationDelegate<TEntityId> Empty<TEntityId>(this IModuleEntityContext<TEntityId> ctx)
            where TEntityId : struct, IEntityKey
        {
            return EmptyInitializer;
        }
    }
}
