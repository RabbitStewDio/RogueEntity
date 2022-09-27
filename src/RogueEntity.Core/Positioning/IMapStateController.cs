using EnTTSharp;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning
{
    /// <summary>
    ///    a service interface to perform general data operations on all maps.
    /// </summary>
    public interface IMapStateController
    {
        /// <summary>
        ///   Reset the data of the whole map.
        /// </summary>
        void ResetState();

        /// <summary>
        ///   Resets a given level.
        /// </summary>
        /// <param name="z"></param>
        void ResetLevel(int z);

        /// <summary>
        ///  Marks all map layers dirty for the given position.
        /// </summary>
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition<TPosition>;
        
        /// <summary>
        ///  Marks all map layers dirty for the given region.
        /// </summary>
        void MarkRegionDirty(int zPositionFrom, int zPositionTo, Optional<Rectangle> layerArea = default);
    }
}
