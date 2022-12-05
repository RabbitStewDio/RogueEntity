using RogueEntity.Api.Utils;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class OutboundConnectionRecord
{
    public readonly Dictionary<PathfinderRegionEdge, float> outboundConnections;

    public OutboundConnectionRecord()
    {
        outboundConnections = new Dictionary<PathfinderRegionEdge, float>();
    }

    public void AddOutboundEdge(in PathfinderRegionEdge edge)
    {
        outboundConnections[edge] = 0;
    }

    public BufferList<PathfinderRegionEdge> GetEdges(BufferList<PathfinderRegionEdge>? b)
    {
        b = BufferList.PrepareBuffer(b);
        foreach (var (key, _) in outboundConnections)
        {
            b.Add(key);
        }

        return b;
    } 
}