using System;

namespace RogueEntity.Core.Movement.Maps
{
    [Obsolete]
    public interface ICombinedCostMap
    {
        int Height { get; }
        int Width { get; }
        
        /// <summary>
        ///   Caluclates the movement cost when entering a given tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        bool MoveCost(int x, int y, out float cost);
    }
}