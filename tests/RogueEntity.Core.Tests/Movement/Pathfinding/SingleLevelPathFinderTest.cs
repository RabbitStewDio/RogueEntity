using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.Pathfinding;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
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
        const string DiagonalBlockRoom = @"
 // 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  , ### ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        const string DiagonalRoomResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  8  ,  7  ,  6  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,   . , ### ,  5  ,  .  ,  .  , ### 
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  4  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  3  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  2  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,   1 , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";


        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult, 1, 1, 7, 7)]
        [TestCase(nameof(DiagonalBlockRoom), DiagonalBlockRoom, DiagonalRoomResult, 1, 1, 7, 7)]
        public void ValidatePathFinding(string id, string sourceText, string resultText, int sx, int sy, int tx, int ty)
        {
            var resistanceMap = ParseMap(sourceText, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            var outboundDirectionalityMapSystem = new OutboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            outboundDirectionalityMapSystem.MarkGloballyDirty();
            outboundDirectionalityMapSystem.Process();
            outboundDirectionalityMapSystem.ResultView.TryGetView(0, out var outboundDirectionalityMap).Should().BeTrue();

            var inboundDirectionalityMapSystem = new InboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            inboundDirectionalityMapSystem.MarkGloballyDirty();
            inboundDirectionalityMapSystem.Process();
            inboundDirectionalityMapSystem.ResultView.TryGetView(0, out var inboundDirectionalityMap).Should().BeTrue();

            var ms = new MovementDataCollector();
            ms.RegisterMovementSource(WalkingMovement.Instance, resistanceMap.As3DMap(0), inboundDirectionalityMap.As3DMap(0), outboundDirectionalityMap.As3DMap(0));
            
            var pfs = new SingleLevelPathFinderSource(new SingleLevelPathFinderPolicy(), ms);

            var startPosition = EntityGridPosition.OfRaw(0, sx, sy);
            var targetPosition = EntityGridPosition.OfRaw(0, tx, ty);
            var pf = pfs.GetPathFinder()
                        .WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(targetPosition))
                        .Build(new AggregateMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));

            var result = pf.TryFindPath(startPosition, out var resultPath);
            result.Should().Be(PathFinderResult.Found);
            resultPath.Should().NotBeEmpty();

            var expectedResultMap = ParseResultMap(resultText, out _);
            var producedResultMap = CreateResult(resistanceMap, resultPath, startPosition, bounds);
            Console.WriteLine("Expected Result\n" + PrintResultMap(expectedResultMap, bounds));
            Console.WriteLine("Computed Result\n" + PrintResultMap(producedResultMap, bounds));

            TestHelpers.AssertEquals(producedResultMap, expectedResultMap, bounds, default, PrintResultMap);
        }

    }
}