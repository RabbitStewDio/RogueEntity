using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;
using System;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;

namespace RogueEntity.Core.Tests.Movement.CostModifier
{
    [TestFixture]
    public class CostMapTest
    {
        const string EmptyRoom = @"
// 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  , ### ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        [Test]
        public void TestInbound()
        {
            var resistanceMap = ParseMap(EmptyRoom, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));
            
            var directionalityMapSystem = new InboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap).Should().BeTrue();

            Assert.NotNull(directionalityMap);
            
            directionalityMap.At(0, 0).Should().Be(DirectionalityInformation.None);
            directionalityMap.At(0, 1).Should().Be(DirectionalityInformation.None);
            directionalityMap.At(1, 0).Should().Be(DirectionalityInformation.None);
            directionalityMap.At(1, 1).Should().Be(DirectionalityInformation.All.WithOut(Direction.UpLeft));
            
            directionalityMap.At(2, 5).Should().Be(DirectionalityInformation.All.WithOut(Direction.DownRight));
        }
        
        [Test]
        public void TestOutbound()
        {
            var resistanceMap = ParseMap(EmptyRoom, out var bounds);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, bounds));
            
            var directionalityMapSystem = new OutboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap).Should().BeTrue();

            Assert.NotNull(directionalityMap);

            directionalityMap.At(0, 0).Should().Be(DirectionalityInformation.None);
            directionalityMap.At(0, 1).Should().Be(DirectionalityInformation.Right | DirectionalityInformation.DownRight);
            directionalityMap.At(1, 0).Should().Be(DirectionalityInformation.Down | DirectionalityInformation.DownRight);
            directionalityMap.At(1, 1).Should().Be(DirectionalityInformation.Right | DirectionalityInformation.Down | DirectionalityInformation.DownRight);
            
            directionalityMap.At(2, 5).Should().Be(DirectionalityInformation.Up | DirectionalityInformation.UpLeft | DirectionalityInformation.UpRight |
                                                   DirectionalityInformation.Left | DirectionalityInformation.DownLeft);
        }
    }
}
