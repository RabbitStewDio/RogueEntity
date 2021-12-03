namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public interface IFlatLevelRegionLoaderSystem
    {
        /// <summary>
        ///    A basic driver function that loads the next requested chunk.
        /// </summary>
        void LoadChunks();
    }

    public interface IMapRegionEvictionSystem
    {

        /// <summary>
        ///    A basic driver function that loads the next requested chunk.
        /// </summary>
        void EvictChunks();
        
    }
}
