using System;
using System.IO;
using GoRogue;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Utils.Maps;

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
  0.000,  0.000,  0.000,  0.000,  1.000,  0.000,  0.000,  0.000,  0.000
  0.000,  0.000,  1.394,  1.838,  2.000,  1.838,  1.394,  0.000,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  0.000,  1.394,  1.838,  2.000,  1.838,  1.394,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  1.000,  0.000,  0.000,  0.000,  0.000
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
  0.000,  0.000,  1.394,  0.000,  0.000,  1.838,  0.000,  0.000,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  0.000,  0.000,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  0.000,  0.000,  1.838,  2.000,  1.838,  1.394,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  1.000,  0.000,  0.000,  0.000,  0.000
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
  0.000,  0.000,  0.000,  0.000,  0.000,  0.000,  1.394,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  0.000,  2.764,  2.172,  1.394,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  0.000,  0.000,  2.764,  3.000,  2.764,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  1.838,  2.000,  1.838,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  1.000,  0.000,  0.000,  0.000,  0.000
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
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  1.000,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,  1.000
  0.000,  1.838,  2.764,  3.586,  4.000,  3.586,  2.764,  1.838,  0.000
  0.000,  1.394,  2.172,  2.764,  3.000,  2.764,  2.172,  1.394,  0.000
  0.000,  0.000,  0.000,  1.838,  2.000,  1.838,  0.000,  0.000,  0.000
  0.000,  0.000,  0.000,  0.000,  1.000,  0.000,  0.000,  0.000,  0.000
";

        
        public static DenseMapData<float> Parse(int width, int height, string text)
        {
            var map = new DenseMapData<float>(width, height);
            var row = -1;
            using var sr = new StringReader(text);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                line = line.Trim();
                if (line.StartsWith("//"))
                {
                    // allow comments. I am not a monster.
                    continue;
                }

                row += 1;
                var vs = line.Split(",");
                for (var index = 0; index < Math.Min(width, vs.Length); index++)
                {
                    var v = vs[index];
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        map[index, row] = 0;
                    }
                    else
                    {
                        // fail hard on parse errors. You should be
                        // able to write an error free float constant.
                        map[index, row] = float.Parse(v);
                    }
                }
            }

            return map;
        }

        public static string PrintMap(IReadOnlyMapData<float> s)
        {
            return s.ExtendToString(
                elementSeparator: ",",
                elementStringifier: (f) => $"{f,7:0.000}");
        }

        public static string PrintMap(IReadOnlyView2D<float> s, in Rectangle bounds)
        {
            return s.ExtendToString(bounds,
                elementSeparator: ",",
                elementStringifier: (f) => $"{f,7:0.000}");
        }
        
        [Test]
        [TestCase(nameof(EmptyRoom), 9, 9, EmptyRoom, EmptyRoomResult)]
        [TestCase(nameof(RoomWithPillars), 9, 9, RoomWithPillars, RoomWithPillarsResult)]
        [TestCase(nameof(RoomDoorNear), 9, 9, RoomDoorNear, RoomDoorNearResult)]
        [TestCase(nameof(RoomDoorFar), 9, 9, RoomDoorFar, RoomDoorFarResult)]
        public void ValidateMap(string name, int width, int height, string sourceText, string resultText)
        {
            var radius = width / 2;
            var source = new SenseSourceDefinition(DistanceCalculation.EUCLIDEAN, radius, radius + 1);
            var sd = new SenseSourceData(radius);
            var pos = new Position2D(width / 2, height / 2);
            
            var resistanceMap = Parse(width, height, sourceText);
            Console.WriteLine("Using room layout \n" + PrintMap(resistanceMap));
            
            var algo = new ShadowPropagationAlgorithm(LinearDecaySensePhysics.Instance);
            var calculatedResult = algo.Calculate(source, pos, resistanceMap, null);
            Console.WriteLine(PrintMap(calculatedResult, new Rectangle(new Coord(0,0), radius, radius)));

            var expectedResult = Parse(width, height, resultText);
            AssertEquals(calculatedResult, expectedResult, new Rectangle(0, 0, width, height), pos);
        }
        
        public static void AssertEquals(IReadOnlyView2D<float> source, 
                                        IReadOnlyView2D<float> other, 
                                        in Rectangle bounds,
                                        in Position2D offset)
        {

            foreach (var pos in bounds.Positions())
            {
                var (x, y) = pos;
                
                var result = source[x - offset.X, y - offset.Y];
                var expected = other[x, y];
                if (Math.Abs(result - expected) > 0.005f)
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected:0.000} but found {result:0.000}.\n" +
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) + "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }
        
    }
}