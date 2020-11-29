using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Movement.Pathfinding;
using RogueEntity.Core.Movement.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;

namespace RogueEntity.Core.Tests.Movement.Pathfinding
{
    [TestFixture]
    public class SingleLevelPathFinderTest
    {
        const string EmptyRoom = @"
// 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        const string EmptyRoomResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,   6 ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,   5 ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,   4 ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,   3 ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,   2 ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,   1 , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult, 1, 1, 7, 7)]
        public void ValidatePathFinding(string id, string sourceText, string resultText, int sx, int sy, int tx, int ty)
        {
            var resistanceMap = ParseMap(sourceText, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            var directionalityMapSystem = new MovementResistanceDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap).Should().BeTrue();

            var pfs = new SingleLevelPathFinderSource(new SingleLevelPathfinderPolicy());
            pfs.RegisterMovementSource(WalkingMovement.Instance, resistanceMap.As3DMap(0), directionalityMap.As3DMap(0));

            var startPosition = EntityGridPosition.OfRaw(0, sx, sy);
            var targetPosition = EntityGridPosition.OfRaw(0, tx, ty);
            var pf = pfs.GetPathFinder()
                        .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(targetPosition))
                        .Build(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));
            
            var result = pf.TryFindPath(startPosition, out var resultPath);
            result.Should().Be(PathFinderResult.Found);
            resultPath.Should().NotBeEmpty();

            var expectedResultMap = ParseResultMap(resultText, out _);
            var producedResultMap = CreateResult(resistanceMap, resultPath, startPosition, bounds);
            Console.WriteLine("Expected Result\n" + PrintResultMap(expectedResultMap, bounds));
            Console.WriteLine("Computed Result\n" + PrintResultMap(producedResultMap, bounds));

            TestHelpers.AssertEquals(producedResultMap, expectedResultMap, bounds, default, PrintResultMap);
        }

        DynamicDataView2D<(bool, int)> CreateResult(DynamicDataView2D<float> resistanceMap,
                                                    List<(EntityGridPosition, IMovementMode)> resultPath,
                                                    EntityGridPosition startPos,
                                                    Rectangle bounds)
        {
            var resultMap = new DynamicDataView2D<(bool, int)>(resistanceMap.ToConfiguration());

            foreach (var (x, y) in bounds.Contents)
            {
                var wall = resistanceMap[x, y] <= 0;
                var pos = startPos.WithPosition(x, y);
                int pathIndex;
                if (pos == startPos)
                {
                    pathIndex = 0;
                }
                else
                {
                    pathIndex = resultPath.PathIndexOf(pos);
                    if (pathIndex >= 0)
                    {
                        pathIndex += 1;
                    }
                }

                resultMap[x, y] = (wall, pathIndex);
            }

            return resultMap;
        }
    }
}