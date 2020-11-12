using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Services;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct ModuleInitializationParameter
    {
        public readonly IModuleEntityInformation EntityInformation;
        public readonly IServiceResolver ServiceResolver;

        public ModuleInitializationParameter(IModuleEntityInformation entityInformation, IServiceResolver serviceResolver)
        {
            EntityInformation = entityInformation;
            ServiceResolver = serviceResolver;
        }
    }
    
    public delegate void ModuleInitializerDelegate<TGameContext>(in ModuleInitializationParameter initParameter, IModuleInitializer<TGameContext> context);
    public delegate void ModuleContentInitializerDelegate<TGameContext>(in ModuleInitializationParameter initParameter, IModuleInitializer<TGameContext> context);
    public delegate void ModuleEntityRoleInitializerDelegate<TGameContext>(in ModuleInitializationParameter initParameter, IModuleInitializer<TGameContext> context, EntityRole role);
    public delegate void ModuleEntityRelationInitializerDelegate<TGameContext>(in ModuleInitializationParameter initParameter, IModuleInitializer<TGameContext> context, EntityRelation role);

    public delegate void EntityRegistrationDelegate<TEntityId>(in ModuleInitializationParameter initParameter,
                                                               EntityRegistry<TEntityId> registry)
        where TEntityId : IEntityKey;

    public delegate void EntitySystemRegistrationDelegate<TGameContext, TEntityId>(in ModuleInitializationParameter initParameter,
                                                                                   IGameLoopSystemRegistration<TGameContext> context,
                                                                                   EntityRegistry<TEntityId> registry)
        where TEntityId : IEntityKey;

    public delegate void GlobalSystemRegistrationDelegate<TGameContext>(in ModuleInitializationParameter initParameter,
                                                                        IGameLoopSystemRegistration<TGameContext> context);

}