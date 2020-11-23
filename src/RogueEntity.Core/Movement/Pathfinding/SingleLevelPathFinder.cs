using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding
{
    /// <summary>
    ///    A pathfinder that searches a 2D plane for a potential path from source to target.
    ///    This worker assumes that movement modes have no context-switching costs (so
    ///    starting to fly from walking is not more expensive than continuing to fly).  
    /// </summary>
    public class SingleLevelPathFinder : IPathFinder, IDisposable
    {
        readonly IPathFinderSource creator;
        readonly List<MovementSourceData3D> movementSourceData;
        readonly SingleLevelPathFinderWorker singleLevelPathFinder;
        bool disposed;

        public SingleLevelPathFinder([NotNull] IPathFinderSource creator,
                                     [NotNull] IBoundedDataViewPool<AStarNode> astarNodePool,
                                     [NotNull] IBoundedDataViewPool<IMovementMode> movementModePool)
        {
            this.movementSourceData = new List<MovementSourceData3D>();
            this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            this.singleLevelPathFinder = new SingleLevelPathFinderWorker(astarNodePool, movementModePool);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            creator.Return(this);
        }

        public void Reset()
        {
            this.disposed = false;
            this.singleLevelPathFinder.Reset();
        }

        public PathFinderResult TryFindPath(EntityGridPosition source,
                                            EntityGridPosition target,
                                            out List<(EntityGridPosition, IMovementMode)> path,
                                            List<(EntityGridPosition, IMovementMode)> pathBuffer = null,
                                            int searchLimit = int.MaxValue)
        {
            if (pathBuffer == null)
            {
                pathBuffer = new List<(EntityGridPosition, IMovementMode)>();
            }
            else
            {
                pathBuffer.Clear();
            }

            var dx = target.GridX - source.GridX;
            var dy = target.GridY - source.GridY;
            var dz = target.GridZ - source.GridZ;
            if (dz != 0)
            {
                path = pathBuffer;
                return PathFinderResult.NotFound;
            }

            singleLevelPathFinder.ConfigureActiveLevel(target.GridZ);
            foreach (var m in movementSourceData)
            {
                singleLevelPathFinder.ConfigureMovementProfile(in m.MovementCost, m.Costs, m.Directions);
            }

            singleLevelPathFinder.ConfigureFinished();
            path = pathBuffer;
            return singleLevelPathFinder.FindPath(source, target, pathBuffer, searchLimit);
        }

        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             [NotNull] IReadOnlyDynamicDataView3D<float> costs,
                                             [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
        {
            this.movementSourceData.Add(new MovementSourceData3D(in costProfile, costs, directions));
        }

        readonly struct MovementSourceData3D
        {
            public readonly MovementCost MovementCost;
            public readonly IReadOnlyDynamicDataView3D<float> Costs;
            public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> Directions;

            public MovementSourceData3D(in MovementCost movementCost,
                                        [NotNull] IReadOnlyDynamicDataView3D<float> costs,
                                        [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
            {
                MovementCost = movementCost;
                Costs = costs ?? throw new ArgumentNullException(nameof(costs));
                Directions = directions ?? throw new ArgumentNullException(nameof(directions));
            }
        }
    }
}