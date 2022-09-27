using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Runtime;
using System;

namespace RogueEntity.Core.Players
{
    /// <summary>
    ///    A basic player service that handles the creation/activation of player entities.
    ///    Each player entity also potentially acts as self-referencing player observer. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BasicPlayerManager<TEntity> : IPlayerManager<TEntity>
        where TEntity : struct, IEntityKey
    {
        readonly IItemResolver<TEntity> itemResolver;
        readonly Lazy<IPlayerServiceConfiguration> playerItemId;
        public event EventHandler<PlayerReference<TEntity>>? PlayerActivated;
        public event EventHandler<PlayerReference<TEntity>>? PlayerDeactivated;

        public BasicPlayerManager(IItemResolver<TEntity> itemResolver,
                                  Lazy<IPlayerServiceConfiguration> playerItemId)
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
                    PlayerActivated?.Invoke(this, new PlayerReference<TEntity>(playerTag, playerEntity));
                    return true;
                }
            }

            var tmpPlayerEntity = itemResolver.Instantiate(playerItemId.Value.PlayerId);
            if (!itemResolver.TryUpdateData(tmpPlayerEntity, playerTag, out playerEntity))
            {
                itemResolver.DiscardUnusedItem(tmpPlayerEntity);
                return false;
            }

            itemResolver.TryUpdateData(tmpPlayerEntity, PlayerObserverTag.CreateFor(playerTag), out _);
            PlayerActivated?.Invoke(this, new PlayerReference<TEntity>(playerTag, playerEntity));
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
                    PlayerDeactivated?.Invoke(this, new PlayerReference<TEntity>(playerTag, e));
                    return true;
                }
            }

            return false;
        }
    }
}
