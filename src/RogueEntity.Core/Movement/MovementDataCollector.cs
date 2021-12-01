using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Movement
{
    /// <summary>
    ///   A shared registry of movement data (directionality of allowed moves and cost of moves). This
    ///   is used by both goal finders and pathfinders.
    /// </summary>
    public class MovementDataCollector: IMovementDataCollector, IMovementDataProvider
    {
        readonly Dictionary<IMovementMode, MovementSourceData> movementCostMaps;

        public MovementDataCollector()
        {
            movementCostMaps = new Dictionary<IMovementMode, MovementSourceData>();
        }

        public void RegisterMovementSource(IMovementMode movementMode,
                                           IReadOnlyDynamicDataView3D<float> cost,
                                           IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirection,
                                           IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirection)
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

            movementCostMaps[movementMode] = new MovementSourceData(cost, inboundDirection, outboundDirection);
        }

        public IReadOnlyDictionary<IMovementMode, MovementSourceData> MovementCosts => movementCostMaps;
    }
    
    public readonly struct MovementSourceData
    {
        public readonly IReadOnlyDynamicDataView3D<float> Costs;
        public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> InboundDirections;
        public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> OutboundDirections;

        public MovementSourceData([NotNull] IReadOnlyDynamicDataView3D<float> costs,
                                  [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirections,
                                  [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirections)
        {
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            InboundDirections = inboundDirections ?? throw new ArgumentNullException(nameof(inboundDirections));
            OutboundDirections = outboundDirections ?? throw new ArgumentNullException(nameof(outboundDirections));
        }
    }

}
