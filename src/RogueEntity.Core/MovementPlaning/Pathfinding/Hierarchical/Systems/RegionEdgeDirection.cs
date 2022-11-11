using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

[Flags]
public enum RegionEdgeDirection
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    West,
    SouthWest,
    NorthWest
}

[Flags]
public enum RegionEdgeState
{
    /// <summary>
    ///    Has been fully processed. 
    /// </summary>
    Clean = 0,
    
    /// <summary>
    ///    Indirectly affected by map changes, requires recalculation of paths within some zones along the northern edge.
    /// </summary>
    EdgeNorth = 1,
    /// <summary>
    ///    Indirectly affected by map changes, requires recalculation of paths within some zones along the eastern edge.
    /// </summary>
    EdgeEast = 2,
    /// <summary>
    ///    Indirectly affected by map changes, requires recalculation of paths within some zones along the southern edge.
    /// </summary>
    EdgeSouth = 4,
    /// <summary>
    ///    Indirectly affected by map changes, requires recalculation of paths within some zones along the western edge.
    /// </summary>
    EdgeWest = 8,
    
    /// <summary>
    ///    Directly affected by map changes, requires recalculation of zones and paths. 
    /// </summary>
    Dirty = 16,
    
    /// <summary>
    ///    Indicates that some path configurations have changed.
    /// </summary>
    PathDirty = 32,
    
    /// <summary>
    ///     Indicates that the zone's source has been removed.  
    /// </summary>
    MarkedForRemove = 64
}