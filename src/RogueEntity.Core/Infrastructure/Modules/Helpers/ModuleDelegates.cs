using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public delegate void ModuleInitializerDelegate<TGameContext>(in ModuleInitializationParameter initParameter, IModuleInitializer<TGameContext> context);

    public delegate void ModuleContentInitializerDelegate<TGameContext>(in ModuleInitializationParameter initParameter, IModuleInitializer<TGameContext> context);

    public delegate void ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId>(in ModuleEntityInitializationParameter<TGameContext, TEntityId> initParameter,
                                                                                      IModuleInitializer<TGameContext> context,
                                                                                      EntityRole role)
        where TEntityId : IEntityKey;

    public delegate void ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId>(in ModuleEntityInitializationParameter<TGameContext, TEntityId> initParameter,
                                                                                          IModuleInitializer<TGameContext> context,
                                                                                          EntityRelation role)
        where TEntityId : IEntityKey;

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