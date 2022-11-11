using RogueEntity.Core.Movement;
using System;
using System.Collections.Generic;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.Pathfinding;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Benchmarks
{
    public abstract class PathFinderBenchmarkBase
    {
        readonly string id;
        readonly List<EntityGridPosition> positions;
        readonly MovementDataCollector movementDataCollector;
        SingleLevelPathFinderSource pathfinderSource;
        Rectangle bounds;

        protected PathFinderBenchmarkBase(string id)
        {
            this.id = id;
            positions = new List<EntityGridPosition>();
            movementDataCollector = new MovementDataCollector();
            pathfinderSource = new SingleLevelPathFinderSource(new SingleLevelPathFinderPolicy(), movementDataCollector);
        }

        public virtual void SetUpGlobal()
        {
            var sourceText = PerformanceTestUtils.ReadResource(id);
            var movementCostData = PerformanceTestUtils.ParseMap(sourceText, out bounds);
            // Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            var directionalityMapSystem = new OutboundMovementDirectionalitySystem<WalkingMovement>(movementCostData.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            if (!directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap))
                throw new Exception();

            movementDataCollector.RegisterMovementSource<WalkingMovement>(WalkingMovement.Instance, movementCostData.As3DMap(0), directionalityMap.As3DMap(0), directionalityMap.As3DMap(0));

            var rnd = new Random(10);
            while (positions.Count < 50)
            {
                var startPosition = EntityGridPosition.OfRaw(0, rnd.Next(bounds.Width / 2, bounds.Width), rnd.Next(bounds.Height / 2, bounds.Height));
                if (positions.Contains(startPosition))
                {
                    continue;
                }

                if (movementCostData[startPosition.GridX, startPosition.GridY] > 0.5)
                {
                    positions.Add(startPosition);
                }
            }

            while (positions.Count < 100)
            {
                var targetPosition = EntityGridPosition.OfRaw(0, rnd.Next(bounds.Width / 2), rnd.Next(bounds.Height / 2));
                if (positions.Contains(targetPosition))
                {
                    continue;
                }

                if (movementCostData[targetPosition.GridX, targetPosition.GridY] > 0.5)
                {
                    positions.Add(targetPosition);
                }
            }


            if (movementCostData[0, 9] < 1 ||
                movementCostData[0, 10] < 1 ||
                movementCostData[0, 11] < 1)
            {
                throw new Exception();
            }

            EnsurePathFindingValid();
        }

        void EnsurePathFindingValid()
        {
            using var pfs = pathfinderSource.GetPathFinder();
            using var pf = pfs.Data
                              .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(EntityGridPosition.OfRaw(0, 0, 11)))
                              .Build(new AggregateMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));
            if (!pf.TryFindPath(EntityGridPosition.OfRaw(0, 0, 9), out var x) ||
                x.resultHint != PathFinderResult.Found)
            {
                throw new ArgumentException();
            }
            //Console.WriteLine($" = {result2} + {string.Join(", ", resultPath2.Select(e => e.Item1))}");
        }

        [SuppressMessage("ReSharper", "NotAccessedVariable")]
        public void ValidatePathFinding()
        {
            TimeSpan totalTime = TimeSpan.Zero;
            int nodesEvaluated = 0;

            var rnd = new Random(11);

            for (int i = 0; i < 100; i += 1)
            {
                var startPosition = positions[rnd.Next(0, 50)];
                var targetPosition = positions[rnd.Next(50, 100)];

                using var pfs = pathfinderSource.GetPathFinder();
                using var pf = pfs.Data
                                  .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(targetPosition))
                                  .Build(new AggregateMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));

                if (!pf.TryFindPath(EntityGridPosition.OfRaw(0, 0, 9), out var x) ||
                    x.resultHint != PathFinderResult.Found)
                {
                    continue;
                }
                // Console.WriteLine($"{i} = From {startPosition} to {targetPosition} = {result} + {string.Join(", ", resultPath.Select(e => e.Item1))}");
                if (i == -1)
                {
                    if (pf is SingleLevelPathFinder spf)
                    {
                        var translatedDataView = spf.ProcessedNodes.TranslateBy(startPosition.GridX, startPosition.GridY);
                        if (translatedDataView[startPosition.GridX, startPosition.GridY].State != AStarNode.NodeState.Closed)
                        {
                            throw new Exception();
                        }
                    }

                    // Console.WriteLine(translatedDataView.ExtendToString(bounds, elementSeparator: "", elementStringifier: e => e.State == AStarNode.NodeState.Closed ? "@" : " "));
                }

                if (pf is IPathFinderPerformanceView pv)
                {
                    totalTime += pv.TimeElapsed;
                    nodesEvaluated += pv.NodesEvaluated;
                    // Console.WriteLine($"Performance View: {pv.NodesEvaluated:n0} in {pv.TimeElapsed}");
                }
            }

            // Console.WriteLine($"Performance View: {nodesEvaluated:n0} nodes in {totalTime} sec");
        }
    }
}