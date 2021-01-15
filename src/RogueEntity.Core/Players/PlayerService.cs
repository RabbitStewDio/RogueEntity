using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Players
{
    /// <summary>
    ///    A basic player service that manages a single observer for every player. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PlayerService<TEntity> : IPlayerService<TEntity>
        where TEntity : IEntityKey
    {
        readonly IItemResolver<TEntity> itemResolver;
        readonly IEntityView<TEntity, PlayerTag> playerEntities;
        readonly IEntityView<TEntity, PlayerObserver> playerObservers;
        readonly ItemDeclarationId playerItemId;
        readonly Dictionary<PlayerTag, PlayerData> playerDataByGuid;
        readonly Dictionary<TEntity, PlayerData> playerDataByEntityKey;

        public PlayerService([NotNull] IItemResolver<TEntity> itemResolver,
                             [NotNull] IEntityView<TEntity, PlayerTag> playerEntities,
                             [NotNull] IEntityView<TEntity, PlayerObserver> playerObservers,
                             ItemDeclarationId playerItemId)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.playerEntities = playerEntities ?? throw new ArgumentNullException(nameof(playerEntities));
            this.playerObservers = playerObservers ?? throw new ArgumentNullException(nameof(playerObservers));

            this.playerItemId = playerItemId;
            this.playerDataByGuid = new Dictionary<PlayerTag, PlayerData>();
            this.playerDataByEntityKey = new Dictionary<TEntity, PlayerData>();

            this.playerEntities.Created += OnPlayerCreated;
            this.playerEntities.Destroyed += OnPlayerDestroyed;
        }

        /// <summary>
        ///   Service method. Updates Observer Entities.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="o"></param>
        /// <param name="pos"></param>
        /// <typeparam name="TPosition"></typeparam>
        public void RefreshObservers<TPosition>(IEntityViewControl<TEntity> v, TEntity k, in PlayerObserver o, in TPosition pos)
            where TPosition : IPosition<TPosition>
        {
            if (!playerDataByGuid.TryGetValue(o.Player, out var data))
            {
                return;
            }

            if (pos.IsInvalid)
            {
                data.Observers.Remove(o.Id);
            }
            else
            {
                data.Observers[o.Id] = o;
            }
        }

        void OnPlayerCreated(object sender, TEntity e)
        {
            if (!playerEntities.GetComponent<PlayerTag>(e, out var playerTag))
            {
                return;
            }

            GetOrCreate(playerTag, true);
        }

        void OnPlayerDestroyed(object sender, TEntity e)
        {
            if (playerDataByEntityKey.TryGetValue(e, out var data))
            {
                PlayerDeactivated?.Invoke(this, new PlayerEventArgs<TEntity>(data.Tag, e));
                playerDataByGuid.Remove(data.Tag);
                playerDataByEntityKey.Remove(data.PlayerEntity);
            }
        }

        public (PlayerTag, TEntity) GetOrCreate(Guid playerId)
        {
            var playerTag = new PlayerTag(playerId);
            var pd = GetOrCreate(playerTag, true);
            return (pd.Tag, pd.PlayerEntity);
        }

        public BufferList<PlayerObserver> QueryObservers(PlayerTag player, BufferList<PlayerObserver> buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);

            if (!playerDataByGuid.TryGetValue(player, out var data))
            {
                return buffer;
            }

            foreach (var o in data.Observers.Values)
            {
                buffer.Add(o);
            }

            return buffer;
        }

        public bool TryQueryPrimaryObserver(PlayerTag player, out PlayerObserver result)
        {
            if (!playerDataByGuid.TryGetValue(player, out var data))
            {
                result = default;
                return false;
            }

            foreach (var observer in data.Observers.Values)
            {
                if (observer.Primary)
                {
                    result = observer;
                    return true;
                }
            }
            
            result = default;
            return false;
        }

        public bool TryRefreshObserver(in PlayerObserver o, out PlayerObserver result)
        {
            if (playerDataByGuid.TryGetValue(o.Player, out var data) &&
                data.Observers.TryGetValue(o.Id, out var v))
            {
                result = v;
                return true;
            }

            result = default;
            return false;
        }

        PlayerData GetOrCreate(PlayerTag playerTag, bool active)
        {
            if (playerDataByGuid.TryGetValue(playerTag, out var pd))
            {
                if (!pd.Active)
                {
                    pd.Active = true;
                    PlayerActivated?.Invoke(this, new PlayerEventArgs<TEntity>(playerTag, pd.PlayerEntity));
                }

                return pd;
            }

            var playerEntity = itemResolver.Instantiate(playerItemId);
            var playerData = new PlayerData(playerEntity, playerTag, active);
            playerDataByGuid[playerTag] = playerData;
            playerDataByEntityKey[playerEntity] = playerData;
            if (active)
            {
                PlayerActivated?.Invoke(this, new PlayerEventArgs<TEntity>(playerTag, playerEntity));
            }

            return playerData;
        }

        class PlayerData
        {
            public TEntity PlayerEntity { get; }
            public bool Active { get; set; }
            public PlayerTag Tag { get; }
            public Dictionary<Guid, PlayerObserver> Observers { get; }

            public PlayerData(TEntity playerEntity,
                              PlayerTag tag,
                              bool active = false)
            {
                Observers = new Dictionary<Guid, PlayerObserver>();
                Tag = tag;
                PlayerEntity = playerEntity;
                Active = active;
            }
        }

        public event EventHandler<PlayerEventArgs<TEntity>> PlayerActivated;
        public event EventHandler<PlayerEventArgs<TEntity>> PlayerDeactivated;
    }
}
