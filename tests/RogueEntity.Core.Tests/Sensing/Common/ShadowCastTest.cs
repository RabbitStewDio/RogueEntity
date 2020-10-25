using System;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.Maps;
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
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.757,  1.394,  1.838,  2.000,  1.838,  1.394,  0.757,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  0.757,  1.394,  1.838,  2.000,  1.838,  1.394,  0.757,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
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
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.757,  1.394,  0.000,  0.000,  1.838,  0.000,  0.000,  0.000
  0.000,  1.394,  2.172,  0.000,  0.000,  2.764,  0.000,  0.000,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  0.000,  0.000,  1.838,  0.000
  0.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  1.394,  0.000,  0.000,  3.000,  2.764,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  1.838,  2.000,  1.838,  1.394,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
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
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  1.394,  0.757,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  2.764,  2.172,  1.394,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  3.586,  0.000,  0.000,  0.000
  0.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  0.000
  0.000,  0.000,  0.000,  0.000,  4.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  2.764,  3.000,  2.764,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  1.838,  2.000,  1.838,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
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
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  1.838,  1.394,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  2.764,  0.000,  0.000,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  0.000,  0.000,  0.000,  3.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  1.838,  2.000,  1.838,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
";

        
        [Test]
        [TestCase(nameof(EmptyRoom), 9, 9, EmptyRoom, EmptyRoomResult)]
        [TestCase(nameof(RoomWithPillars), 9, 9, RoomWithPillars, RoomWithPillarsResult)]
        [TestCase(nameof(RoomDoorNear), 9, 9, RoomDoorNear, RoomDoorNearResult)]
        [TestCase(nameof(RoomDoorFar), 9, 9, RoomDoorFar, RoomDoorFarResult)]
        public void ValidateMap(string name, int width, int height, string sourceText, string resultText)
        {
            var radius = width / 2;
            var source = new SenseSourceDefinition(DistanceCalculation.Euclid, radius + 1);
            var sd = new SenseSourceData(radius);
            var pos = new Position2D(width / 2, height / 2);
            
            var resistanceMap = Parse(sourceText);
            Console.WriteLine("Using room layout \n" + PrintMap(resistanceMap));
            
            var algo = new ShadowPropagationAlgorithm(LinearDecaySensePhysics.For(DistanceCalculation.Euclid));
            var calculatedResult = algo.Calculate(source, source.Intensity, pos, resistanceMap, null);
            Console.WriteLine(PrintMap(calculatedResult, new Rectangle(new Position2D(0,0), radius, radius)));

            var expectedResult = Parse(resultText);
            AssertEquals(calculatedResult, expectedResult, new Rectangle(0, 0, width, height), pos);
        }
    }
}