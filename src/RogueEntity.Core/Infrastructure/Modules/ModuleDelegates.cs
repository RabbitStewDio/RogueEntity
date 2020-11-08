using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public delegate void ModuleInitializerDelegate<TGameContext>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> context);
    public delegate void ModuleContentInitializerDelegate<TGameContext>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> context);
    public delegate void ModuleEntityRoleInitializerDelegate<TGameContext>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> context, EntityRole role);
    public delegate void ModuleEntityRelationInitializerDelegate<TGameContext>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> context, EntityRelation role);

    public delegate void EntityRegistrationDelegate<TEntityId>(IServiceResolver resolver,
                                                               EntityRegistry<TEntityId> registry)
        where TEntityId : IEntityKey;

    public delegate void EntitySystemRegistrationDelegate<TGameContext, TEntityId>(IServiceResolver resolver,
                                                                                   IGameLoopSystemRegistration<TGameContext> context,
                                                                                   EntityRegistry<TEntityId> registry,
                                                                                   ICommandHandlerRegistration<TGameContext, TEntityId> handler)
        where TEntityId : IEntityKey;

    public delegate void GlobalSystemRegistrationDelegate<TGameContext>(IServiceResolver resolver,
                                                                        IGameLoopSystemRegistration<TGameContext> context);

}