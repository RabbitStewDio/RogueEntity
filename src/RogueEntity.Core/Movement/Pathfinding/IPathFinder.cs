using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinder
    {
        
    }

    public interface IPathFinderSource
    {
        IPathFinder GetPathFinder(in PathfindingMovementCostFactors movementProfile);
    }
    
    public interface IPathFinderSourceBackend
    {
        void RegisterMovementSource<TMovement>([NotNull] IReadOnlyDynamicDataView3D<float> cost,
                                               [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> direction);
    }

    public class PathFinderSource: IPathFinderSourceBackend
    {
        readonly Dictionary<Type, IReadOnlyDynamicDataView3D<float>> movementCostMaps;
        readonly Dictionary<Type, IReadOnlyDynamicDataView3D<DirectionalityInformation>> movementDirectionMaps;

        public PathFinderSource()
        {
            movementCostMaps = new Dictionary<Type, IReadOnlyDynamicDataView3D<float>>();
            movementDirectionMaps = new Dictionary<Type, IReadOnlyDynamicDataView3D<DirectionalityInformation>>();
        }

        public void RegisterMovementSource<TMovement>(IReadOnlyDynamicDataView3D<float> cost,
                                                      IReadOnlyDynamicDataView3D<DirectionalityInformation> direction)
        {
            movementCostMaps[typeof(TMovement)] = cost ?? throw new ArgumentNullException(nameof(cost));
            movementDirectionMaps[typeof(TMovement)] = direction ?? throw new ArgumentNullException(nameof(direction));
        }

        public IPathFinder GetPathFinder(in PathfindingMovementCostFactors movementProfile)
        {
            var pf = new PathFinder();
            foreach (var m in movementProfile.MovementCosts)
            {
                
            }
            return null;
        }
        
    }

    public class PathFinder: IPathFinder
    {
        
    }
}