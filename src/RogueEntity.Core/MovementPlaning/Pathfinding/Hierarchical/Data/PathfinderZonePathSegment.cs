using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public readonly struct PathfinderZonePathSegment
{
    public readonly Position2D Origin;
    public readonly Position2D Target;
    public readonly float Cost;
    public readonly List<(Direction direction, byte movementType)> TraversalSteps;

    public PathfinderZonePathSegment(Position2D origin, Position2D target, float cost, List<(Direction direction, byte movementType)> traversalSteps)
    {
        this.Origin = origin;
        this.Cost = cost;
        this.TraversalSteps = traversalSteps;
        this.Target = target;
    }

    public void PopulatePath(SingleLevelPath path, int z, MovementModeEncoding encoder)
    {
        path.BeginRecordPath(Origin, z);
        for (var i = 0; i < TraversalSteps.Count; i++)
        {
            var (d, m) = TraversalSteps[i];
            var mode = encoder[m];
            path.RecordStep(d, mode);
        }
    }
}