using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.Pathfinding;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinder : IGoalFinder, IPathFinderPerformanceView
    {
        readonly List<MovementCostData3D> movementSourceData;
        readonly SingleLevelGoalFinderWorker singleLevelPathFinder;
        readonly Stopwatch sw;

        SingleLevelGoalFinderBuilder currentOwner;
        IGoalFinderTargetEvaluator targetEvaluator;
        bool disposed;
        float searchRadius;

        public SingleLevelGoalFinder()
        {
            movementSourceData = new List<MovementCostData3D>();
            sw = new Stopwatch();
            singleLevelPathFinder = new SingleLevelGoalFinderWorker();
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
            this.singleLevelPathFinder.Reset();
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
                return PathFinderResult.NotFound;
            }

            var searchRadiusInt = (int) Math.Ceiling(Math.Max(0, searchRadius));
            if (searchRadiusInt == 0)
            {
                path = pathBuffer;
                return PathFinderResult.NotFound;
            }

            var searchBounds = Rectangle.WithRadius(source.GridX, source.GridY, searchRadiusInt, searchRadiusInt);
            singleLevelPathFinder.ConfigureActiveLevel(source, searchBounds);
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

            TargetEvaluator.CollectGoals(Position.From(source), searchRadius, heuristics, singleLevelPathFinder);
            sw.Start();
            try
            {
                path = pathBuffer;
                return singleLevelPathFinder.PerformSearch(source, pathBuffer);
            }
            finally
            {
                sw.Stop();
                TimeElapsed = sw.Elapsed;
            }
        }

        public IGoalFinderTargetEvaluator TargetEvaluator => targetEvaluator;
        public int NodesEvaluated => singleLevelPathFinder.NodesEvaluated;
        public TimeSpan TimeElapsed { get; private set; }

        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             [NotNull] IReadOnlyDynamicDataView3D<float> costs,
                                             [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
        {
            this.movementSourceData.Add(new MovementCostData3D(in costProfile, costs, directions));
        }
    }
}
