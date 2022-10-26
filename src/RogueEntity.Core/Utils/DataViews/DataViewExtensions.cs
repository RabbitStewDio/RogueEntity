using System;
using System.Runtime.CompilerServices;
using System.Text;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public static class DataViewExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TData TryGetMapValue<TResistanceMap, TData>(this TResistanceMap resistanceMap,
                                                                   ref IReadOnlyBoundedDataView<TData>? resistanceTile,
                                                                   int tx,
                                                                   int ty,
                                                                   in TData defaultValue)
            where TResistanceMap : IReadOnlyDynamicDataView2D<TData>
        {
            if (resistanceTile == null! || !resistanceTile.TryGet(tx, ty, out var resistance))
            {
                if (!resistanceMap.TryGetData(tx, ty, out resistanceTile) ||
                    !resistanceTile.TryGet(tx, ty, out resistance))
                {
                    resistance = defaultValue;
                }
            }

            return resistance;
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static ref T? TryGetRefForUpdate<T>(this IDynamicDataView2D<T> data,
                                                   ref IBoundedDataView<T>? tile,
                                                   int x,
                                                   int y,
                                                   ref T? defaultValue,
                                                   out bool success,
                                                   DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (tile != null)
            {
                if (tile.Contains(x, y))
                {
                    return ref tile.TryGetForUpdate(x, y, ref defaultValue, out success);
                }
            }

            if (data.TryGetWriteAccess(x, y, out tile, mode))
            {
                return ref tile.TryGetForUpdate(x, y, ref defaultValue, out success);
            }

            success = false;
            return ref defaultValue;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetTileForUpdate<TResistanceMap, TData>(this TResistanceMap resistanceMap,
                                                                      ref IBoundedDataView<TData>? resistanceTile,
                                                                      int tx,
                                                                      int ty,
                                                                      DataViewCreateMode mode = DataViewCreateMode.Nothing)
            where TResistanceMap : IDynamicDataView2D<TData>
        {
            if (resistanceTile != null)
            {
                if (resistanceTile.Contains(tx, ty))
                {
                    return true;
                }
            }

            return resistanceMap.TryGetWriteAccess(tx, ty, out resistanceTile, mode);
        }

        public static IDynamicDataView2D<TData> GetOrCreate<TData>(this IDynamicDataView3D<TData> data, int z)
        {
            if (data.TryGetWritableView(z, out var view, DataViewCreateMode.CreateMissing))
            {
                return view;
            }

            throw new InvalidOperationException();
        }

        public static TranslatedDataView<TData> TranslateBy<TData>(this IReadOnlyView2D<TData> view, int offsetX, int offsetY)
        {
            return new TranslatedDataView<TData>(view, offsetX, offsetY);
        }

        public static IReadOnlyDynamicDataView3D<TResult> Transform<TSource, TResult>(this IReadOnlyDynamicDataView3D<TSource> source,
                                                                                      Func<TSource, TResult> transformFunction)
        {
            return new TransformedView3D<TSource, TResult>(source, transformFunction);
        }

        public static IReadOnlyDynamicDataView2D<TResult> Transform<TSource, TResult>(this IReadOnlyDynamicDataView2D<TSource> source,
                                                                                      Func<TSource, TResult> transformFunction)
        {
            return new TransformedView2D<TSource, TResult>(source, transformFunction);
        }

        public static IReadOnlyDynamicDataView3D<T> As3DMap<T>(this IReadOnlyDynamicDataView2D<T> layerData, int z) => new DataViewWrapper3D<T>(z, layerData);

        class DataViewWrapper3D<T> : IReadOnlyDynamicDataView3D<T>
        {
            public event EventHandler<DynamicDataView3DEventArgs<T>>? ViewCreated
            {
                add { }
                remove { }
            }

            public event EventHandler<DynamicDataView3DEventArgs<T>>? ViewExpired
            {
                add { }
                remove { }
            }

            public event EventHandler<DynamicDataView3DEventArgs<T>>? ViewReset
            {
                add { }
                remove { }
            }

            readonly int z;
            readonly IReadOnlyDynamicDataView2D<T> backend;

            public DataViewWrapper3D(int z, IReadOnlyDynamicDataView2D<T> backend)
            {
                this.z = z;
                this.backend = backend;
            }

            public bool TryGetView(int zLevel, [MaybeNullWhen(false)] out IReadOnlyDynamicDataView2D<T> view)
            {
                if (this.z == zLevel)
                {
                    view = backend;
                    return true;
                }

                view = default;
                return false;
            }

            public BufferList<int> GetActiveLayers(BufferList<int>? buffer = null)
            {
                buffer = BufferList.PrepareBuffer(buffer);
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

        /// <summary>
        /// Allows stringifying the contents of a map view. Takes characters to
        /// surround the map printout, and each row, the method used to get the string representation
        /// of each element (defaulting to the ToString function of type T), and separation
        /// characters for each element and row.
        /// </summary>
        /// <typeparam name="T"/>
        /// <param name="map"/>
        /// <param name="bounds"></param>
        /// <param name="begin">Character(s) that should precede the IMapView printout.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">
        /// Function to use to get the string representation of each value. null uses the ToString
        /// function of type T.
        /// </param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the IMapView printout.</param>
        /// <returns>A string representation of the map, as viewd by the given map view.</returns>
        public static string ExtendToString<T>(this IReadOnlyView2D<T> map,
                                               in Rectangle bounds,
                                               string begin = "",
                                               string beginRow = "",
                                               Func<T, string>? elementStringifier = null,
                                               string rowSeparator = "\n",
                                               string elementSeparator = " ",
                                               string endRow = "",
                                               string end = "")
        {
            elementStringifier ??= obj => obj?.ToString() ?? "";

            var result = new StringBuilder(begin);
            for (var y = bounds.MinExtentY; y <= bounds.MaxExtentY; y++)
            {
                result.Append(beginRow);
                for (var x = bounds.MinExtentX; x <= bounds.MaxExtentX; x++)
                {
                    if (map.TryGet(x, y, out var val))
                    {
                        result.Append(elementStringifier(val));
                    }

                    if (x != bounds.MaxExtentX)
                    {
                        result.Append(elementSeparator);
                    }
                }

                result.Append(endRow);
                if (y != bounds.MaxExtentY)
                {
                    result.Append(rowSeparator);
                }
            }

            result.Append(end);

            return result.ToString();
        }

        /// <summary>
        /// Allows stringifying the contents of a map view. Takes characters to
        /// surround the map, and each row, the method used to get the string representation of each
        /// element (defaulting to the ToString function of type T), and separation characters for
        /// each element and row. Takes the size of the field to give each element, characters to
        /// surround the MapView printout, and each row, the method used to get the string
        /// representation of each element (defaulting to the ToString function of type T), and
        /// separation characters for each element and row.
        /// </summary>
        /// <typeparam name="T"/>
        /// <param name="map"/>
        /// <param name="bounds"></param>
        /// <param name="fieldSize">
        /// The amount of space each element should take up in characters. A positive number aligns
        /// the text to the right of the space, while a negative number aligns the text to the left.
        /// </param>
        /// <param name="begin">Character(s) that should precede the IMapView printout.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">
        /// Function to use to get the string representation of each value. Null uses the ToString
        /// function of type T.
        /// </param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the IMapView printout.</param>
        /// <returns>A string representation of the map, as viewd by the given map view.</returns>
        public static string ExtendToString<T>(this IReadOnlyView2D<T> map,
                                               in Rectangle bounds,
                                               int fieldSize,
                                               string begin = "",
                                               string beginRow = "",
                                               Func<T, string>? elementStringifier = null,
                                               string rowSeparator = "\n",
                                               string elementSeparator = " ",
                                               string endRow = "",
                                               string end = "")
        {
            if (elementStringifier == null)
                elementStringifier = obj => obj?.ToString() ?? "";

            var fmtString = $"{{0, {fieldSize}}}";
            var result = new StringBuilder(begin);
            for (var y = bounds.MinExtentY; y <= bounds.MaxExtentY; y++)
            {
                result.Append(beginRow);
                for (var x = bounds.MinExtentX; x <= bounds.MaxExtentX; x++)
                {
                    if (map.TryGet(x, y, out var value))
                    {
                        result.Append(string.Format(fmtString, elementStringifier(value)));
                    }
                    
                    if (x != bounds.MaxExtentX)
                    {
                        result.Append(elementSeparator);
                    }
                }

                result.Append(endRow);
                if (y != bounds.MaxExtentY)
                {
                    result.Append(rowSeparator);
                }
            }

            result.Append(end);

            return result.ToString();
        }

        public static string ExtendToString(this IReadOnlyView2D<float> map,
                                            in Rectangle bounds,
                                            int fieldSize)
        {
            string Format(float f) => $"{f:0.00}";
            return map.ExtendToString(bounds, fieldSize, "", "", Format);
        }

        public static string ExtendToString(this IReadOnlyView2D<float> map,
                                            in Rectangle bounds,
                                            Func<float,string> format)
        {
            return map.ExtendToString(bounds, "", "", format);
        }


        public static void RemoveAllViews<T>(this IDynamicDataView3D<T> data, BufferList<int>? buffer = null)
        {
            buffer = data.GetActiveLayers(buffer);
            foreach (var b in buffer)
            {
                data.RemoveView(b);
            }
        }
    }
}