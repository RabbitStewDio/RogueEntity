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
        readonly SingleLevelPathPool singleLevelPathPool;
        readonly List<MovementCostData3D> movementSourceData;
        readonly SingleLevelPathFinderWorker singleLevelPathFinderWorker;
        readonly Stopwatch sw;

        SingleLevelPathFinderBuilder? currentOwner;
        IPathFinderTargetEvaluator? targetEvaluator;
        bool disposed;

        public SingleLevelPathFinder(IBoundedDataViewPool<AStarNode> astarNodePool,
                                     IBoundedDataViewPool<IMovementMode> movementModePool,
                                     SingleLevelPathPool singleLevelPathPool)
        {
            this.singleLevelPathPool = singleLevelPathPool;
            this.movementSourceData = new List<MovementCostData3D>();
            this.singleLevelPathFinderWorker = new SingleLevelPathFinderWorker(astarNodePool, movementModePool);
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

        public IPathFinderTargetEvaluator? TargetEvaluator => targetEvaluator;

        public void Configure(SingleLevelPathFinderBuilder owner)
        {
            this.disposed = false;
            this.movementSourceData.Clear();
            this.singleLevelPathFinderWorker.Reset();
            this.currentOwner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public IPathFinder WithTarget(IPathFinderTargetEvaluator evaluator)
        {
            this.targetEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            return this;
        }

        public void Reset()
        {
            this.disposed = false;
            this.singleLevelPathFinderWorker.Reset();
            this.movementSourceData.Clear();
        }

        public IReadOnlyDynamicDataView2D<AStarNode> ProcessedNodes => singleLevelPathFinderWorker.Nodes;

        public bool TryFindPath<TPosition>(in TPosition source,
                                           out (PathFinderResult resultHint, IPath path, float pathCost) path,
                                           int searchLimit = Int32.MaxValue) where TPosition : IPosition<TPosition>
        {
            if (source.IsInvalid)
            {
                path = default;
                return false;
            }

            singleLevelPathFinderWorker.ConfigureActiveLevel(source.GridZ);
            var heuristics = DistanceCalculation.Manhattan;
            foreach (var m in movementSourceData)
            {
                singleLevelPathFinderWorker.ConfigureMovementProfile(in m);
                if (heuristics.IsOtherMoreAccurate(m.MovementCost.MovementStyle))
                {
                    heuristics = m.MovementCost.MovementStyle;
                }
            }

            singleLevelPathFinderWorker.ConfigureFinished(heuristics.AsAdjacencyRule());

            if (targetEvaluator == null || !targetEvaluator.Initialize(source, heuristics))
            {
                path = default;
                return false;
            }

            sw.Restart();
            try
            {
                var pathBuffer = singleLevelPathPool.Lease();
                var (result, cost) = singleLevelPathFinderWorker.FindPath(source, targetEvaluator, pathBuffer, searchLimit);
                if (result == PathFinderResult.NotFound)
                {
                    pathBuffer.Dispose();
                    path = default;
                    return false;
                }

                path = (result, pathBuffer, cost);
                return true;
            }
            finally
            {
                TimeElapsed = sw.Elapsed;
            }
        }

        public int NodesEvaluated => singleLevelPathFinderWorker.NodesEvaluated;
        public TimeSpan TimeElapsed { get; private set; }

        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             IReadOnlyDynamicDataView3D<float> costs,
                                             IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirections,
                                             IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirections)
        {
            this.movementSourceData.Add(new MovementCostData3D(in costProfile, costs, inboundDirections, outboundDirections));
        }
    }
}