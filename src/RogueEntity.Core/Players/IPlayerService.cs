using RogueEntity.Api.Utils;
using System;

namespace RogueEntity.Core.Players
{
    public interface IPlayerService<TEntity>
    {
        /// <summary>
        ///   Returns an ordered list of observers. Returns an empty list if the player
        ///   has no active observers. The first entry in the list will be the primary
        ///   observer that is human controlled and usually centered around the player's
        ///   avatar or focus of attention. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        BufferList<PlayerObserver> QueryObservers(PlayerTag player, BufferList<PlayerObserver> buffer = null);

        bool TryQueryPrimaryObserver(PlayerTag player, out PlayerObserver result);
        
        bool TryRefreshObserver(in PlayerObserver o, out PlayerObserver result);

        /// <summary>
        ///   Fired when a new player has been spawned or an existing inactive player has become
        ///   active (ie after loading a save-game).
        /// </summary>
        event EventHandler<PlayerEventArgs<TEntity>> PlayerActivated;

        /// <summary>
        ///   Fired when a player has become inactive, usually by death or quitting the current game session.
        /// </summary>
        event EventHandler<PlayerEventArgs<TEntity>> PlayerDeactivated;
    }
}
