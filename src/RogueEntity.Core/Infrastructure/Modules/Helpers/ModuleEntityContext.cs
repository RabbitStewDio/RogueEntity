using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public class ModuleEntityContext<TGameContext, TEntityId> : IModuleEntityContext<TGameContext, TEntityId>, 
                                                                IModuleInitializationData<TGameContext, TEntityId>,
                                                                IModuleContentContext<TGameContext, TEntityId>
        where TEntityId : IEntityKey
    {
        readonly Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TGameContext, TEntityId>)> declaredBulkItems;
        readonly Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TGameContext, TEntityId>)> declaredReferenceItems;
        
        readonly Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TGameContext, TEntityId>)> activeBulkItems;
        readonly Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TGameContext, TEntityId>)> activeReferenceItems;
        
        readonly List<EntitySystemFactory> systemFactories;

        public ModuleEntityContext(ModuleId moduleId)
        {
            this.CurrentModuleId = moduleId;
            this.declaredBulkItems = new Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TGameContext, TEntityId>)>();
            this.declaredReferenceItems = new Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TGameContext, TEntityId>)>();
            this.activeBulkItems = new Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TGameContext, TEntityId>)>();
            this.activeReferenceItems = new Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TGameContext, TEntityId>)>();
            this.systemFactories = new List<EntitySystemFactory>();
        }

        public ModuleId CurrentModuleId { get; set; }
        
        public IEnumerable<(ModuleId, IBulkItemDeclaration<TGameContext, TEntityId>)> DeclaredBulkItems
        {
            get { return activeBulkItems.Values; }
        }

        public IEnumerable<(ModuleId, IReferenceItemDeclaration<TGameContext, TEntityId>)> DeclaredReferenceItems
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
            if ( declaredBulkItems.TryGetValue(id, out var raw))
            {
                item = raw.Item2;
                return true;
            }

            item = default;
            return false;
        }

        public bool TryGetDefinedReferenceItem(ItemDeclarationId id, out IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            if (declaredReferenceItems.TryGetValue(id, out var raw))
            {
                item = raw.Item2;
                return true;
            }

            item = default;
            return false;
        }

        public void DefineReferenceItemTemplate(IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredReferenceItems[item.Id] = (CurrentModuleId, item);
        }

        public void DefineBulkItemTemplate(IBulkItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredBulkItems[item.Id] = (CurrentModuleId, item);
        }

        public ItemDeclarationId Activate(IBulkItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredBulkItems[item.Id] = (CurrentModuleId, item);
            activeBulkItems[item.Id] = (CurrentModuleId, item);
            return item.Id;
        }

        public ItemDeclarationId Activate(IReferenceItemDeclaration<TGameContext, TEntityId> item)
        {
            declaredReferenceItems[item.Id] = (CurrentModuleId, item);
            activeReferenceItems[item.Id] = (CurrentModuleId, item);
            return item.Id;
        }

        public void Register(EntitySystemId id,
                             int priority,
                             EntityRegistrationDelegate<TEntityId> entityRegistration)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.CurrentModuleId,
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
                DeclaringModule = this.CurrentModuleId,
                Id = id,
                Priority = priority,
                EntityRegistration = null,
                EntitySystemRegistration = entitySystemRegistration,
                InsertionOrder = systemFactories.Count
            });
        }

        class EntitySystemFactory : IEntitySystemDeclaration<TGameContext, TEntityId>
        {
            public ModuleId DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }

            public EntityRegistrationDelegate<TEntityId> EntityRegistration { get; set; }

            public EntitySystemRegistrationDelegate<TGameContext, TEntityId> EntitySystemRegistration { get; set; }
        }
    }
}