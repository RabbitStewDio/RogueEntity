using System;
using System.Collections.Generic;
using System.IO;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Tests
{
    public static class TestHelpers
    {
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
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
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
                                                $"SourceMap:\n" + PrintMap(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
                                                $"Expected:\n" + PrintMap(other, in bounds));
                }
            }
        }

        public static void AssertEquals(IReadOnlyView2D<string> source,
                                        IReadOnlyView2D<string> other,
                                        in Rectangle bounds,
                                        in Position2D offset)
        {
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                var result = source[x - offset.X, y - offset.Y];
                var expected = other[x, y];
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

        public static void AssertEquals<TData>(IReadOnlyView2D<TData> source,
                                               IReadOnlyView2D<TData> other,
                                               in Rectangle bounds,
                                               in Position2D offset,
                                               Func<IReadOnlyView2D<TData>, Rectangle, string> printFunction)
            where TData: IEquatable<TData>
        {
            var cmp = EqualityComparer<TData>.Default;
            
            foreach (var pos in bounds.Contents)
            {
                var (x, y) = pos;

                var result = source[x - offset.X, y - offset.Y];
                var expected = other[x, y];
                if (!cmp.Equals(result, expected))
                {
                    throw new ArgumentException($"Error in comparison at [{x}, {y}]: Expected {expected} but found {result}.\n" +
                                                $"{bounds} \n" +
                                                $"SourceMap:\n" + printFunction(source, new Rectangle(bounds.MinExtentX - offset.X, bounds.MinExtentY - offset.Y, bounds.Width, bounds.Height)) +
                                                "\n" +
                                                $"Expected:\n" + printFunction(other, bounds));
                }
            }
        }

        public static string PrintMap(DynamicDataView2D<float> s)
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

        public static IReadOnlyDynamicDataView3D<T> As3DMap<T>(this IReadOnlyDynamicDataView2D<T> layerData, int z) => new DataViewWrapper3D<T>(z, layerData);

        class DataViewWrapper3D<T> : IReadOnlyDynamicDataView3D<T>
        {
            public event EventHandler<DynamicDataView3DEventArgs<T>> ViewCreated;
            public event EventHandler<DynamicDataView3DEventArgs<T>> ViewExpired;
            readonly int z;
            readonly IReadOnlyDynamicDataView2D<T> backend;

            public DataViewWrapper3D(int z, IReadOnlyDynamicDataView2D<T> backend)
            {
                this.z = z;
                this.backend = backend;
            }

            public bool TryGetView(int zLevel, out IReadOnlyDynamicDataView2D<T> view)
            {
                if (this.z == zLevel)
                {
                    view = backend;
                    return true;
                }

                view = default;
                return false;
            }

            public List<int> GetActiveLayers(List<int> buffer = null)
            {
                if (buffer == null)
                {
                    buffer = new List<int>();
                }
                else
                {
                    buffer.Clear();
                }

                buffer.Add(z);
                return buffer;
            }

            public int OffsetX
            {
                get { return backend.OffsetX; }
            }

            public int OffsetY
            {
                get { return backend.OffsetY; }
            }

            public int TileSizeX
            {
                get { return backend.TileSizeX; }
            }

            public int TileSizeY
            {
                get { return backend.TileSizeY; }
            }
        }
    }
}