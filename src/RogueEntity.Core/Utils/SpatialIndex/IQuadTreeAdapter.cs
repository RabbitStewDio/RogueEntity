namespace RogueEntity.Core.Utils.SpatialIndex
{
    /// <summary>
    ///   A helper interface that separates the element management from quad-tree assembly.
    ///   This allows us to have one global index of elements and their bounds with multiple
    ///   QuadTrees storing index-references.
    /// </summary>
    public interface IQuadTreeAdapter
    {
        int ElementIndexRange { get; }

        bool TryGetBounds(FreeListIndex index, out BoundingBox boundingBox);

        bool TryGetDebugData(FreeListIndex index, out string data);
    }
}
