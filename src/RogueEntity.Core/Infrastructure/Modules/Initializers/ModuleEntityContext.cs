using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules.Initializers
{
    public class ModuleEntityContext<TGameContext, TEntityId> : IModuleEntityContext<TGameContext, TEntityId>, IModuleInitializationData<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        readonly Dictionary<ItemDeclarationId, IBulkItemDeclaration<TGameContext, TEntityId>> declaredBulkItems;
        readonly Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TGameContext, TEntityId>> declaredReferenceItems;
        
        readonly Dictionary<ItemDeclarationId, IBulkItemDeclaration<TGameContext, TEntityId>> activeBulkItems;
        readonly Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TGameContext, TEntityId>> activeReferenceItems;
        
        readonly List<EntitySystemFactory> systemFactories;
        readonly string moduleId;

        public ModuleEntityContext(string moduleId)
        {
            this.moduleId = moduleId;
            this.declaredBulkItems = new Dictionary<ItemDeclarationId, IBulkItemDeclaration<TGameContext, TEntityId>>();
            this.declaredReferenceItems = new Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TGameContext, TEntityId>>();
            this.activeBulkItems = new Dictionary<ItemDeclarationId, IBulkItemDeclaration<TGameContext, TEntityId>>();
            this.activeReferenceItems = new Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TGameContext, TEntityId>>();
            this.systemFactories = new List<EntitySystemFactory>();
        }

        public IEnumerable<IBulkItemDeclaration<TGameContext, TEntityId>> DeclaredBulkItems
        {
            get { return activeBulkItems.Values; }
        }

        public IEnumerable<IReferenceItemDeclaration<TGameContext, TEntityId>> DeclaredReferenceItems
        {
            get { return activeReferenceItems.Values; }
        }

        public IEnumerable<IEntitySystemDeclaration<TGameContext, TEntityId>> EntitySystems
        {
            get { return systemFactories; }
        }

        public void DeclareTraitRoles<TItemTrait>(params EntityRole[] roles)
        {
        }

        public void DeclareTraitRelations<TItemTrait>(params EntityRelation[] relations)
        {
        }

        public bool TryGetDefinedBulkItem(ItemDeclarationId id, out IBulkItemDeclaration<TGameContext, TEntityId> item)
        {
            return declaredBulkItems.TryGetValue(id, out item);
        }

        public bool TryGetDefinedReferenceItem(ItemDeclarationId id, out IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            return declaredReferenceItems.TryGetValue(id, out item);
        }

        public void DefineReferenceItemTemplate(IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredReferenceItems[item.Id] = item;
        }

        public void DefineBulkItemTemplate(IBulkItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredBulkItems[item.Id] = item;
        }

        public ItemDeclarationId Activate(IBulkItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredBulkItems[item.Id] = item;
            activeBulkItems[item.Id] = item;
            return item.Id;
        }

        public ItemDeclarationId Activate(IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredReferenceItems[item.Id] = item;
            activeReferenceItems[item.Id] = item;
            return item.Id;
        }

        public void Register(EntitySystemId id,
                             int priority,
                             EntityRegistrationDelegate<TEntityId> entityRegistration)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.moduleId,
                Id = id,
                Priority = priority,
                EntityRegistration = entityRegistration,
                EntitySystemRegistration = default,
                InsertionOrder = systemFactories.Count
            });
        }

        public void Register(EntitySystemId id,
                             int priority,
                             EntitySystemRegistrationDelegate<TGameContext, TEntityId> entitySystemRegistration)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.moduleId,
                Id = id,
                Priority = priority,
                EntityRegistration = null,
                EntitySystemRegistration = entitySystemRegistration,
                InsertionOrder = systemFactories.Count
            });
        }

        class EntitySystemFactory : IEntitySystemFactory<TGameContext, TEntityId>, IEntitySystemDeclaration<TGameContext, TEntityId>
        {
            public string DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }

            public EntityRegistrationDelegate<TEntityId> EntityRegistration { get; set; }

            public EntitySystemRegistrationDelegate<TGameContext, TEntityId> EntitySystemRegistration { get; set; }

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