using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Players
{
    public interface IPlayerLookup<TEntity>
    {
        BufferList<PlayerTag> QueryPlayers(BufferList<PlayerTag>? queryBuffer = null);
        bool TryQueryPlayer(in PlayerTag playerTag, [MaybeNullWhen(false)] out TEntity playerEntity);
    }
    
    public interface IPlayerService
    {
        /// <summary>
        ///   Returns an ordered list of observers. Returns an empty list if the player
        ///   has no active observers. The first entry in the list will be the primary
        ///   observer that is human controlled and usually centered around the player's
        ///   avatar or focus of attention. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="queryBuffer"></param>
        /// <returns></returns>
        BufferList<PlayerObserver> QueryObservers(PlayerTag player, BufferList<PlayerObserver>? queryBuffer = null);

        bool TryQueryPrimaryObserver(PlayerTag player, out PlayerObserver result);
        
        bool TryRefreshObserver(in PlayerObserver o, out PlayerObserver result);
    }
}
