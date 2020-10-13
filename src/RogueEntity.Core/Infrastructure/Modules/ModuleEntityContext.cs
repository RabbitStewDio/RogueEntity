using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public static class ModuleEntityContext
    {
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

    public class ModuleEntityContext<TGameContext, TEntityId> : IModuleEntityContext<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        readonly Dictionary<ItemDeclarationId, IBulkItemDeclaration<TGameContext, TEntityId>> declaredBulkItems;
        readonly Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TGameContext, TEntityId>> declaredReferenceItems;
        readonly List<IEntitySystemFactory<TGameContext, TEntityId>> systemFactories;
        readonly string moduleId;

        public ModuleEntityContext(string moduleId)
        {
            this.moduleId = moduleId;
            this.declaredBulkItems = new Dictionary<ItemDeclarationId, IBulkItemDeclaration<TGameContext, TEntityId>>();
            this.declaredReferenceItems = new Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TGameContext, TEntityId>>();
            this.systemFactories = new List<IEntitySystemFactory<TGameContext, TEntityId>>();
        }

        public IEnumerable<IBulkItemDeclaration<TGameContext, TEntityId>> DeclaredBulkItems
        {
            get { return declaredBulkItems.Values; }
        }

        public IEnumerable<IReferenceItemDeclaration<TGameContext, TEntityId>> DeclaredReferenceItems
        {
            get { return declaredReferenceItems.Values; }
        }

        public IEnumerable<IEntitySystemFactory<TGameContext, TEntityId>> EntitySystems
        {
            get { return systemFactories; }
        }

        public ItemDeclarationId Declare(IBulkItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredReferenceItems.Remove(item.Id);
            declaredBulkItems[item.Id] = item;
            return item.Id;
        }

        public ItemDeclarationId Declare(IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredBulkItems.Remove(item.Id);
            declaredReferenceItems[item.Id] = item;
            return item.Id;
        }

        public void Register(EntitySystemId id,
                             int priority,
                             ModuleEntityContext.EntityRegistrationDelegate<TEntityId> entityRegistration)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.moduleId,
                Id = id,
                Priority = priority,
                EntityRegistration = entityRegistration,
                EntitySystemRegistration = default
            });
        }

        public void Register(EntitySystemId id,
                             int priority,
                             ModuleEntityContext.EntitySystemRegistrationDelegate<TGameContext, TEntityId> entitySystemRegistration)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.moduleId,
                Id = id,
                Priority = priority,
                EntityRegistration = null,
                EntitySystemRegistration = entitySystemRegistration
            });
        }

        class EntitySystemFactory : IEntitySystemFactory<TGameContext, TEntityId>
        {
            public string DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }

            public ModuleEntityContext.EntityRegistrationDelegate<TEntityId> EntityRegistration { get; set; }

            public ModuleEntityContext.EntitySystemRegistrationDelegate<TGameContext, TEntityId> EntitySystemRegistration { get; set; }

            public void Register(IServiceResolver serviceResolver,
                                 IGameLoopSystemRegistration<TGameContext> context,
                                 EntityRegistry<TEntityId> entityRegistry,
                                 ICommandHandlerRegistration<TGameContext, TEntityId> commandRegistration)
            {
                EntityRegistration?.Invoke(serviceResolver, entityRegistry);
                EntitySystemRegistration?.Invoke(serviceResolver, context, entityRegistry, commandRegistration);
            }
        }
    }
}