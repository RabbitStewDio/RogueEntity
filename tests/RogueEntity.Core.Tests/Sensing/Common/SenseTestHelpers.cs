using System;
using System.IO;
using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class SenseTestHelpers
    {
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
    }
}