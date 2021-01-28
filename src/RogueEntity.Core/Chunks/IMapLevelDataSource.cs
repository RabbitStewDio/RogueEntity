using System;

namespace RogueEntity.Core.Chunks
{
    /// <summary>
    ///   Encapsulates the level generator or level activation logic.
    /// </summary>
    public interface IMapLevelDataSource
    {
        event EventHandler<int> LevelCreateComplete;
        event EventHandler<int> LevelWriteBackComplete;
        
        public bool TryPrepareRemoveMapLevel(int z);
        
        /// <summary>
        ///   Activates a map layer. If there is a previously created state available,
        ///   load that instead, otherwise do whatever is needed to fill the data.
        ///
        ///   If the level is loaded, the map context will fire the appropriate events.
        /// </summary>
        /// <param name="z"></param>
        /// <returns>false if there is no such level, true if the level is going to be loaded.</returns>
        public bool TryCreateMapLevel(int z);
        public bool TryWriteBackMapLevel(int z);
    }

    public interface IMapLevelDataSourceSystem
    {
        public void UnloadChunks();
        public void LoadChunks();
    }
}
