using RogueEntity.Api.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RogueEntity.Generator.MapFragments
{
    public static class MapFragmentParser
    {
        static readonly List<string> EmptyTags = new List<string>();

        /// <summary>
        ///   Temporary object using during parsing.
        /// </summary>
        public class MapFragmentParseInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public Size Size { get; set; }
            public Dictionary<string, string> Properties { get; set; }
            public Dictionary<char, List<string>> Symbols { get; set; }
            public List<string> Tags { get; set; }
        }

        class OptionalNodeDeserializer : INodeDeserializer
        {
            readonly INodeDeserializer inner;

            public OptionalNodeDeserializer(INodeDeserializer inner)
            {
                this.inner = inner;
            }

            public bool Deserialize(IParser reader,
                                    Type expectedType,
                                    Func<IParser, Type, object> nestedObjectDeserializer, out object value)
            {
                if (IsOptional(expectedType, out var innerType))
                {
                    if (!inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out var rawValue))
                    {
                        value = default;
                        return false;
                    }

                    return ConstructOptional(innerType, rawValue, out value);
                }

                return inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }

            bool ConstructOptional(Type t, object value, out object created)
            {
                if (!t.IsInstanceOfType(value))
                {
                    created = default;
                    return false;
                }

                var mi = GetType().GetMethod(nameof(MakeOptional));
                if (mi == null)
                {
                    created = default;
                    return false;
                }

                created = mi.MakeGenericMethod(t).Invoke(this, new[] {value});
                return true;
            }

            Optional<T> MakeOptional<T>(T value)
            {
                return Optional.ValueOf(value);
            }

            bool IsOptional(Type t, out Type innerType)
            {
                if (!t.IsGenericType)
                {
                    innerType = t;
                    return false;
                }

                if (t.GetGenericTypeDefinition() == typeof(Optional<bool>).GetGenericTypeDefinition())
                {
                    innerType = t.GenericTypeArguments[0];
                    return true;
                }

                innerType = t;
                return false;
            }
        }

        public static bool TryParseFromFile(string fileName, out MapFragment m)
        {
            var text = File.ReadAllText(fileName, Encoding.UTF8);
            return TryParse(text, out m, fileName);
        }

        public static bool TryParse(string data, out MapFragment m, string context = null)
        {
            var deserializer = new DeserializerBuilder()
                               // .WithNodeDeserializer(inner => new OptionalNodeDeserializer(inner),
                               //                       s => s.InsteadOf<ObjectNodeDeserializer>())
                               .Build();

            try
            {
                var parser = new Parser(new StringReader(data));
                parser.Consume<StreamStart>();
                if (!parser.Accept<DocumentStart>(out var startEvent))
                {
                    Log.Information("YAML structure from {Context} is empty.", context);
                    m = default;
                    return false;
                }

                var mapInfo = deserializer.Deserialize<MapFragmentParseInfo>(parser);
                if (!parser.Accept<DocumentStart>(out var yamlStart))
                {
                    Log.Information("{Context} had no YAML secondary document separators.", context);
                    m = default;
                    return false;
                }

                if (mapInfo.Size.Width == 0 || mapInfo.Size.Height == 0)
                {
                    Log.Information("Empty map is not allowed.");
                    m = default;
                    return false;
                }

                if (mapInfo.Symbols == null || mapInfo.Symbols.Count == 0)
                {
                    Log.Information("Symbol declaration is mandatory.");
                    m = default;
                    return false;
                }

                if (!ParseMapData(data.Substring(yamlStart.End.Index), mapInfo, out var mapData))
                {
                    Log.Information("Unable to parse map data payload.");
                    m = default;
                    return false;
                }

                var props = new RuleProperties();
                if (mapInfo.Properties != null)
                {
                    foreach (var kv in mapInfo.Properties)
                    {
                        props.AddProperty(kv.Key, kv.Value);
                    }
                }

                if (!props.TryGetValue(MapFragmentExtensions.ConnectivityProperty, out string c))
                {
                    Log.Error("Definition had no connectivity information");
                    m = default;
                    return false;
                }

                var connectivity = MapFragmentExtensions.ParseMapFragmentConnectivity(c);

                var mi = new MapFragmentInfo(mapInfo.Name, mapInfo.Type, connectivity, props, mapInfo.Tags ?? EmptyTags);
                m = new MapFragment(mapData, mi);
                return true;
            }
            catch (SyntaxErrorException se)
            {
                Log.Error(se, "Failed to parse {Context}", context);
                m = default;
                return false;
            }
        }

        public static IEnumerable<string> GetLines(this string str, bool removeEmptyLines = false)
        {
            using (var sr = new StringReader(str))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (removeEmptyLines && string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    yield return line;
                }
            }
        }

        static bool ParseMapData(string data, MapFragmentParseInfo info, out IReadOnlyMapData<MapFragmentTagDeclaration> map)
        {
            var mapWritable = new DenseMapData<MapFragmentTagDeclaration>(info.Size.Width, info.Size.Height);
            int y = 0;
            foreach (var line in data.GetLines(true))
            {
                if (y >= mapWritable.Width)
                {
                    break;
                }

                for (var x = 0; x < line.Length && x < mapWritable.Width; x++)
                {
                    var c = line[x];
                    if (!info.Symbols.TryGetValue(c, out var value))
                    {
                        continue;
                    }

                    switch (value.Count)
                    {
                        case 0:
                            continue;
                        case 1:
                            mapWritable[x, y] = new MapFragmentTagDeclaration(value[0], null, null);
                            break;
                        case 2:
                            mapWritable[x, y] = new MapFragmentTagDeclaration(value[0], value[1], null);
                            break;
                        default:
                            mapWritable[x, y] = new MapFragmentTagDeclaration(value[0], value[1], value[2]);
                            break;
                    }
                }

                y += 1;
            }

            map = mapWritable;
            return true;
        }
    }
}