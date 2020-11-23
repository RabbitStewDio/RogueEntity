using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Movement.Pathfinding;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.Algorithms;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;

namespace RogueEntity.Core.Tests.Movement.Pathfinding
{
    [TestFixture]
    public class SingleLevelPathFinderTest
    {
        const string EmptyRoom = @"
// 9x9; an empty room
  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  ###
  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  ,  ###  
";

        const string EmptyRoomResult = @"
// 11x11; an empty room
  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000
";

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult, 1, 1, 8, 8)]
        public void ValidatePathFinding(string id, string sourceText, string resultText, int sx, int sy, int tx, int ty)
        {
            var resistanceMap = ParseMap(sourceText, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));

            var directionalityMapSystem = new MovementResistanceDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.TryGetView(0, out var directionalityMap).Should().BeTrue();


            var pfs = new SingleLevelPathFinderSource();
            pfs.RegisterMovementSource(WalkingMovement.Instance, resistanceMap.As3DMap(0), directionalityMap.As3DMap(0));
            var pf = pfs.GetPathFinder(new PathfindingMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));

            var result = pf.TryFindPath(EntityGridPosition.OfRaw(0, sx, sy, 0), EntityGridPosition.OfRaw(0, tx, ty, 0), out var resultPath);
            result.Should().Be(PathFinderResult.Found);
            
            var resultMap = ParseMap(sourceText, out _);
            
            
            resultPath.Should()
                      .Equal(
                          (EntityGridPosition.OfRaw(0, sx, sy, 0), WalkingMovement.Instance),
                          (EntityGridPosition.OfRaw(0, sx, sy, 0), WalkingMovement.Instance)
                      );
        }
    }
}