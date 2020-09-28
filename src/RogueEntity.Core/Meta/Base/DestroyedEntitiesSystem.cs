using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Helpers;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Meta.Base
{
    public class DestroyedEntitiesSystem<TEntity>
        where TEntity : IEntityKey
    {
        readonly EntityRegistry<TEntity> entityRegistry;

        public DestroyedEntitiesSystem(EntityRegistry<TEntity> entityRegistry)
        {
            this.entityRegistry = entityRegistry ?? throw new ArgumentNullException(nameof(entityRegistry));
        }

        public void DeleteMarkedEntities<TGameContext>(TGameContext _)
        {
            DestroyedEntitiesSystem.Logger.Debug("HELLO WORLD " + typeof(TEntity));
            var en = entityRegistry.PersistentView<DestroyedMarker>();
            var x = EntityKeyListPool.Reserve(en);
            try
            {
                foreach (var v in x)
                {
                    DestroyedEntitiesSystem.Logger.Debug("Destroyed {item}", v);
                    entityRegistry.Destroy(v);
                }
            }
            finally
            {
                EntityKeyListPool.Release(x);
            }
        }
    }

    public static class DestroyedEntitiesSystem
    {
        internal static readonly ILogger Logger = SLog.ForContext(typeof(DestroyedEntitiesSystem));
        
        public static void SchedulePreviouslyMarkedItemsForDestruction<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                                              TGameContext _,
                                                                                              TItemId k,
                                                                                              in CascadingDestroyedMarker m)
            where TItemId : IEntityKey
        {
            Logger.Debug("Activate destruction for inventory item {inventoryItem}", k);
            v.AssignOrReplace(k, new DestroyedMarker());
            v.RemoveComponent<CascadingDestroyedMarker>(k);
        }

        public static void MarkDestroyedContainerEntities<TEntity, TGameContext, TItemId, TInventory>(IEntityViewControl<TEntity> v,
                                                                                                      TGameContext context,
                                                                                                      TEntity k,
                                                                                                      in DestroyedMarker m,
                                                                                                      in TInventory inventory)
            where TEntity : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>
            where TItemId : IEntityKey
            where TInventory : IContainerView<TItemId>
        {
            Logger.Debug("Schedule destruction for contents of container {container} - " + typeof(TEntity) , k);
            foreach (var inventoryItem in inventory.Items)
            {
                Logger.Debug("Schedule destruction for inventory item {inventoryItem}", inventoryItem);
                context.ItemResolver.DestroyNext(inventoryItem);
            }
        }
    }
}