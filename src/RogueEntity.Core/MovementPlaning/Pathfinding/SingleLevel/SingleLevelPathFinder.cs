using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    /// <summary>
    ///    A pathfinder that searches a 2D plane for a potential path from source to target.
    ///    This worker assumes that movement modes have no context-switching costs (so
    ///    starting to fly from walking is not more expensive than continuing to fly).  
    /// </summary>
    public class SingleLevelPathFinder : IPathFinder, IPathFinderPerformanceView
    {
        readonly List<MovementCostData3D> movementSourceData;
        readonly SingleLevelPathFinderWorker singleLevelPathFinder;
        readonly Stopwatch sw;

        SingleLevelPathFinderBuilder? currentOwner;
        IPathFinderTargetEvaluator? targetEvaluator;
        bool disposed;

        public SingleLevelPathFinder(IBoundedDataViewPool<AStarNode> astarNodePool,
                                     IBoundedDataViewPool<IMovementMode> movementModePool)
        {
            this.movementSourceData = new List<MovementCostData3D>();
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
            currentOwner?.Return(this);
            currentOwner = null;
            targetEvaluator = null;
        }

        public IPathFinderTargetEvaluator? TargetEvaluator
        {
            get { return targetEvaluator; }
        }

        public void Configure(SingleLevelPathFinderBuilder owner,
                              IPathFinderTargetEvaluator evaluator)
        {
            this.disposed = false;
            this.movementSourceData.Clear();
            this.singleLevelPathFinder.Reset();
            this.currentOwner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.targetEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public void Reset()
        {
            this.disposed = false;
            this.singleLevelPathFinder.Reset();
            this.movementSourceData.Clear();
        }

        public IReadOnlyDynamicDataView2D<AStarNode> ProcessedNodes => singleLevelPathFinder.Nodes;

        public PathFinderResult TryFindPath<TPosition>(in TPosition source,
                                                       out BufferList<(TPosition, IMovementMode)> path,
                                                       BufferList<(TPosition, IMovementMode)>? pathBuffer = null,
                                                       int searchLimit = Int32.MaxValue)
            where TPosition : IPosition<TPosition>
        {
            pathBuffer = BufferList.PrepareBuffer(pathBuffer);
            if (source.IsInvalid)
            {
                path = pathBuffer;
                return PathFinderResult.NotFound;
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

            if (targetEvaluator == null || !targetEvaluator.Initialize(source, heuristics))
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
                                             IReadOnlyDynamicDataView3D<float> costs,
                                             IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
        {
            this.movementSourceData.Add(new MovementCostData3D(in costProfile, costs, directions));
        }
    }
}
