using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.Goals;
using RogueEntity.Core.Movement.Pathfinding;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinder : IGoalFinder, IPathFinderPerformanceView
    {
        static readonly ILogger Logger = SLog.ForContext<SingleLevelGoalFinder>();
        
        readonly List<MovementCostData3D> movementSourceData;
        readonly SingleLevelGoalFinderDijkstraWorker singleLevelPathFinderDijkstra;
        readonly Stopwatch sw;

        SingleLevelGoalFinderBuilder currentOwner;
        IGoalFinderTargetEvaluator targetEvaluator;
        bool disposed;
        float searchRadius;

        public SingleLevelGoalFinder()
        {
            movementSourceData = new List<MovementCostData3D>();
            sw = new Stopwatch();
            singleLevelPathFinderDijkstra = new SingleLevelGoalFinderDijkstraWorker();
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

        public void Configure([NotNull] SingleLevelGoalFinderBuilder owner,
                              [NotNull] IGoalFinderTargetEvaluator evaluator,
                              float searchRadiusParam)
        {
            this.searchRadius = searchRadiusParam;
            this.disposed = false;
            this.movementSourceData.Clear();
            this.singleLevelPathFinderDijkstra.Reset();
            this.currentOwner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.targetEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public PathFinderResult TryFindPath<TPosition>(in TPosition source, 
                                                       out List<(TPosition, IMovementMode)> path, 
                                                       List<(TPosition, IMovementMode)> pathBuffer = null, 
                                                       int searchLimit = Int32.MaxValue)
            where TPosition : IPosition<TPosition>
        {
            if (pathBuffer == null)
            {
                pathBuffer = new List<(TPosition, IMovementMode)>();
            }
            else
            {
                pathBuffer.Clear();
            }

            if (source.IsInvalid)
            {
                path = pathBuffer;
                Logger.Verbose("Goal not found: source is invalid");
                return PathFinderResult.NotFound;
            }

            var searchRadiusInt = (int) Math.Ceiling(Math.Max(0, searchRadius));
            if (searchRadiusInt == 0)
            {
                path = pathBuffer;
                Logger.Verbose("Goal not found: search area is empty");
                return PathFinderResult.NotFound;
            }

            singleLevelPathFinderDijkstra.ConfigureActiveLevel(source, searchRadiusInt);
            var heuristics = DistanceCalculation.Manhattan;
            foreach (var m in movementSourceData)
            {
                singleLevelPathFinderDijkstra.ConfigureMovementProfile(in m.MovementCost, m.Costs, m.Directions);
                if (heuristics.IsOtherMoreAccurate(m.MovementCost.MovementStyle))
                {
                    heuristics = m.MovementCost.MovementStyle;
                }
            }

            singleLevelPathFinderDijkstra.ConfigureFinished(heuristics.AsAdjacencyRule());

            var goals = TargetEvaluator.CollectGoals(Position.From(source), searchRadius, heuristics, singleLevelPathFinderDijkstra);
            if (goals == 0)
            {
                path = pathBuffer;
                Logger.Verbose("Goal not found: no goals specified");
                return PathFinderResult.NotFound;
            }
            
            sw.Reset();
            sw.Start();
            try
            {
                path = pathBuffer;
                return singleLevelPathFinderDijkstra.PerformSearch(source, pathBuffer, searchLimit);
            }
            finally
            {
                sw.Stop();
                TimeElapsed = sw.Elapsed;
            }
        }

        public IGoalFinderTargetEvaluator TargetEvaluator => targetEvaluator;
        public int NodesEvaluated => singleLevelPathFinderDijkstra.NodesEvaluated;
        public TimeSpan TimeElapsed { get; private set; }

        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             [NotNull] IReadOnlyDynamicDataView3D<float> costs,
                                             [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
        {
            this.movementSourceData.Add(new MovementCostData3D(in costProfile, costs, directions));
        }
    }
}
