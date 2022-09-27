using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Api.Modules.Helpers
{
    public class ModuleEntityContext<TEntityId> : IModuleEntityContext<TEntityId>,
                                                  IModuleInitializationData<TEntityId>,
                                                  IModuleContentContext<TEntityId>
        where TEntityId : struct, IEntityKey
    {
        readonly Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TEntityId>)> declaredBulkItems;
        readonly Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TEntityId>)> declaredReferenceItems;

        readonly Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TEntityId>)> activeBulkItems;
        readonly Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TEntityId>)> activeReferenceItems;

        readonly List<EntitySystemFactory> systemFactories;

        public ModuleEntityContext(ModuleId moduleId)
        {
            this.CurrentModuleId = moduleId;
            this.declaredBulkItems = new Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TEntityId>)>();
            this.declaredReferenceItems = new Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TEntityId>)>();
            this.activeBulkItems = new Dictionary<ItemDeclarationId, (ModuleId, IBulkItemDeclaration<TEntityId>)>();
            this.activeReferenceItems = new Dictionary<ItemDeclarationId, (ModuleId, IReferenceItemDeclaration<TEntityId>)>();
            this.systemFactories = new List<EntitySystemFactory>();
        }

        public ModuleId CurrentModuleId { get; set; }

        public IEnumerable<(ModuleId, IBulkItemDeclaration<TEntityId>)> DeclaredBulkItems
        {
            get { return activeBulkItems.Values; }
        }

        public IEnumerable<(ModuleId, IReferenceItemDeclaration<TEntityId>)> DeclaredReferenceItems
        {
            get { return activeReferenceItems.Values; }
        }

        public IEnumerable<IEntitySystemDeclaration<TEntityId>> EntitySystems
        {
            get { return systemFactories; }
        }

        public void DeclareTraitRoles<TItemTrait>(params EntityRole[] roles)
        { }

        public void DeclareTraitRelations<TItemTrait>(params EntityRelation[] relations)
        { }

        public bool TryGetDefinedBulkItem(ItemDeclarationId id, [MaybeNullWhen(false)] out IBulkItemDeclaration<TEntityId> item)
        {
            if (declaredBulkItems.TryGetValue(id, out var raw))
            {
                item = raw.Item2;
                return true;
            }

            item = default;
            return false;
        }

        public bool TryGetDefinedReferenceItem(ItemDeclarationId id, [MaybeNullWhen(false)] out IReferenceItemDeclaration<TEntityId> item)
        {
            if (declaredReferenceItems.TryGetValue(id, out var raw))
            {
                item = raw.Item2;
                return true;
            }

            item = default;
            return false;
        }

        public void DefineReferenceItemTemplate(IReferenceItemDeclaration<TEntityId> item)
        {
            declaredReferenceItems[item.Id] = (CurrentModuleId, item);
        }

        public void DefineBulkItemTemplate(IBulkItemDeclaration<TEntityId> item)
        {
            declaredBulkItems[item.Id] = (CurrentModuleId, item);
        }

        public ItemDeclarationId Activate(IBulkItemDeclaration<TEntityId> item)
        {
            declaredBulkItems[item.Id] = (CurrentModuleId, item);
            activeBulkItems[item.Id] = (CurrentModuleId, item);
            return item.Id;
        }

        public ItemDeclarationId Activate(IReferenceItemDeclaration<TEntityId> item)
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
                             EntitySystemRegistrationDelegate<TEntityId>? entitySystemRegistration)
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

        class EntitySystemFactory : IEntitySystemDeclaration<TEntityId>
        {
            public ModuleId DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }
            public int InsertionOrder { get; set; }

            public EntityRegistrationDelegate<TEntityId>? EntityRegistration { get; set; }

            public EntitySystemRegistrationDelegate<TEntityId>? EntitySystemRegistration { get; set; }
        }
    }
}
