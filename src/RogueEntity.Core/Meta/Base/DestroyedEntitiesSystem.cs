using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Helpers;

namespace RogueEntity.Core.Meta.Base
{
    public class DestroyedEntitiesSystem<TEntity> where TEntity : IEntityKey
    {
        readonly EntityRegistry<TEntity> entityRegistry;

        public DestroyedEntitiesSystem(EntityRegistry<TEntity> entityRegistry)
        {
            this.entityRegistry = entityRegistry ?? throw new ArgumentNullException(nameof(entityRegistry));
        }

        public void DeleteMarkedEntities<TGameContext>(TGameContext _)
        {
            var en = entityRegistry.PersistentView<DestroyedMarker>();
            var x = EntityKeyListPool.Reserve(en);
            try
            {
                foreach (var v in x)
                {
                    entityRegistry.Destroy(v);
                }
            }
            finally
            {
                EntityKeyListPool.Release(x);
            }
        }
    }
}
