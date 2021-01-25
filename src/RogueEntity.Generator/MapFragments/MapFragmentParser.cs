using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RogueEntity.Generator.MapFragments
{
    public class MapFragmentParser
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(MapFragmentParser));
        static readonly List<string> EmptyTags = new List<string>();

        public delegate bool MapFragmentPostProcessor(in MapFragment mf, out MapFragment result);

        readonly List<MapFragmentPostProcessor> postProcessors;

        /// <summary>
        ///   Temporary object using during parsing. This object is instantiated via
        ///   reflection and all setters are called via reflection. So shut up, Inspector.
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        class MapFragmentParseInfo
        {
            public string Guid { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public Size Size { get; set; }
            public Dictionary<string, string> Properties { get; set; }
            public Dictionary<char, List<string>> Symbols { get; set; }
            public List<string> Tags { get; set; }
        }

        public MapFragmentParser()
        {
            this.postProcessors = new List<MapFragmentPostProcessor>();
        }

        public void AddPostProcessor([NotNull] MapFragmentPostProcessor p)
        {
            if (p == null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            this.postProcessors.Add(p);
        }

        public void RemovePostProcessor([NotNull] MapFragmentPostProcessor p)
        {
            this.postProcessors.Remove(p);
        }

        public ReadOnlyListWrapper<MapFragmentPostProcessor> PostProcessors => postProcessors;
        
        public bool TryParseFromFile(string fileName, out MapFragment m)
        {
            var text = File.ReadAllText(fileName, Encoding.UTF8);
            return TryParse(text, out m, fileName);
        }

        public bool TryParse(string data, out MapFragment mapFragment, string context = null)
        {
            var deserializer = new DeserializerBuilder()
                               .Build();

            try
            {
                var parser = new Parser(new StringReader(data));
                parser.Consume<StreamStart>();
                if (!parser.Accept<DocumentStart>(out _))
                {
                    Logger.Information("YAML structure from {Context} is empty", context);
                    mapFragment = default;
                    return false;
                }

                var mapInfo = deserializer.Deserialize<MapFragmentParseInfo>(parser);
                if (!parser.Accept<DocumentStart>(out var yamlStart))
                {
                    Logger.Information("{Context} had no YAML secondary document separators", context);
                    mapFragment = default;
                    return false;
                }

                if (mapInfo.Size.Width == 0 || mapInfo.Size.Height == 0)
                {
                    Logger.Information("Empty map is not allowed");
                    mapFragment = default;
                    return false;
                }

                if (mapInfo.Symbols == null || mapInfo.Symbols.Count == 0)
                {
                    Logger.Information("Symbol declaration is mandatory");
                    mapFragment = default;
                    return false;
                }

                if (!ParseMapData(data.Substring(yamlStart.End.Index), mapInfo, out var mapData))
                {
                    Logger.Information("Unable to parse map data payload");
                    mapFragment = default;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(mapInfo.Name))
                {
                    mapInfo.Name = context ?? $"Auto-Generated-Name:{GuidUtility.Create(GuidUtility.UrlNamespace, data)}";
                }

                var props = new RuleProperties();
                if (mapInfo.Properties != null)
                {
                    foreach (var kv in mapInfo.Properties)
                    {
                        props.AddProperty(kv.Key, kv.Value);
                    }
                }
                
                var mi = new MapFragmentInfo(mapInfo.Name, mapInfo.Type, props, mapInfo.Tags ?? EmptyTags);
                if (!Guid.TryParse(mapInfo.Guid, out var id))
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, mapInfo.Name);
                }
                
                mapFragment = new MapFragment(id, mapData, mi, new TypedRuleProperties());

                for (var index = 0; index < postProcessors.Count; index++)
                {
                    var pp = postProcessors[index];
                    if (!pp(mapFragment, out var result))
                    {
                        mapFragment = default;
                        return false;
                    }
                    
                    mapFragment = result;
                }

                return true;
            }
            catch (SyntaxErrorException se)
            {
                Log.Error(se, "Failed to parse {Context}", context);
                mapFragment = default;
                return false;
            }
        }

        public static bool TryPostProcessConnectivity(in MapFragment f, out MapFragment result)
        {
            var props = f.Info.Properties;
            if (!props.TryGetValue(MapFragmentExtensions.ConnectivityProperty, out string c))
            {
                Log.Error("Definition had no connectivity information");
                result = default;
                return false;
            }

            var connectivity = MapFragmentExtensions.ParseMapFragmentConnectivity(c);
            f.Properties.Define(connectivity);
            result = f;
            return true;
        }

        static IEnumerable<string> GetLines(string str, bool removeEmptyLines = false)
        {
            using var sr = new StringReader(str);
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

        static bool ParseMapData(string data, MapFragmentParseInfo info, out IReadOnlyMapData<MapFragmentTagDeclaration> map)
        {
            var mapWritable = new DenseMapData<MapFragmentTagDeclaration>(info.Size.Width, info.Size.Height);
            int y = 0;
            foreach (var line in GetLines(data, true))
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

                    mapWritable[x, y] = new MapFragmentTagDeclaration(value);
                }

                y += 1;
            }

            map = mapWritable;
            return true;
        }
    }
}