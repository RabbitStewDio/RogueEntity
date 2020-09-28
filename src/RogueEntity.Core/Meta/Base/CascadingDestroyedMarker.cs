namespace RogueEntity.Core.Meta.Base
{
    /// <summary>
    ///   A marker indicating that the entity should be marked as destroyed in the next iteration.
    ///   This allows us to unwind recursive structures without blowing the nice and clean structure
    ///   of the on-function-per-task entity system at the cost that it may take more than one
    ///   iteration to clean up all nested elements.
    /// </summary>
    public readonly struct CascadingDestroyedMarker
    {
        
    }
}