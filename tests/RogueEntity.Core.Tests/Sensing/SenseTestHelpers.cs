using System;
using System.IO;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Tests.Sensing
{
    public static class SenseTestHelpers
    {
        public static void AssertEquals(IReadOnlyView2D<float> source,
                                        IReadOnlyView2D<float> other,
                                        in Rectangle bounds,
                                        in Position2D offset)
        {
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                var result = source[x - offset.X, y - offset.Y];
                var expected = other[x, y];
                if (Math.Abs(result - expected) > 0.005f)
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected:0.000} but found {result:0.000}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) + "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }

        public static void AssertEquals(IReadOnlyView2D<bool> source,
                                        IReadOnlyView2D<bool> other,
                                        in Rectangle bounds,
                                        in Position2D offset)
        {
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                var result = source[x - offset.X, y - offset.Y];
                var expected = other[x, y];
                if (result != expected)
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected} but found {result}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) + "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }

        public static DynamicBoolDataView ParseBool(string text) => ParseBool(text, out _);

        public static DynamicBoolDataView ParseBool(string text, out Rectangle parsedBounds)
        {
            var tmp = Parse(text, out parsedBounds);
            var result = new DynamicBoolDataView(tmp.OffsetX, tmp.OffsetY, tmp.TileSizeX, tmp.TileSizeY);
            ImportData(result, tmp, f => f > 0);
            return result;
        }
        
        public static DynamicDataView<float> Parse(string text) => Parse(text, out _);
        public static DynamicDataView<float> Parse(string text, out Rectangle parsedBounds)
        {
            var map = new DynamicDataView<float>(0, 0, 64, 64);
            var row = -1;
            using var sr = new StringReader(text);

            var maxX = 0;
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
                for (var index = 0; index < vs.Length; index++)
                {
                    var v = vs[index];
                    if (string.IsNullOrWhiteSpace(v) || v.Trim().Equals("."))
                    {
                        // Allows for some cleaner to see maps. Using a 
                        // dot for zero makes the cell countable by humans, but 
                        // reduces visual weight to see the interesting bits of data.
                        map[index, row] = 0;
                    }
                    else
                    {
                        // fail hard on parse errors. You should be
                        // able to write an error free float constant.
                        map[index, row] = float.Parse(v);
                    }
                    maxX = Math.Max(maxX, index);
                }
            }

            parsedBounds = new Rectangle(0, 0, maxX + 1, row + 1);
            return map;
        }

        public static string PrintMap(DynamicDataView<float> s)
        {
            return PrintMap(s, s.GetActiveBounds());
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
                                    elementStringifier: (f) => f != 0 ? $"{f,7:0.000}" : "   .   ");
        }

        public static string PrintMap(IReadOnlyView2D<bool> s, in Rectangle bounds)
        {
            return s.ExtendToString(bounds,
                                    elementSeparator: ",",
                                    elementStringifier: (f) => f ? $" 1" : " .");
        }

        public static void ImportData<TResult, TSource>(this IDynamicDataView2D<TResult> targetMap, DynamicDataView<TSource> source, Func<TSource, TResult> converter)
        {
            foreach (var bounds in source.GetActiveTiles())
            {
                if (!source.TryGetData(bounds.X, bounds.Y, out var tile))
                {
                    continue;
                }

                foreach (var (x, y) in tile.Bounds.Contents)
                {
                    if (!tile.TryGet(x, y, out var s)) continue;
                    targetMap.TrySet(x, y, converter(s));
                }
            }
        }
        
        public static Lazy<T> AsLazy<T>(this T l) => new Lazy<T>(l);
    }
}