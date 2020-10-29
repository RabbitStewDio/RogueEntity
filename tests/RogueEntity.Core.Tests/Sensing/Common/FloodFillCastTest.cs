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
  0.000,  0.757,  1.172,  1.586,  2.000,  1.586,  1.172,  0.757,  0.000
  0.000,  1.172,  2.172,  2.586,  3.000,  2.586,  2.172,  1.172,  0.000
  0.000,  1.586,  2.586,  3.586,  4.000,  3.586,  2.586,  1.586,  0.000
  0.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  0.000
  0.000,  1.586,  2.586,  3.586,  4.000,  3.586,  2.586,  1.586,  0.000
  0.000,  1.172,  2.172,  2.586,  3.000,  2.586,  2.172,  1.172,  0.000
  0.000,  0.757,  1.172,  1.586,  2.000,  1.586,  1.172,  0.757,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
";

        const string RoomWithPillars = @"
// 11x11; an empty room - larger so that we can have a better radius
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0
0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0
0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0
0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0
0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0
0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0
0, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0
0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0
0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
";

        const string RoomWithPillarsResult = @"
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  3.172,  2.757,  1.757,  0.757,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  4.172,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  4.586,  5.586,  6.586,  7.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  8.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  0.000
";

        [Test]
        [TestCase(nameof(EmptyRoom), 9, 9, EmptyRoom, EmptyRoomResult)]
        [TestCase(nameof(RoomWithPillars), 15, 15, RoomWithPillars, RoomWithPillarsResult)]
        public void ValidateMap(string name, int width, int height, string sourceText, string resultText)
        {
            var radius = width / 2;
            var source = new SenseSourceDefinition(DistanceCalculation.Euclid, radius + 1);
            var pos = new Position2D(width / 2, height / 2);

            var resistanceMap = Parse(sourceText);
            Console.WriteLine("Using room layout \n" + PrintMap(resistanceMap));

            var algo = new FloodFillPropagationAlgorithm(LinearDecaySensePhysics.For(DistanceCalculation.Euclid), new FloodFillWorkingDataSource());
            var calculatedResult = algo.Calculate(source, source.Intensity, pos, resistanceMap);
            Console.WriteLine(PrintMap(calculatedResult, new Rectangle(new Position2D(0, 0), radius, radius)));

            var expectedResult = Parse(resultText);
            AssertEquals(calculatedResult, expectedResult, new Rectangle(0, 0, width, height), pos);
        }
    }
}