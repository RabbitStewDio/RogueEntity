using System;
using System.Text;

namespace RogueEntity.Core.Utils.DataViews
{
    public static class DataViewExtensions
    {
        public static TData TryGet<TResistanceMap, TData>(this TResistanceMap resistanceMap,
                                                          ref IReadOnlyBoundedDataView<TData> resistanceTile,
                                                          int tx,
                                                          int ty,
                                                          in TData defaultValue)
            where TResistanceMap : IReadOnlyDynamicDataView2D<TData>
        {
            if (resistanceTile == null || !resistanceTile.TryGet(tx, ty, out var resistance))
            {
                if (!resistanceMap.TryGetData(tx, ty, out resistanceTile) ||
                    !resistanceTile.TryGet(tx, ty, out resistance))
                {
                    resistance = defaultValue;
                }
            }

            return resistance;
        }

        public static TData TryGetForUpdate<TResistanceMap, TData>(this TResistanceMap resistanceMap,
                                                                   ref IBoundedDataView<TData> resistanceTile,
                                                                   int tx,
                                                                   int ty,
                                                                   in TData defaultValue, 
                                                                   DataViewCreateMode mode = DataViewCreateMode.Nothing)
            where TResistanceMap : IDynamicDataView2D<TData>
        {
            if (resistanceTile == null || !resistanceTile.TryGet(tx, ty, out var resistance))
            {
                if (!resistanceMap.TryGetWriteAccess(tx, ty, out resistanceTile) ||
                    !resistanceTile.TryGet(tx, ty, out resistance))
                {
                    resistance = defaultValue;
                }
            }

            return resistance;
        }

        public static bool TryUpdate<TResistanceMap, TData>(this TResistanceMap resistanceMap,
                                                         ref IBoundedDataView<TData> resistanceTile,
                                                         int tx,
                                                         int ty,
                                                         in TData value, 
                                                         DataViewCreateMode mode = DataViewCreateMode.Nothing)
            where TResistanceMap : IDynamicDataView2D<TData>
        {
            if (resistanceTile == null)
            {
                if (!resistanceMap.TryGetWriteAccess(tx, ty, out resistanceTile, mode))
                {
                    return false;
                }
            }
            
            if (resistanceTile.TrySet(tx, ty, in value))
            {
                return true;
            }

            return resistanceMap.TrySet(tx, ty, in value);

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
                                               Func<T, string> elementStringifier = null,
                                               string rowSeparator = "\n",
                                               string elementSeparator = " ",
                                               string endRow = "",
                                               string end = "")
        {
            elementStringifier ??= obj => obj.ToString();

            var result = new StringBuilder(begin);
            for (var y = bounds.MinExtentY; y <= bounds.MaxExtentY; y++)
            {
                result.Append(beginRow);
                for (var x = bounds.MinExtentX; x <= bounds.MaxExtentX; x++)
                {
                    var val = map[x, y];
                    result.Append(elementStringifier(val));
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
                                               Func<T, string> elementStringifier = null,
                                               string rowSeparator = "\n",
                                               string elementSeparator = " ",
                                               string endRow = "",
                                               string end = "")
        {
            if (elementStringifier == null)
                elementStringifier = obj => obj.ToString();

            var result = new StringBuilder(begin);
            for (var y = bounds.MinExtentY; y <= bounds.MaxExtentY; y++)
            {
                result.Append(beginRow);
                for (var x = bounds.MinExtentX; x <= bounds.MaxExtentX; x++)
                {
                    result.Append(string.Format($"{{0, {fieldSize}}} ", elementStringifier(map[x, y])));
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
    }
}