using EnTTSharp.Entities;
using RogueEntity.Core.Players;

namespace RogueEntity.Core.MapLoading
{
    public interface IMapRegionSystem
    {
        /// <summary>
        ///   Invoked when a new player has spawned. This uses some built-in default
        ///   to place the player in the first level. 
        /// </summary>
        void RequestLoadLevelFromNewPlayer<TItemId>(IEntityViewControl<TItemId> v,
                                                    TItemId k,
                                                    in PlayerObserverTag player,
                                                    in NewPlayerTag newPlayerTag)
            where TItemId : IEntityKey;

        /// <summary>
        ///   Invoked when a existing player requests to be moved to a different level.
        ///   May not be appropriate for all game types. Also used when a player is moving
        ///   into a new level by entering a stair case or portal, where the player
        ///   has no control over where the end point of the portal lies. 
        /// </summary>
        void RequestLoadLevelFromChangeLevelCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                             TItemId k,
                                                             in PlayerObserverTag player,
                                                             in ChangeLevelCommand cmd)
            where TItemId : IEntityKey;

        /// <summary>
        ///   Invoked when a player is moving into a new level by falling or by knowing where
        ///   the end point of a given portal is placed. Useful for stairs that should line
        ///   up across levels or for jumping down a hole in the ground. 
        /// </summary>
        void RequestLoadLevelFromChangePositionCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                                TItemId k,
                                                                in PlayerObserverTag player,
                                                                in ChangeLevelPositionCommand cmd)
            where TItemId : IEntityKey;

        /// <summary>
        ///    A basic driver function that loads the next requested chunk.
        /// </summary>
        void LoadChunks();
    }
}
