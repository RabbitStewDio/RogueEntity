using System.Linq;
using System.Runtime.InteropServices;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public static class ShadowPropagationAlgorithmHelpers
    {
        public static readonly ReadOnlyListWrapper<Direction> DiagonalDirectionsOfNeighbors = AdjacencyRule.Diagonals.DirectionsOfNeighbors().ToList();

        public static bool IsFullyBlocked(float v)
        {
            return v >= 1;
        }

        /// <summary>
        ///  Represents a light propagation direction.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public readonly struct PropagationDirection
        {
            public readonly sbyte xx;
            public readonly sbyte xy;
            public readonly sbyte yx;
            public readonly sbyte yy;

            public PropagationDirection(int xx, int xy, int yx, int yy)
            {
                this.xx = (sbyte)xx;
                this.xy = (sbyte)xy;
                this.yx = (sbyte)yx;
                this.yy = (sbyte)yy;
            }
        }
    }
}