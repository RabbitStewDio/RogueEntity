using EnTTSharp.Entities;
using RogueEntity.Core.MapLoading.MapRegions;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public interface IFlatLevelRegionRequestHandlerSystem
    {
        /// <summary>
        ///   Invoked when a existing player requests to be moved to a different level.
        ///   May not be appropriate for all game types. Also used when a player is moving
        ///   into a new level by entering a stair case or portal, where the player
        ///   has no control over where the end point of the portal lies. 
        /// </summary>
        void RequestLoadLevelFromChangeLevelCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                             TItemId k,
                                                             in ChangeLevelRequest cmd)
            where TItemId : IEntityKey;

        /// <summary>
        ///   Invoked when a player is moving into a new level by falling or by knowing where
        ///   the end point of a given portal is placed. Useful for stairs that should line
        ///   up across levels or for jumping down a hole in the ground. 
        /// </summary>
        void RequestLoadLevelFromChangePositionCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                                TItemId k,
                                                                in ChangeLevelPositionRequest cmd)
            where TItemId : IEntityKey;

        public void RequestEvictLevelFromRequest<TItemId>(IEntityViewControl<TItemId> v,
                                                          TItemId k,
                                                          in EvictLevelRequest cmd)
            where TItemId : IEntityKey;
    }
}
