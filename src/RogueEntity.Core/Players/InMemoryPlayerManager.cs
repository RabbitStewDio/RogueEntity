using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System;

namespace RogueEntity.Core.Players
{
    public class InMemoryPlayerManager<TEntity, TProfileData> : IPlayerManager<TEntity, TProfileData>
        where TEntity : IEntityKey
    {
        readonly IPlayerProfileManager<TProfileData> profileManager;
        readonly IItemResolver<TEntity> itemResolver;
        readonly Lazy<IPlayerServiceConfiguration> playerItemId;

        public InMemoryPlayerManager([NotNull] IItemResolver<TEntity> itemResolver,
                                     [NotNull] Lazy<IPlayerServiceConfiguration> playerItemId,
                                     [NotNull] IPlayerProfileManager<TProfileData> profileManager)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.playerItemId = playerItemId ?? throw new ArgumentNullException(nameof(playerItemId));
            this.profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
        }

        public bool TryActivatePlayer(Guid playerId, out PlayerTag playerTag, out TEntity playerEntity, out TProfileData playerProfile)
        {
            if (!profileManager.TryLoadPlayerData(playerId, out playerProfile))
            {
                playerTag = default;
                playerEntity = default;
                return false;
            }
            
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

            playerEntity = itemResolver.Instantiate(playerItemId.Value.PlayerId);
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
