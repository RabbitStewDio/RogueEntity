using EnTTSharp.Entities;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System.Collections.Generic;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public class BoxPusherWinConditionSystems
    {
        readonly HashSet<GridPosition2D> targetSpots;
        readonly HashSet<GridPosition2D> boxPositions;

        EntityGridPosition playerPosition;

        public BoxPusherWinConditionSystems()
        {
            targetSpots = new HashSet<GridPosition2D>();
            boxPositions = new HashSet<GridPosition2D>();
        }

        public void StartCheckWinCondition()
        {
            targetSpots.Clear();
            boxPositions.Clear();
            playerPosition = default;
        }

        public void FindPlayer<TActorId>(IEntityViewControl<TActorId> actors, TActorId k, in EntityGridPosition pos, in PlayerTag playerTag, in BoxPusherPlayerProfile levelStats)
            where TActorId : struct, IEntityKey
        {
            playerPosition = pos;
        }

        public void CollectTargetSpots<TItemId>(IEntityViewControl<TItemId> items, TItemId k, in EntityGridPosition pos, in BoxPusherTargetFieldMarker targetMarker)
            where TItemId : struct, IEntityKey
        {
            if (playerPosition.IsInvalid)
            {
                return;
            }

            if (pos.IsInvalid || pos.GridZ != playerPosition.GridZ)
            {
                return;
            }

            targetSpots.Add(pos.ToGridXY());
        }

        public void CollectBoxPositions<TItemId>(IEntityViewControl<TItemId> items, TItemId k, in EntityGridPosition pos, in BoxPusherBoxMarker targetMarker)
            where TItemId : struct, IEntityKey
        {
            if (playerPosition.IsInvalid)
            {
                return;
            }

            if (pos.IsInvalid || pos.GridZ != playerPosition.GridZ)
            {
                return;
            }

            boxPositions.Add(pos.ToGridXY());
        }

        public void FinishEvaluateWinCondition<TActorId>(IEntityViewControl<TActorId> actors, TActorId k, in EntityGridPosition pos, in PlayerTag playerTag, in BoxPusherPlayerProfile levelStats)
            where TActorId : struct, IEntityKey
        {
            if (pos.IsInvalid)
            {
                return;
            }
            
            if (boxPositions.SetEquals(targetSpots))
            {
                levelStats.RecordLevelComplete(pos.GridZ);
            }
            else
            {
                levelStats.RecordLevelProgress(pos.GridZ);
            }
        }
    }
}
