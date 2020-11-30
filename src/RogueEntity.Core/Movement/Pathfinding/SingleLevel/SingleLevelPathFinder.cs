using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding.SingleLevel
{
    /// <summary>
    ///    A pathfinder that searches a 2D plane for a potential path from source to target.
    ///    This worker assumes that movement modes have no context-switching costs (so
    ///    starting to fly from walking is not more expensive than continuing to fly).  
    /// </summary>
    public class SingleLevelPathFinder : IPathFinder, IPathFinderPerformanceView
    {
        readonly List<MovementSourceData3D> movementSourceData;
        readonly SingleLevelPathFinderWorker singleLevelPathFinder;
        readonly Stopwatch sw;

        SingleLevelPathFinderBuilder currentOwner;
        IPathFinderTargetEvaluator targetEvaluator;
        bool disposed;

        public SingleLevelPathFinder([NotNull] IBoundedDataViewPool<AStarNode> astarNodePool,
                                     [NotNull] IBoundedDataViewPool<IMovementMode> movementModePool)
        {
            this.movementSourceData = new List<MovementSourceData3D>();
            this.singleLevelPathFinder = new SingleLevelPathFinderWorker(astarNodePool, movementModePool);
            this.sw = new Stopwatch();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            currentOwner.Return(this);
            currentOwner = null;
            targetEvaluator = null;
        }

        public IPathFinderTargetEvaluator TargetEvaluator
        {
            get { return targetEvaluator; }
        }

        public void Configure([NotNull] SingleLevelPathFinderBuilder owner,
                              [NotNull] IPathFinderTargetEvaluator evaluator)
        {
            this.disposed = false;
            this.movementSourceData.Clear();
            this.singleLevelPathFinder.Reset();
            this.currentOwner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.targetEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public IPathFinder Build()
        {
            return this;
        }

        public void Reset()
        {
            this.disposed = false;
            this.singleLevelPathFinder.Reset();
            this.movementSourceData.Clear();
        }

        public IReadOnlyDynamicDataView2D<AStarNode> ProcessedNodes => singleLevelPathFinder.Nodes;

        public PathFinderResult TryFindPath(EntityGridPosition source,
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

            singleLevelPathFinder.ConfigureActiveLevel(source.GridZ);
            var heuristics = DistanceCalculation.Manhattan;
            foreach (var m in movementSourceData)
            {
                singleLevelPathFinder.ConfigureMovementProfile(in m.MovementCost, m.Costs, m.Directions);
                if (heuristics.IsOtherMoreAccurate(m.MovementCost.MovementStyle))
                {
                    heuristics = m.MovementCost.MovementStyle;
                }

            }
            singleLevelPathFinder.ConfigureFinished(heuristics.AsAdjacencyRule());

            if (!targetEvaluator.Initialize(source, heuristics))
            {
                path = pathBuffer;
                return PathFinderResult.NotFound;
            }

            path = pathBuffer;
            sw.Restart();
            try
            {
                return singleLevelPathFinder.FindPath(source, targetEvaluator, pathBuffer, searchLimit);
            }
            finally
            {
                TimeElapsed = sw.Elapsed;
            }
        }

        public int NodesEvaluated => singleLevelPathFinder.NodesEvaluated;
        public TimeSpan TimeElapsed { get; private set; }

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