using System;
using System.Text;
using JetBrains.Annotations;

namespace RogueEntity.Core.Utils.Maps
{
    public static class MapDataExtensions
    {
        public readonly struct MapView<T> : IReadOnlyView2D<T>
        {
            readonly IReadOnlyMapData<T> map;

            public MapView([NotNull] IReadOnlyMapData<T> map)
            {
                this.map = map ?? throw new ArgumentNullException(nameof(map));
            }

            public bool TryGet(int x, int y, out T data)
            {
                if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                {
                    data = default;
                    return false;
                }
                
                data = map[x, y];
                return true;
            }

            public T this[int x, int y]
            {
                get
                {
                    if (x < 0 || y < 0) return default;
                    if (x >= map.Width || y >= map.Height) return default;
                    return map[x, y];
                }
            }
        }

        public static MapView<T> ToView<T>(this IReadOnlyMapData<T> map) => new MapView<T>(map);

        /// <summary>
        /// Allows stringifying the contents of a map view. Takes characters to
        /// surround the map printout, and each row, the method used to get the string representation
        /// of each element (defaulting to the ToString function of type T), and separation
        /// characters for each element and row.
        /// </summary>
        /// <typeparam name="T"/>
        /// <param name="map"/>
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
        public static string ExtendToString<T>(this IReadOnlyMapData<T> map,
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
            for (var y = 0; y < map.Height; y++)
            {
                result.Append(beginRow);
                for (var x = 0; x < map.Width; x++)
                {
                    result.Append(elementStringifier(map[x, y]));
                    if (x != map.Width - 1) result.Append(elementSeparator);
                }

                result.Append(endRow);
                if (y != map.Height - 1) result.Append(rowSeparator);
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
        public static string ExtendToString<T>(this IReadOnlyMapData<T> map,
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
            for (var y = 0; y < map.Height; y++)
            {
                result.Append(beginRow);
                for (var x = 0; x < map.Width; x++)
                {
                    result.Append(string.Format($"{{0, {fieldSize}}} ", elementStringifier(map[x, y])));
                    if (x != map.Width - 1) result.Append(elementSeparator);
                }

                result.Append(endRow);
                if (y != map.Height - 1) result.Append(rowSeparator);
            }

            result.Append(end);

            return result.ToString();
        }

        public static string ExtendToString(this IReadOnlyMapData<float> map, int fieldSize)
        {
            string Format(float f) => $"{f:0.00}";
            return ExtendToString(map, fieldSize, "", "", Format);
        }
    }
}