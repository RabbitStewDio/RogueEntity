using RogueEntity.Core.Players;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IPlayerSpawnInformationSource
    {
        /// <summary>
        ///   Todo: This is most likely a separate concern of the PlayerSpawning module.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        bool TryCreateInitialLevelRequest(in PlayerTag player, out int level); 
        
    }
}
