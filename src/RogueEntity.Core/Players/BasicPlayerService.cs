using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Positioning;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Players
{
    /// <summary>
    ///    A basic player service that manages a single observer for every player. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BasicPlayerService<TEntity> : IPlayerService, IPlayerLookup<TEntity>
        where TEntity : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<BasicPlayerService<TEntity>>();

        readonly Lazy<ITimeSource> timeSource;
        readonly Dictionary<PlayerTag, PlayerData> playerDataByGuid;
        readonly Dictionary<TEntity, PlayerData> playerDataByEntityKey;
        readonly List<PlayerData> playerDataBuffer;
        readonly List<Guid> observerIdBuffer;

        public BasicPlayerService([NotNull] Lazy<ITimeSource> timeSource)
        {
            this.timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            
            this.playerDataByGuid = new Dictionary<PlayerTag, PlayerData>();
            this.playerDataByEntityKey = new Dictionary<TEntity, PlayerData>();
            
            this.playerDataBuffer = new List<PlayerData>();
            this.observerIdBuffer = new List<Guid>();
        }

        /// <summary>
        ///   Step 1: Find all currently active players.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="o"></param>
        /// <param name="playerTag"></param>
        public void RefreshPlayers(IEntityViewControl<TEntity> v, TEntity k, in PlayerTag playerTag)
        {
            if (playerDataByEntityKey.TryGetValue(k, out var playerData))
            {
                if (playerData.Tag != playerTag)
                {
                    Logger.Warning("Player Entity {PlayerEntity} changed identity; old player tag was {Existing}, new tag is {New}", k, playerData.Tag, playerTag);
                    playerData.Active = false;
                    return;
                }

                playerData.LastActiveFrame = timeSource.Value.FixedStepTime;
            }
            else
            {
                var data = new PlayerData(k, playerTag, true);
                data.LastActiveFrame = timeSource.Value.FixedStepTime;
                data.Active = true;
                playerDataByEntityKey[k] = data;
                playerDataByGuid[playerTag] = data;
            }
        }

        /// <summary>
        ///   Step 2: Remove all currently inactive players.
        /// </summary>
        public void RemoveExpiredPlayerData()
        {
            playerDataBuffer.Clear();
            var currentTime = timeSource.Value.FixedStepTime;

            foreach (var p in playerDataByGuid.Values)
            {
                if (!p.Active || p.LastActiveFrame != currentTime)
                {
                    playerDataBuffer.Add(p);
                }
            }

            foreach (var p in playerDataBuffer)
            {
                playerDataByGuid.Remove(p.Tag);
                playerDataByEntityKey.Remove(p.PlayerEntity);
            }

            playerDataBuffer.Clear();
        }

        /// <summary>
        ///   Step 3: Service method. Updates Observer Entities.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="o"></param>
        /// <param name="pos"></param>
        /// <typeparam name="TPosition"></typeparam>
        /// <typeparam name="TItemId"></typeparam>
        public void RefreshObservers<TItemId, TPosition>(IEntityViewControl<TItemId> v, TItemId k, in PlayerObserverTag o, in TPosition pos)
            where TPosition : IPosition<TPosition>
            where TItemId : IEntityKey
        {
            if (!playerDataByGuid.TryGetValue(o.ControllingPlayer, out var data))
            {
                if (o.CanSurvivePlayer)
                {
                    // eliminate control from this observer.
                    v.RemoveComponent<PlayerObserverTag>(k);
                }
                else
                {
                    v.AssignOrReplace<DestroyedMarker>(k);
                }
                return;
            }

            if (pos.IsInvalid)
            {
                data.Observers.Remove(o.Id);
            }
            else
            {
                var currentTime = timeSource.Value.FixedStepTime;
                data.Observers[o.Id] = (currentTime, new PlayerObserver(o.Id, o.ControllingPlayer, false, Position.From(pos)));
            }
        }

        /// <summary>
        ///   Step 4: Remove all obsolete observer entries. 
        /// </summary>
        public void RemoveObsoleteObservers()
        {
            var currentTime = timeSource.Value.FixedStepTime;
            foreach (var p in playerDataByGuid.Values)
            {
                observerIdBuffer.Clear();
                foreach (var (key, (age, _)) in p.Observers)
                {
                    if (age != currentTime)
                    {
                        observerIdBuffer.Add(key);
                    }
                }

                foreach (var o in observerIdBuffer)
                {
                    p.Observers.Remove(o);
                }
            }
        }

        public bool TryQueryPlayer(in PlayerTag playerTag, out TEntity playerEntity)
        {
            if (playerDataByGuid.TryGetValue(playerTag, out var p))
            {
                playerEntity = p.PlayerEntity;
                return true;
            }

            playerEntity = default;
            return false;
        }

        public BufferList<PlayerObserver> QueryObservers(PlayerTag player, BufferList<PlayerObserver> queryBuffer = null)
        {
            queryBuffer = BufferList.PrepareBuffer(queryBuffer);

            if (!playerDataByGuid.TryGetValue(player, out var data))
            {
                return queryBuffer;
            }

            foreach (var (_, o) in data.Observers.Values)
            {
                queryBuffer.Add(o);
            }

            return queryBuffer;
        }

        public bool TryQueryPrimaryObserver(PlayerTag player, out PlayerObserver result)
        {
            result = default;
            if (!playerDataByGuid.TryGetValue(player, out var data))
            {
                return false;
            }


            var resultFlag = false;
            foreach (var (_, observer) in data.Observers.Values)
            {
                if (observer.Primary)
                {
                    result = observer;
                    return true;
                }
                
                if (!resultFlag)
                {
                    resultFlag = true;
                    result = observer;
                }
            }

            return resultFlag;
        }

        public bool TryRefreshObserver(in PlayerObserver o, out PlayerObserver result)
        {
            if (playerDataByGuid.TryGetValue(o.Player, out var data) &&
                data.Observers.TryGetValue(o.Id, out var v))
            {
                result = v.Item2;
                return true;
            }

            result = default;
            return false;
        }

        class PlayerData
        {
            public TEntity PlayerEntity { get; }
            public bool Active { get; set; }
            public PlayerTag Tag { get; }
            public Dictionary<Guid, (int, PlayerObserver)> Observers { get; }
            public int LastActiveFrame { get; set; }

            public PlayerData(TEntity playerEntity,
                              PlayerTag tag,
                              bool active = false)
            {
                Observers = new Dictionary<Guid, (int, PlayerObserver)>();
                Tag = tag;
                PlayerEntity = playerEntity;
                Active = active;
            }
        }
    }
}
