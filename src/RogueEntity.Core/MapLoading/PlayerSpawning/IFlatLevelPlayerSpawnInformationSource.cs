using RogueEntity.Core.Players;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    public interface IFlatLevelPlayerSpawnInformationSource
    {
        /// <summary>
        ///   Tells the player spawn service where new players should be placed.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        bool TryCreateInitialLevelRequest(in PlayerTag player, out int level); 
        
    }
}
