namespace RogueEntity.Core.Movement.Pathing
{
    public enum PathfinderResult
    {
        /// <summary>
        ///  There is no path to the target.
        /// </summary>
        Failed = 0,

        /// <summary>
        ///   You are already within range of the target.
        /// </summary>
        Arrived = 1,

        /// <summary>
        ///   The target has been found further away.
        /// </summary>
        Found = 2
    }
}