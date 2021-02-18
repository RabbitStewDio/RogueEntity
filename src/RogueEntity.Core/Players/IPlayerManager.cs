using System;

namespace RogueEntity.Core.Players
{
    public interface IPlayerManager<TEntity>
    {
        /// <summary>
        ///   Login
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="playerTag"></param>
        /// <param name="playerEntity"></param>
        /// <returns></returns>
        bool TryActivatePlayer(Guid playerId, out PlayerTag playerTag, out TEntity playerEntity);

        /// <summary>
        ///   Logout
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        bool TryDeactivatePlayer(Guid playerId);
    }
}
