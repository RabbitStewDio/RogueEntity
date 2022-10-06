using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement
{
    /// <summary>
    ///   A shared registry of movement data (directionality of allowed moves and cost of moves). This
    ///   is used by both goal finders and pathfinders.
    /// </summary>
    public class MovementDataCollector : IMovementDataCollector, IMovementDataProvider
    {
        readonly Dictionary<IMovementMode, MovementSourceData> movementCostMaps;
        readonly Dictionary<Type, MovementSourceData> movementCostMapsByType;

        public MovementDataCollector()
        {
            movementCostMaps = new Dictionary<IMovementMode, MovementSourceData>();
            movementCostMapsByType = new Dictionary<Type, MovementSourceData>();
        }

        public void RegisterMovementSource(MovementSourceData d)
        {
            var cost = d.Costs;
            if (cost == null)
            {
                throw new ArgumentNullException(nameof(cost));
            }

            var inboundDirection = d.InboundDirections;
            if (inboundDirection == null)
            {
                throw new ArgumentNullException(nameof(inboundDirection));
            }

            var outboundDirection = d.OutboundDirections;
            if (outboundDirection == null)
            {
                throw new ArgumentNullException(nameof(outboundDirection));
            }

            movementCostMaps[d.MovementMode] = d;
            movementCostMapsByType[d.MovementMode.GetType()] = d;
        }
        
        public void RegisterMovementSource<TMovementMode>(IMovementMode movementMode,
                                                          IReadOnlyDynamicDataView3D<float> cost,
                                                          IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirection,
                                                          IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirection) 
            where TMovementMode : IMovementMode
        {
            if (cost == null)
            {
                throw new ArgumentNullException(nameof(cost));
            }

            if (inboundDirection == null)
            {
                throw new ArgumentNullException(nameof(inboundDirection));
            }

            if (outboundDirection == null)
            {
                throw new ArgumentNullException(nameof(outboundDirection));
            }

            movementCostMaps[movementMode] = new MovementSourceData(movementMode, cost, inboundDirection, outboundDirection);
            movementCostMapsByType[typeof(TMovementMode)] = new MovementSourceData(movementMode, cost, inboundDirection, outboundDirection);
        }

        public bool TryGet<TMovementMode>(out MovementSourceData m)
        {
            return movementCostMapsByType.TryGetValue(typeof(TMovementMode), out m);
        }
        
        public IReadOnlyDictionary<IMovementMode, MovementSourceData> MovementCosts => movementCostMaps;
    }
}