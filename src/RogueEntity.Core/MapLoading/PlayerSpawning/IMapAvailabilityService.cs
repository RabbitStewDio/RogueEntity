using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    public interface IMapAvailabilityService
    {
        /// <summary>
        ///   Tests whether the given position is in a fully loaded map region. This will never
        ///   return true if there had been an error in loading that region.
        /// </summary>
        bool IsLevelPositionAvailable<TPosition>(TPosition p)
            where TPosition : IPosition<TPosition>;

        bool IsLevelReadyForSpawning(int zPosition);
    }
}
