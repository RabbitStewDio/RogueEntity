using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class InboundConnectionRecord
{
    public readonly HashSet<PathfinderRegionEdge> inboundEdges;

    public InboundConnectionRecord()
    {
        this.inboundEdges = new HashSet<PathfinderRegionEdge>();
    }

    public void AddInboundEdge(in PathfinderRegionEdge edge)
    {
        inboundEdges.Add(edge);
    }

    public bool RemoveInboundEdge(in PathfinderRegionEdge edge)
    {
        inboundEdges.Remove(edge);
        return inboundEdges.Count == 0;
    }
}