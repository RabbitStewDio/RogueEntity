using EnTTSharp.Annotations;

namespace RogueEntity.Core.Infrastructure.Meta.Base
{
    /// <summary>
    ///   A tagging component that marks all entities that cease to exist in any given frame.
    ///   Instead of destroying elements when they die, we let them live so that other computations
    ///   during that frame can still safely reference them. We safely kill entities at the end of
    ///   each frame.
    /// </summary>
    [EntityComponent]
    public readonly struct DestroyedMarker
    {
    }
}