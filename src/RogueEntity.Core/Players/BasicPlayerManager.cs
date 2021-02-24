using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System;

namespace RogueEntity.Core.Players
{
    public class BasicPlayerManager<TEntity> : IPlayerManager<TEntity>
        where TEntity : IEntityKey
    {
        readonly IItemResolver<TEntity> itemResolver;
        readonly Lazy<IPlayerServiceConfiguration> playerItemId;

        public BasicPlayerManager([NotNull] IItemResolver<TEntity> itemResolver,
                                  [NotNull] Lazy<IPlayerServiceConfiguration> playerItemId)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.playerItemId = playerItemId ?? throw new ArgumentNullException(nameof(playerItemId));
        }

        public bool TryActivatePlayer(Guid playerId, out PlayerTag playerTag, out TEntity playerEntity)
        {
            var entities = itemResolver.QueryProvider.QueryById(playerItemId.Value.PlayerId);
            playerTag = new PlayerTag(playerId);
            foreach (var e in entities)
            {
                if (!itemResolver.TryQueryData(e, out PlayerTag maybePlayerTag))
                {
                    continue;
                }

                if (maybePlayerTag.Equals(playerTag))
                {
                    playerEntity = e;
                    return true;
                }
            }

            var tmpPlayerEntity = itemResolver.Instantiate(playerItemId.Value.PlayerId);
            if (!itemResolver.TryUpdateData(tmpPlayerEntity, playerTag, out playerEntity))
            {
                itemResolver.DiscardUnusedItem(tmpPlayerEntity);
                return false;
            }

            return true;
        }

        public bool TryDeactivatePlayer(Guid playerId)
        {
            var entities = itemResolver.QueryProvider.QueryById(playerItemId.Value.PlayerId);
            var playerTag = new PlayerTag(playerId);
            foreach (var e in entities)
            {
                if (!itemResolver.TryQueryData(e, out PlayerTag maybePlayerTag))
                {
                    continue;
                }

                if (maybePlayerTag.Equals(playerTag))
                {
                    itemResolver.Destroy(e);
                    return true;
                }
            }

            return false;
        }
    }
}
