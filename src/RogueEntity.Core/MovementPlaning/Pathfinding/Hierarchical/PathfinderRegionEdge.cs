using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    /// <summary>
    ///    Records a transition between traversable zones across regions. A edge is always recorded along the northern (local y = 0) and
    ///    western edge of a region (local x = 0).
    /// </summary>
    public readonly struct PathfinderRegionEdge : IEquatable<PathfinderRegionEdge>
    {
        public readonly TraversableZoneId OwnerId;
        public readonly EntityGridPosition SourceRegionPosition;
        public readonly EntityGridPosition TargetRegionPosition;
        public readonly float MovementCost;

        public PathfinderRegionEdge(TraversableZoneId ownerId,
                                    EntityGridPosition sourceRegionPosition,
                                    EntityGridPosition targetRegionPosition,
                                    float movementCost)
        {
            this.OwnerId = ownerId;
            this.SourceRegionPosition = sourceRegionPosition;
            this.TargetRegionPosition = targetRegionPosition;
            this.MovementCost = movementCost;
        }

        public bool Equals(PathfinderRegionEdge other)
        {
            return OwnerId.Equals(other.OwnerId) && 
                   SourceRegionPosition.Equals(other.SourceRegionPosition) && 
                   TargetRegionPosition.Equals(other.TargetRegionPosition) && 
                   MovementCost.Equals(other.MovementCost);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = OwnerId.GetHashCode();
                hashCode = (hashCode * 397) ^ SourceRegionPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ TargetRegionPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ MovementCost.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PathfinderRegionEdge other && Equals(other);
        }

        public static bool operator ==(PathfinderRegionEdge left, PathfinderRegionEdge right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PathfinderRegionEdge left, PathfinderRegionEdge right)
        {
            return !left.Equals(right);
        }
    }
}