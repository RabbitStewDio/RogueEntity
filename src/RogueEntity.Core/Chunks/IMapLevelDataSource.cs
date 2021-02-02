using System;

namespace RogueEntity.Core.Chunks
{
    /// <summary>
    ///   Encapsulates the level generator or level activation logic.
    /// </summary>
    public interface IMapLevelDataSource<TJobKey>
    {
        event EventHandler<TJobKey> LevelCreateComplete;
        event EventHandler<TJobKey> LevelWriteBackComplete;
        event EventHandler<TJobKey> LevelUnloadComplete;
        
        public bool TryPrepareRemoveMapLevel(TJobKey z);
        /// <summary>
        ///   Queries whether a level with the given key can exist.
        ///   Use this to limit the generated levels. 
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool CanCreateLevel(TJobKey z);
        
        /// <summary>
        ///   Activates a map layer. If there is a previously created state available,
        ///   load that instead, otherwise do whatever is needed to fill the data.
        ///
        ///   If the level is loaded, the map context will fire the appropriate events.
        /// </summary>
        /// <param name="z"></param>
        /// <returns>false if there is no such level, true if the level is going to be loaded.</returns>
        public bool TryCreateMapLevel(TJobKey z);
        public bool TryWriteBackMapLevel(TJobKey z);
    }

    public interface IMapLevelDataSourceSystem
    {
        public void UnloadChunks();
        public void LoadChunks();
        public void WriteChunks();

        public void Deactivate();
    }
}
