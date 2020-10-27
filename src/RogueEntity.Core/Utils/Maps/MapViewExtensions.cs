using System;
using System.Text;

namespace RogueEntity.Core.Utils.Maps
{
    public static class MapViewExtensions
    {
        public static TranslatedDataView<TData> TranslateBy<TData>(this IReadOnlyView2D<TData> view, int offsetX, int offsetY)
        {
            return new TranslatedDataView<TData>(view, offsetX, offsetY);
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
            if (elementStringifier == null)
            {
                elementStringifier = obj => obj.ToString();
            }

            var result = new StringBuilder(begin);
            for (var y = bounds.MinExtentY; y <= bounds.MaxExtentY; y++)
            {
                result.Append(beginRow);
                for (var x = bounds.MinExtentX; x <= bounds.MaxExtentX; x++)
                {
                    result.Append(elementStringifier(map[x, y]));
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