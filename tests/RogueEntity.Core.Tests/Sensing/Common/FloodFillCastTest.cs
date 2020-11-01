using System;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using static RogueEntity.Core.Tests.Sensing.SenseTestHelpers;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class FloodFillCastTest
    {
        const string EmptyRoom = @"
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

        const string EmptyRoomResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  0.757,  1.172,  1.586,  2.000,  1.586,  1.172,  0.757,   .   
   .   ,  1.172,  2.172,  2.586,  3.000,  2.586,  2.172,  1.172,   .   
   .   ,  1.586,  2.586,  3.586,  4.000,  3.586,  2.586,  1.586,   .   
   .   ,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,   .   
   .   ,  1.586,  2.586,  3.586,  4.000,  3.586,  2.586,  1.586,   .   
   .   ,  1.172,  2.172,  2.586,  3.000,  2.586,  2.172,  1.172,   .   
   .   ,  0.757,  1.172,  1.586,  2.000,  1.586,  1.172,  0.757,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string EmptyRoomResultCheb = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,   .   
   .   ,  2.000,  3.000,  3.000,  3.000,  3.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  4.000,  4.000,  4.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  4.000,  4.000,  4.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  3.000,  3.000,  3.000,  3.000,  2.000,   .   
   .   ,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string EmptyRoomDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        const string EmptyRoomDirectionsCheb = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ , ┌  , ┬  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ├  , ┌  , ┌  , ┬  , ┐  , ┐  , ┤  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , ┴  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
";

        const string RoomWithPillars = @"
//  A room with a long winding corridor. 
//
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,  1.000,  1.000,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,   .   ,   .   ,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPillarsResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  3.172,  2.757,  1.757,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.172,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.586,  5.586,  6.586,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPillarDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┬  , ┐  , ┤  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ┌  , ┬  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        const string RoomWithPartialBlock = @"
//  A room with a long winding corridor. 
//
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,  1.000,  0.500,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,   .   ,   .   ,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPartialBlockResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  3.172,  2.757,  1.757,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.172,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.586,  5.586,  6.586,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  3.500,   .   ,  0.672,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  2.500,  2.086,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,  0.086,  1.086,  1.500,  1.086,  0.672,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPartialBlockDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┬  , ┐  , ┤  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ┌  , ┬  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┴  ,  ~ , ┐  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┴  , ┘  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ├  , └  , ┴  , ┘  , ┘  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        [Test]
        [TestCase(nameof(EmptyRoom), 9, 9, DistanceCalculation.Euclid, EmptyRoom, EmptyRoomResult, EmptyRoomDirections)]
        [TestCase(nameof(EmptyRoom), 9, 9, DistanceCalculation.Chebyshev, EmptyRoom, EmptyRoomResultCheb, EmptyRoomDirectionsCheb)]
        [TestCase(nameof(RoomWithPillars), 15, 15, DistanceCalculation.Euclid, RoomWithPillars, RoomWithPillarsResult, RoomWithPillarDirections)]
        [TestCase(nameof(RoomWithPartialBlock), 15, 15, DistanceCalculation.Euclid, RoomWithPartialBlock, RoomWithPartialBlockResult, RoomWithPartialBlockDirections)]
        public void ValidateMap(string name, int width, int height, DistanceCalculation dc, string sourceText, string intensityResultText, string directionResultText)
        {
            var radius = width / 2;
            var source = new SenseSourceDefinition(dc, radius + 1);
            var pos = new Position2D(width / 2, height / 2);

            var resistanceMap = Parse(sourceText);
            Console.WriteLine("Using room layout \n" + PrintMap(resistanceMap, new Rectangle(0, 0, width, height)));

            var algo = new FloodFillPropagationAlgorithm(LinearDecaySensePhysics.For(dc), new FloodFillWorkingDataSource());
            var calculatedResult = algo.Calculate(source, source.Intensity, pos, resistanceMap);
            Console.WriteLine(PrintMap(calculatedResult, new Rectangle(new Position2D(0, 0), radius, radius)));
            Console.WriteLine(PrintMap(new SenseMapDirectionTestView(calculatedResult), new Rectangle(new Position2D(0, 0), radius, radius)));

            var expectedResult = Parse(intensityResultText);
            AssertEquals(calculatedResult, expectedResult, new Rectangle(0, 0, width, height), pos);
            
            var expectedDirections = ParseDirections(directionResultText, out _);
            AssertEquals(new SenseMapDirectionTestView(calculatedResult), expectedDirections, new Rectangle(0, 0, width, height), pos);
        }
    }
}