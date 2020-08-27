using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public class ModuleEntityContext<TGameContext, TEntityId> : IModuleEntityContext<TGameContext, TEntityId> where TEntityId : IEntityKey
    {
        public delegate void SystemRegistrationDelegate(IGameLoopSystemRegistration<TGameContext> context, 
                                                        EntityRegistry<TEntityId> registry,
                                                        ICommandHandlerRegistration<TGameContext, TEntityId> handler);

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

        public void Register(EntitySystemId id, int priority,
                             Action<EntityRegistry<TEntityId>> entityRegistration, 
                             SystemRegistrationDelegate systemRegistration = null)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.moduleId,
                Id = id,
                Priority = priority,
                EntityRegistration = entityRegistration,
                SystemRegistration = systemRegistration
            });
        }

        public void Register(EntitySystemId id, int priority, SystemRegistrationDelegate systemRegistration)
        {
            systemFactories.Add(new EntitySystemFactory
            {
                DeclaringModule = this.moduleId,
                Id = id,
                Priority = priority,
                EntityRegistration = null,
                SystemRegistration = systemRegistration
            });
        }


        class EntitySystemFactory: IEntitySystemFactory<TGameContext, TEntityId>
        {
            public string DeclaringModule { get; set; }
            public EntitySystemId Id { get; set; }
            public int Priority { get; set; }

            public Action<EntityRegistry<TEntityId>> EntityRegistration { get; set; }

            public SystemRegistrationDelegate SystemRegistration { get; set; }

            public void Register(IGameLoopSystemRegistration<TGameContext> context, EntityRegistry<TEntityId> entityRegistry,
                                 ICommandHandlerRegistration<TGameContext, TEntityId> commandRegistration)
            {
                EntityRegistration?.Invoke(entityRegistry);
                SystemRegistration?.Invoke(context, entityRegistry, commandRegistration);
            }
        }
    }
}