using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.IO;

namespace RogueEntity.Core.Tests.Fixtures
{
    public static class TestHelpers
    {
        public static T At<T>(this IReadOnlyView2D<T> view, int x, int y)
        {
            if (view.TryGet(x, y, out var r))
            {
                return r;
            }

            return default;
        }

        public static DynamicDataView2D<TData> Parse<TData>(string text, TokenParser tokenParser, out Rectangle parsedBounds)
        {
            var map = new DynamicDataView2D<TData>(0, 0, 64, 64);
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
                    var v = vs[index].Trim();
                    if (!tokenParser.TryParse(v, out TData result))
                    {
                        throw new Exception($"Unable to parse token {v} as {typeof(TData)}");
                    }

                    map[index, row] = result;
                    maxX = Math.Max(maxX, index);
                }
            }

            parsedBounds = new Rectangle(0, 0, maxX + 1, row + 1);
            return map;
        }
        
        public static DynamicDataView2D<TData> Parse<TToken, TData>(string text, TokenParser tokenParser, out Rectangle parsedBounds, Func<TToken, TData> fx)
        {
            var map = new DynamicDataView2D<TData>(0, 0, 64, 64);
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
                    var v = vs[index].Trim();
                    if (!tokenParser.TryParse(v, out TToken result))
                    {
                        throw new Exception($"Unable to parse token {v} as {typeof(TData)}");
                    }

                    map[index, row] = fx(result);
                    maxX = Math.Max(maxX, index);
                }
            }

            parsedBounds = new Rectangle(0, 0, maxX + 1, row + 1);
            return map;
        }


        public static void AssertEquals(IReadOnlyView2D<float> source,
                                        IReadOnlyView2D<float> other,
                                        in Rectangle bounds,
                                        in Position2D offset)
        {
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                if (!source.TryGet(x - offset.X, y - offset.Y, out var result))
                {
                    // throw new IndexOutOfRangeException($"Unable to compare map data: Source index at ({x - offset.X},{y - offset.Y}) out of range");
                } 
                if (!other.TryGet(x, y, out var expected))
                {
                    // throw new IndexOutOfRangeException($"Unable to compare map data: Target index at ({x},{y}) out of range");
                }
                
                if (Math.Abs(result - expected) > 0.005f)
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected:0.000} but found {result:0.000}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }

        public static void AssertEquals(IReadOnlyView2D<bool> source,
                                        IReadOnlyView2D<bool> other,
                                        in Rectangle bounds,
                                        in Position2D offset = default)
        {
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                if (!source.TryGet(x - offset.X, y - offset.Y, out var result) ||
                    !other.TryGet(x, y, out var expected))
                {
                    throw new IndexOutOfRangeException();
                }
                
                if (result != expected)
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected} but found {result}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }

        public static void AssertEquals(IReadOnlyView2D<string> source,
                                        IReadOnlyView2D<string> other,
                                        in Rectangle bounds,
                                        in Position2D offset = default)
        {
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                if (!source.TryGet(x - offset.X, y - offset.Y, out var result) ||
                    !other.TryGet(x, y, out var expected))
                {
                    throw new IndexOutOfRangeException();
                }
                
                if (!string.Equals(result, expected, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected} but found {result}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }

        public static void AssertEquals<TData>(IReadOnlyView2D<TData> testResult,
                                               IReadOnlyView2D<TData> expectedResult,
                                               in Rectangle bounds,
                                               in Position2D offset,
                                               Func<IReadOnlyView2D<TData>, Rectangle, string> printFunction)
            where TData: IEquatable<TData>
        {
            var cmp = EqualityComparer<TData>.Default;
            
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                if (!testResult.TryGet(x - offset.X, y - offset.Y, out var result) ||
                    !expectedResult.TryGet(x, y, out var expected))
                {
                    throw new IndexOutOfRangeException();
                }
                
                if (!cmp.Equals(result, expected))
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected} but found {result}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + printFunction(testResult, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
                                                $"Expected:\n" + printFunction(expectedResult, bounds));
                }
            }
        }

        public static string PrintMap(DynamicDataView2D<float> s)
        {
            return PrintMap(s, s.GetActiveBounds());
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

        public static string PrintMap(IReadOnlyView2D<string> s, in Rectangle bounds)
        {
            return s.ExtendToString(bounds,
                                    elementSeparator: ",",
                                    elementStringifier: (f) => f);
        }

        public static void ImportData<TResult, TSource>(this IDynamicDataView2D<TResult> targetMap, DynamicDataView2D<TSource> source, Func<TSource, TResult> converter)
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

        public static void ImportData<TResult>(this IDynamicDataView2D<TResult> targetMap, DynamicDataView2D<TResult> source)
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
                    targetMap.TrySet(x, y, s);
                }
            }
        }

        public static Lazy<T> AsLazy<T>(this T l) => new Lazy<T>(l);
    }
}