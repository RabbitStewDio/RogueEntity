using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.Pathfinding;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System.Collections.Generic;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;
using Assert = NUnit.Framework.Assert;

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
 ### ,  .  ,  8  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  7  ,  .  , ### ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  6  , ### ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  5  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  4  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  3  ,  2  ,  1  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";

        readonly ILogger logger = SLog.ForContext<SingleLevelPathFinderTest>();

        readonly Dictionary<string, (string sourceText, string resultText, Position2D source, Position2D target)> testCases = 
            new Dictionary<string, (string sourceText, string resultText, Position2D source, Position2D target)>()
        {
            { nameof(EmptyRoom), (EmptyRoom, EmptyRoomResult, new Position2D(1,1), new Position2D(7,7)) },
            { nameof(DiagonalBlockRoom), (DiagonalBlockRoom, DiagonalRoomResult, new Position2D(1,1), new Position2D(7,7)) },
        };

        [Test]
        [TestCase(nameof(EmptyRoom))]
        [TestCase(nameof(DiagonalBlockRoom))]
        public void ValidatePathFinding(string id)
        {
            var (sourceText, resultText, sourcePos, targetPos) = testCases[id];
            var (sx, sy) = sourcePos;
            var (tx, ty) = targetPos;
            var resistanceMap = ParseMap(sourceText, out var bounds);
            logger.Debug("Using room layout \n{Map}", TestHelpers.PrintMap(resistanceMap, bounds));

            var outboundDirectionalityMapSystem = new OutboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            outboundDirectionalityMapSystem.MarkGloballyDirty();
            outboundDirectionalityMapSystem.Process();
            outboundDirectionalityMapSystem.ResultView.TryGetView(0, out var outboundDirectionalityMap).Should().BeTrue();

            var inboundDirectionalityMapSystem = new InboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            inboundDirectionalityMapSystem.MarkGloballyDirty();
            inboundDirectionalityMapSystem.Process();
            inboundDirectionalityMapSystem.ResultView.TryGetView(0, out var inboundDirectionalityMap).Should().BeTrue();

            Assert.NotNull(inboundDirectionalityMap);
            Assert.NotNull(outboundDirectionalityMap);
            
            var ms = new MovementDataCollector();
            ms.RegisterMovementSource<WalkingMovement>(WalkingMovement.Instance, resistanceMap.As3DMap(0), inboundDirectionalityMap.As3DMap(0), outboundDirectionalityMap.As3DMap(0));
            
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
            logger.Debug("Expected Result:\n{Expected}\nComputed Result:\n{Computed}", 
                         PrintResultMap(expectedResultMap, bounds), 
                         PrintResultMap(producedResultMap, bounds));

            TestHelpers.AssertEquals(producedResultMap, expectedResultMap, bounds, default, PrintResultMap);
        }

    }
}