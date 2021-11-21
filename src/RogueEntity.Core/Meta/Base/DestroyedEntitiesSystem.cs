using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Helpers;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Core.Meta.Base
{
    public class DestroyedEntitiesSystem<TEntity>
        where TEntity : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(DestroyedEntitiesSystem<TEntity>));
        readonly EntityRegistry<TEntity> entityRegistry;

        public DestroyedEntitiesSystem(EntityRegistry<TEntity> entityRegistry)
        {
            this.entityRegistry = entityRegistry ?? throw new ArgumentNullException(nameof(entityRegistry));
        }

        public void DeleteMarkedEntities()
        {
            var en = entityRegistry.PersistentView<DestroyedMarker>();
            var x = EntityKeyListPool.Reserve(en);
            try
            {
                foreach (var v in x)
                {
                    if (!entityRegistry.IsValid(v))
                    {
                        Logger.Warning("Invalid key {Key} in persistent view", v);
                    }
                    
                    entityRegistry.Destroy(v);
                }
            }
            finally
            {
                EntityKeyListPool.Release(x);
            }
        }

        public static void SchedulePreviouslyMarkedItemsForDestruction(IEntityViewControl<TEntity> v,
                                                                       TEntity k,
                                                                       in CascadingDestroyedMarker m)
        {
            Logger.Debug("Activate destruction for inventory item {InventoryItem}", k);
            v.AssignOrReplace(k, new DestroyedMarker());
            v.RemoveComponent<CascadingDestroyedMarker>(k);
        }
    }

    public class DestroyContainerContentsSystem<TContainerEntity, TItemId>
        where TItemId : IEntityKey
        where TContainerEntity : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<DestroyContainerContentsSystem<TContainerEntity, TItemId>>();
        readonly IItemResolver<TItemId> itemContext;

        public DestroyContainerContentsSystem([NotNull] IItemResolver<TItemId> itemContext)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        public void MarkDestroyedContainerEntities<TInventory>(IEntityViewControl<TContainerEntity> v,
                                                               TContainerEntity k,
                                                               in DestroyedMarker m,
                                                               in TInventory inventory)
            where TInventory : IContainerView<TItemId>
        {
            Logger.Debug("Schedule destruction for contents of container {Container} of type {ContainerType} ", k, typeof(TContainerEntity));
            foreach (var inventoryItem in inventory.Items)
            {
                Logger.Debug("Schedule destruction for inventory item {InventoryItem}", inventoryItem);
                itemContext.DestroyNext(inventoryItem);
            }
        }
    }
}
