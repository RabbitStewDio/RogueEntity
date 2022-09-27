using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Api.Modules.Helpers
{
    public delegate void ModuleInitializerDelegate(in ModuleInitializationParameter initParameter, IModuleInitializer context);

    public delegate void ModuleContentInitializerDelegate(in ModuleInitializationParameter initParameter, IModuleInitializer context);

    public delegate void ModuleEntityRoleInitializerDelegate<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                                                      IModuleInitializer context,
                                                                                      EntityRole role)
        where TEntityId : struct, IEntityKey;

    public delegate void ModuleEntityRelationInitializerDelegate<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                                                          IModuleInitializer context,
                                                                                          EntityRelation role)
        where TEntityId : struct, IEntityKey;

    public delegate void EntityRegistrationDelegate<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                               EntityRegistry<TEntityId> registry)
        where TEntityId : struct, IEntityKey;

    public delegate void EntitySystemRegistrationDelegate<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                                     IGameLoopSystemRegistration context,
                                                                     EntityRegistry<TEntityId> registry)
        where TEntityId : struct, IEntityKey;

    public delegate void GlobalSystemRegistrationDelegate(in ModuleInitializationParameter initParameter,
                                                          IGameLoopSystemRegistration context);
}
