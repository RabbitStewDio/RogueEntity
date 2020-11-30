using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Movement.Pathfinding;
using RogueEntity.Core.Movement.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Benchmarks
{
    public abstract class PathFinderBenchmarkBase
    {
        readonly string id;
        readonly List<EntityGridPosition> positions;
        readonly SingleLevelPathFinderSource pathfinderSource;
        Rectangle bounds;

        public PathFinderBenchmarkBase(string id)
        {
            this.id = id;
            positions = new List<EntityGridPosition>();
            pathfinderSource = new SingleLevelPathFinderSource(new SingleLevelPathfinderPolicy());
        }

        public virtual void SetUpGlobal()
        {
            var sourceText = PerformanceTestUtils.ReadResource(id);
            var resistanceMap = PerformanceTestUtils.ParseMap(sourceText, out bounds);
            // Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            var directionalityMapSystem = new MovementResistanceDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            if (!directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap))
                throw new Exception();

            pathfinderSource.RegisterMovementSource(WalkingMovement.Instance, resistanceMap.As3DMap(0), directionalityMap.As3DMap(0));

            var rnd = new Random(10);
            while (positions.Count < 50)
            {
                var startPosition = EntityGridPosition.OfRaw(0, rnd.Next(bounds.Width / 2, bounds.Width), rnd.Next(bounds.Height / 2, bounds.Height));
                if (resistanceMap[startPosition.GridX, startPosition.GridY] > 0.5)
                {
                    positions.Add(startPosition);
                }
            }

            while (positions.Count < 100)
            {
                var targetPosition = EntityGridPosition.OfRaw(0, rnd.Next(bounds.Width / 2), rnd.Next(bounds.Height / 2));
                if (resistanceMap[targetPosition.GridX, targetPosition.GridY] > 0.5)
                {
                    positions.Add(targetPosition);
                }
            }
            
            
            if (resistanceMap[0, 9] < 1 ||
                resistanceMap[0, 10] < 1 ||
                resistanceMap[0, 11] < 1)
            {
                throw new Exception();
            }
        }
        
        public void ValidatePathFinding()
        {

            TimeSpan totalTime = TimeSpan.Zero;
            int nodesEvaluated = 0;

            using (var pf = pathfinderSource.GetPathFinder()
                               .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(EntityGridPosition.OfRaw(0, 0, 11)))
                               .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1))))
            {
                var result2 = pf.TryFindPath(EntityGridPosition.OfRaw(0, 0, 9), out var resultPath2);
                if (pf is IPathFinderPerformanceView pv)
                {
                    totalTime += pv.TimeElapsed;
                    nodesEvaluated += pv.NodesEvaluated;
                }

                //Console.WriteLine($" = {result2} + {string.Join(", ", resultPath2.Select(e => e.Item1))}");
            }

            var rnd = new Random(11);
            
            for (int i = 0; i < 100; i += 1)
            {
                var startPosition = positions[rnd.Next(0, 50)];
                var targetPosition = positions[rnd.Next(50, 100)];

                using (var pf = pathfinderSource.GetPathFinder()
                                   .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(targetPosition))
                                   .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1))))
                {
                    var result = pf.TryFindPath(startPosition, out var resultPath);
                    // Console.WriteLine($"{i} = From {startPosition} to {targetPosition} = {result} + {string.Join(", ", resultPath.Select(e => e.Item1))}");
                    if (i == -1)
                    {
                        var spf = pf as SingleLevelPathFinder;
                        var translatedDataView = spf.ProcessedNodes.TranslateBy(startPosition.GridX, startPosition.GridY);
                        if (translatedDataView[startPosition.GridX, startPosition.GridY].State != AStarNode.NodeState.Closed)
                        {
                            throw new Exception();
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
            }

            Console.WriteLine($"Performance View: {nodesEvaluated:n0} nodes in {totalTime} sec");
        }
    }
}