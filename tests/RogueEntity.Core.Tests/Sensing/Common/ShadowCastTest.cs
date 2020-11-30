using System;
using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using static RogueEntity.Core.Tests.Sensing.SenseTestHelpers;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class ShadowCastTest
    {
        const string EmptyRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string EmptyRoomResult = @"
   .   ,   .   ,  0.528,  0.877,  1.000,  0.877,  0.528,   .   ,   .   
   .   ,  0.757,  1.394,  1.838,  2.000,  1.838,  1.394,  0.757,   .   
  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528
   .   ,  0.757,  1.394,  1.838,  2.000,  1.838,  1.394,  0.757,   .   
   .   ,   .   ,  0.528,  0.877,  1.000,  0.877,  0.528,   .   ,   .   
";

        const string EmptyRoomDirections = @"
  ~ ,  ~ , ┌# , ┌# , ┬# , ┐# , ┐# ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  , ┐# 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  , ┐# 
 ├# , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  , ┤# 
 └# , └  , └  , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
 └# , └  , └  , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ ,  ~ , └# , └# , ┴# , ┘# , ┘# ,  ~ ,  ~
";

        const string RoomWithPillars = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 1, 1, 0, 0, 0, 1 
1, 0, 0, 0, 0, 1, 1, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 1, 1, 0, 0, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string RoomWithPillarsResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
  0.528,  1.394,  2.172,  2.764,  3.000,   .   ,   .   ,   .   ,   .   
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528
   .   ,   .   ,   .   ,  1.838,  2.000,  1.838,  1.394,   .   ,   .   
   .   ,   .   ,   .   ,  0.877,  1.000,  0.877,  0.528,   .   ,   .   
";

        const string RoomWithPillarsDirection = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
 ┌# , ┌  , ┌  , ┌# , ┬# ,  ~ ,  ~ ,  ~ ,  ~ 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐# , ┐# , ┐  , ┐# 
 ├# , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  , ┤# 
 └# , └  , └  , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
 └# , └  , └# , └# , ┴  , ┘  , ┘# , ┘# , ┘# 
  ~ ,  ~ ,  ~ , └  , ┴  , ┘  , ┘  ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ , └# , ┴# , ┘# , ┘# ,  ~ ,  ~
";

        const string RoomDoorNear = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 0, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 0, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string RoomDoorNearResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.757,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  2.172,   .   ,   .   
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
   .   ,   .   ,   .   ,  2.764,  3.000,  2.764,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.838,  2.000,  1.838,   .   ,   .   ,   .   
   .   ,   .   ,  0.528,  0.877,  1.000,  0.877,  0.528,   .   ,   .   
";

        const string RoomDoorNearDirection = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┐  ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┐  ,  ~ ,  ~ 
 ┌# , ┌# , ┌# , ┌# , ┬# , ┐  , ┐# , ┐# , ┐# 
 ├# , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  , ┤# 
 └# , └# , └# , └# , ┴  , ┘# , ┘# , ┘# , ┘# 
  ~ ,  ~ ,  ~ , └  , ┴  , ┘  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ , └  , ┴  , ┘  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ , └# , └# , ┴# , ┘# , ┘# ,  ~ ,  ~
";

        const string RoomDoorFar = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 0, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 0, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string RoomDoorFarResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.394,   .   ,   .   
  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528
   .   ,   .   ,   .   ,  1.838,  2.000,  1.838,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  0.877,  1.000,  0.877,   .   ,   .   ,   .   
";

        const string RoomDoorFarDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┐  ,  ~ ,  ~ 
 ┌# , ┌# , ┌# , ┌# , ┬# , ┐  , ┐# , ┐# , ┐# 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  , ┐# 
 ├# , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  , ┤# 
 └# , └  , └  , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
 └# , └# , └# , └# , ┴  , ┘# , ┘# , ┘# , ┘# 
  ~ ,  ~ ,  ~ , └  , ┴  , ┘  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ , └# , ┴# , ┘# ,  ~ ,  ~ ,  ~
";
        
        const string PartialBlockedRoom = @"
// 11x11; an empty room
1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0
1.0, 0.0, 0.0, 0.0, 0.1, 0.0, 0.0, 0.0, 1.0 
1.0, 0.0, 0.0, 0.0, 0.1, 0.0, 0.0, 0.0, 1.0 
1.0, 0.0, 0.0, 0.0, 0.1, 0.0, 0.0, 0.0, 1.0 
1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0 
1.0, 0.0, 0.0, 0.0, 0.1, 0.0, 0.0, 0.0, 1.0 
1.0, 0.0, 0.0, 0.5, 0.0, 0.0, 0.0, 0.0, 1.0 
1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0 
1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0
";

        const string PartialBlockedResult = @"
   .   ,   .   ,  0.428,  0.639,  0.729,  0.639,  0.428,   .   ,   .   
   .   ,  0.757,  1.255,  1.489,  1.620,  1.489,  1.255,  0.757,   .   
  0.528,  1.394,  2.172,  2.488,  2.700,  2.488,  2.172,  1.394,  0.528
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.877,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
  0.528,  1.394,  2.172,  2.488,  2.700,  2.488,  2.172,  1.394,  0.528
   .   ,  0.757,  0.628,  1.654,  1.800,  1.654,  1.255,  0.757,   .   
   .   ,   .   ,  0.475,  0.789,  0.900,  0.789,  0.475,   .   ,   .   
";

        const string PartialBlockedDirections = @"
  ~ ,  ~ , ┌# , ┌# , ┬# , ┐# , ┐# ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  , ┐# 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  , ┐# 
 ├# , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  , ┤# 
 └# , └  , └  , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
 └# , └  , └  , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ ,  ~ , └# , └# , ┴# , ┘# , ┘# ,  ~ ,  ~
";

        const string DiagonalWallTestRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 1, 0, 0, 0, 1 
1, 0, 0, 0, 0, 1, 0, 0, 1 
1, 0, 0, 0, 0, 0, 1, 0, 1 
1, 1, 0, 0, ., 0, 0, 0, 1 
1, 1, 1, 0, 0, 0, 0, 0, 1 
1, 0, 1, 1, 0, 0, 1, 1, 1 
1, 0, 0, 1, 1, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string DiagonalWallTestResult = @"
   .   ,   .   ,  0.528,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  0.757,  1.394,  1.838,  2.000,   .   ,   .   ,   .   ,   .   
  0.528,  1.394,  2.172,  2.764,  3.000,  2.764,   .   ,   .   ,   .   
   .   ,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
   .   ,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
   .   ,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.877
   .   ,   .   ,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.528
   .   ,   .   ,   .   ,  1.838,  2.000,  1.838,  1.394,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.528,   .   ,   .   
";

        const string DiagonalWallTestDirections = @"
  ~ ,  ~ , ┌# ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬# ,  ~ ,  ~ ,  ~ ,  ~ 
 ┌# , ┌  , ┌  , ┌  , ┬  , ┐# ,  ~ ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐# , ┐  , ┐# 
  ~ , ├# , ├  , ├  , ┼ *, ┤  , ┤  , ┤  , ┤# 
  ~ , └# , └# , └  , ┴  , ┘  , ┘  , ┘  , ┘# 
  ~ ,  ~ , └# , └# , ┴  , ┘  , ┘# , ┘# , ┘# 
  ~ ,  ~ ,  ~ , └# , ┴# , ┘  , ┘  ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┘# ,  ~ ,  ~
";

        
        [Test]
        [TestCase(nameof(PartialBlockedRoom), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, PartialBlockedRoom, PartialBlockedResult, PartialBlockedDirections)]
        [TestCase(nameof(EmptyRoom), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, EmptyRoom, EmptyRoomResult, EmptyRoomDirections)]
        [TestCase(nameof(RoomWithPillars), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, RoomWithPillars, RoomWithPillarsResult, RoomWithPillarsDirection)]
        [TestCase(nameof(RoomDoorNear), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, RoomDoorNear, RoomDoorNearResult, RoomDoorNearDirection)]
        [TestCase(nameof(RoomDoorFar), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, RoomDoorFar, RoomDoorFarResult, RoomDoorFarDirections)]
        [TestCase(nameof(DiagonalWallTestRoom), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, DiagonalWallTestRoom, DiagonalWallTestResult, DiagonalWallTestDirections)]
        public void ValidateMap(string name, int width, int height, DistanceCalculation dc, AdjacencyRule ar, string resistanceMapText, string brightnessResultText, string directionResultText)
        {
            var radius = width / 2;
            var source = new SenseSourceDefinition(dc, ar,  radius + 1);
            var pos = new Position2D(width / 2, height / 2);

            var resistanceMap = ParseMap(resistanceMapText, out var roomArea);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, roomArea));

            var directionalityMapSystem = new SensoryResistanceDirectionalitySystem<object, VisionSense>(resistanceMap.As3DMap(0));
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap);

            var algo = new ShadowPropagationAlgorithm(LinearDecaySensePhysics.For(DistanceCalculation.Euclid), new ShadowPropagationResistanceDataSource());
            var calculatedBrightnessMap = algo.Calculate(source, source.Intensity, pos, resistanceMap, directionalityMap);
            Console.WriteLine(TestHelpers.PrintMap(calculatedBrightnessMap, new Rectangle(new Position2D(0, 0), radius, radius)));
            Console.WriteLine(TestHelpers.PrintMap(new SenseMapDirectionTestView(calculatedBrightnessMap), new Rectangle(new Position2D(0, 0), radius, radius)));

            var expectedResult = ParseMap(brightnessResultText, out _);
            TestHelpers.AssertEquals(calculatedBrightnessMap, expectedResult, new Rectangle(0, 0, width, height), pos);
            
            var expectedDirections = ParseDirections(directionResultText, out _);
            TestHelpers.AssertEquals(calculatedBrightnessMap, expectedDirections, new Rectangle(0, 0, width, height), pos, PrintSenseDirectionStore);
        }
    }
}