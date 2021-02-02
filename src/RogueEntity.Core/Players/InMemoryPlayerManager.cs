using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class InMemoryPlayerManager<TEntity, TProfileData> : IPlayerManager<TEntity, TProfileData>
        where TEntity : IEntityKey
    {
        readonly IItemResolver<TEntity> itemResolver;
        readonly Lazy<IPlayerServiceConfiguration> playerItemId;
        readonly Dictionary<Guid, PlayerRecord> records;

        public InMemoryPlayerManager([NotNull] IItemResolver<TEntity> itemResolver, Lazy<IPlayerServiceConfiguration> playerItemId)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.playerItemId = playerItemId;
            this.records = new Dictionary<Guid, PlayerRecord>();
        }

        public bool TryActivatePlayer(Guid playerId, out PlayerTag playerTag, out TEntity playerEntity, out TProfileData playerProfile)
        {
            if (records.TryGetValue(playerId, out var record))
            {
                var entities = itemResolver.QueryProvider.QueryById(playerItemId.Value.PlayerId);
                playerTag = new PlayerTag(record.PlayerId);
                foreach (var e in entities)
                {
                    if (!itemResolver.TryQueryData(e, out PlayerTag maybePlayerTag))
                    {
                        continue;
                    }

                    if (maybePlayerTag.Equals(playerTag))
                    {
                        playerEntity = e;
                        playerProfile = record.ProfileData;
                        return true;
                    }
                }
                
                playerEntity = itemResolver.Instantiate(playerItemId.Value.PlayerId);
                playerProfile = record.ProfileData;
                return true;
            }

            playerProfile = default;
            playerEntity = default;
            playerTag = default;
            return false;
        }

        public bool TryCreatePlayer(in TProfileData profile, out PlayerTag playerTag, out TEntity playerEntity, out TProfileData profileData)
        {
            for (var trials = 0; trials < 100; trials += 1)
            {
                var maybePlayerId = Guid.NewGuid();
                if (!records.ContainsKey(maybePlayerId))
                {
                    playerEntity = itemResolver.Instantiate(playerItemId.Value.PlayerId);
                    playerTag = new PlayerTag(maybePlayerId);
                    records.Add(maybePlayerId, new PlayerRecord(maybePlayerId, profile));
                    profileData = profile;
                    return true;
                }
            }

            profileData = default;
            playerTag = default;
            playerEntity = default;
            return false;
        }

        public bool TryDiscardPlayerState(Guid playerId)
        {
            return records.Remove(playerId);
        }

        public IReadOnlyList<(Guid playerId, TProfileData)> KnownPlayers => records.Values.Select(e => (playerId: e.PlayerId, profileData: e.ProfileData)).ToList();
        
        readonly struct PlayerRecord
        {
            public readonly Guid PlayerId;
            public readonly TProfileData ProfileData;

            public PlayerRecord(Guid playerId, TProfileData profileData)
            {
                this.PlayerId = playerId;
                this.ProfileData = profileData;
            }
        }

    }
}
