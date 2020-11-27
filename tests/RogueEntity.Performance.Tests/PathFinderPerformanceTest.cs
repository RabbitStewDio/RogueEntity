using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Movement.Pathfinding;
using RogueEntity.Core.Movement.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Performance.Tests
{
    public class PathFinderPerformanceTest
    {
        [Test]
        [TestCase("Maze256.txt", 0, 9, 235, 254)]
        public void ValidatePathFinding(string id, int sx, int sy, int tx, int ty)
        {
            var sourceText = PerformanceTestUtils.ReadResource(id);
            var resistanceMap = PerformanceTestUtils.ParseMap(sourceText, out var bounds);
            // Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            var directionalityMapSystem = new MovementResistanceDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap).Should().BeTrue();

            var pfs = new SingleLevelPathFinderSource(new SingleLevelPathfinderPolicy());
            
            pfs.RegisterMovementSource(WalkingMovement.Instance, resistanceMap.As3DMap(0), directionalityMap.As3DMap(0));
            var rnd = new Random(10);

            var positions = new List<EntityGridPosition>();
            while (positions.Count < 50)
            {
                var startPosition = EntityGridPosition.OfRaw(0, rnd.Next(bounds.Width / 2, bounds.Width), rnd.Next(bounds.Height / 2, bounds.Height), 0);
                if (resistanceMap[startPosition.GridX, startPosition.GridY] > 0.5)
                {
                    positions.Add(startPosition);
                }
            }

            while (positions.Count < 100)
            {
                var targetPosition = EntityGridPosition.OfRaw(0, rnd.Next(bounds.Width / 2), rnd.Next(bounds.Height / 2), 0);
                if (resistanceMap[targetPosition.GridX, targetPosition.GridY] > 0.5)
                {
                    positions.Add(targetPosition);
                }
            }

            resistanceMap[0, 9].Should().Be(1);
            resistanceMap[0, 10].Should().Be(1);
            resistanceMap[0, 11].Should().Be(1);

            using (var pf = pfs.GetPathFinder()
                               .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(EntityGridPosition.OfRaw(0, 0, 11)))
                               .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1))))
            {
                var result2 = pf.TryFindPath(EntityGridPosition.OfRaw(0, 0, 9), out var resultPath2);
                Console.WriteLine($" = {result2} + {string.Join(", ", resultPath2.Select(e => e.Item1))}");
            }
            
            for (int i = 0; i < 100; i += 1)
            {
                var startPosition = positions[rnd.Next(0, 50)];
                var targetPosition = positions[rnd.Next(50, 100)];

                using (var pf = pfs.GetPathFinder()
                                   .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(targetPosition))
                                   .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1))))
                {
                    var result = pf.TryFindPath(startPosition, out var resultPath);
                    Console.WriteLine($"From {startPosition} to {targetPosition} = {result} + {string.Join(", ", resultPath.Select(e => e.Item1))}");
                    if (i == 0)
                    {
                        var spf = pf as SingleLevelPathFinder;
                        var translatedDataView = spf.ProcessedNodes.TranslateBy(startPosition.GridX, startPosition.GridY);
                        translatedDataView[startPosition.GridX, startPosition.GridY].State.Should().Be(AStarNode.NodeState.Closed);
                        Console.WriteLine(translatedDataView
                                              .ExtendToString(bounds, elementSeparator: "", elementStringifier: e => e.State == AStarNode.NodeState.Closed ? "@" : " "));
                    }
                }
            }
        }
    }
}