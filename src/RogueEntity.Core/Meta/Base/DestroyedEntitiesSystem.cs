using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Helpers;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;
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

        public void DeleteMarkedEntities<TGameContext>(TGameContext _)
        {
            Logger.Debug("HELLO WORLD " + typeof(TEntity));
            var en = entityRegistry.PersistentView<DestroyedMarker>();
            var x = EntityKeyListPool.Reserve(en);
            try
            {
                foreach (var v in x)
                {
                    Logger.Debug("Destroyed {item}", v);
                    entityRegistry.Destroy(v);
                }
            }
            finally
            {
                EntityKeyListPool.Release(x);
            }
        }
        
        public static void SchedulePreviouslyMarkedItemsForDestruction<TGameContext>(IEntityViewControl<TEntity> v,
                                                                                     TGameContext _,
                                                                                     TEntity k,
                                                                                     in CascadingDestroyedMarker m)
        {
            Logger.Debug("Activate destruction for inventory item {inventoryItem}", k);
            v.AssignOrReplace(k, new DestroyedMarker());
            v.RemoveComponent<CascadingDestroyedMarker>(k);
        }

    }

    public class DestroyContainerContentsSystem<TGameContext, TContainerEntity, TItemId>
        where TItemId : IEntityKey
        where TContainerEntity : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<DestroyContainerContentsSystem<TGameContext, TContainerEntity, TItemId>>();
        readonly IItemResolver<TGameContext, TItemId> itemContext;

        public DestroyContainerContentsSystem([NotNull] IItemResolver<TGameContext, TItemId> itemContext)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        public void MarkDestroyedContainerEntities<TInventory>(IEntityViewControl<TContainerEntity> v,
                                                               TGameContext context,
                                                               TContainerEntity k,
                                                               in DestroyedMarker m,
                                                               in TInventory inventory)
            where TInventory : IContainerView<TItemId>
        {
            Logger.Debug("Schedule destruction for contents of container {container} - " + typeof(TContainerEntity), k);
            foreach (var inventoryItem in inventory.Items)
            {
                Logger.Debug("Schedule destruction for inventory item {inventoryItem}", inventoryItem);
                itemContext.DestroyNext(inventoryItem);
            }
        }
    }
}