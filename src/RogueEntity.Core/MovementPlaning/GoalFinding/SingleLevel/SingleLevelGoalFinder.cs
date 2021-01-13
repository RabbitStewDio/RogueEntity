using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Pathfinding;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinder : IGoalFinder, IPathFinderPerformanceView
    {
        static readonly ILogger Logger = SLog.ForContext<SingleLevelGoalFinder>();
        
        readonly List<MovementCostData3D> movementSourceData;
        readonly GoalSet goalRecordSet;
        readonly BufferList<GoalRecord> goalRecordBuffer;
        readonly BufferList<GoalRecord> goalFilterBuffer;
        readonly SingleLevelGoalFinderDijkstraWorker singleLevelPathFinderDijkstra;
        readonly Stopwatch sw;

        SingleLevelGoalFinderBuilder currentOwner;
        IGoalFinderTargetSource targetSource;
        IGoalFinderFilter filter;
        bool disposed;
        float searchRadius;

        public SingleLevelGoalFinder()
        {
            goalRecordBuffer = new BufferList<GoalRecord>();
            goalFilterBuffer = new BufferList<GoalRecord>();
            goalRecordSet = new GoalSet();
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
            goalRecordSet.Clear();
            goalRecordBuffer.Clear();
            goalFilterBuffer.Clear();
            movementSourceData.Clear();
            currentOwner = null;
            targetSource = null;
        }

        public void Configure([NotNull] SingleLevelGoalFinderBuilder owner,
                              [NotNull] IGoalFinderTargetSource source,
                              [NotNull] IGoalFinderFilter filterFromBuilder,
                              float searchRadiusParam)
        {
            this.filter = filterFromBuilder;
            this.searchRadius = searchRadiusParam;
            this.disposed = false;
            this.movementSourceData.Clear();
            this.singleLevelPathFinderDijkstra.Reset();
            this.currentOwner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.targetSource = source ?? throw new ArgumentNullException(nameof(source));
        }

        public PathFinderResult TryFindPath<TPosition>(in TPosition source, 
                                                       out BufferList<(TPosition, IMovementMode)> path, 
                                                       BufferList<(TPosition, IMovementMode)> pathBuffer = null, 
                                                       int searchLimit = Int32.MaxValue)
            where TPosition : IPosition<TPosition>
        {
            pathBuffer = BufferList.PrepareBuffer(pathBuffer);
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

            if (!TryApplyGoals(source, heuristics, out var goals))
            {
                path = pathBuffer;
                return PathFinderResult.NotFound;
            }

            goals.CopyTo(goalRecordBuffer);

            foreach (var goal in goalRecordBuffer)
            {
                singleLevelPathFinderDijkstra.AddGoal(goal);
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

        bool TryApplyGoals<TPosition>(TPosition source, DistanceCalculation heuristics, out GoalSet goals)
            where TPosition : IPosition<TPosition>
        {
            var position = Position.From(source);
            goals = TargetSource.CollectGoals(position, searchRadius, heuristics, goalRecordSet);
            if (goals.Count == 0)
            {
                Logger.Verbose("Goal not found: no goals specified");
                return false;
            }

            goals = filter.FilterGoals(position, searchRadius, heuristics, goals);
            return goals.Count > 0;
        }

        public IGoalFinderTargetSource TargetSource => targetSource;
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
