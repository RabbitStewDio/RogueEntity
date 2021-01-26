using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Utils.Maps;
using Serilog;
using System;
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
        readonly Dictionary<string, MapFragmentParseInfo> processedTemplates;

        public MapFragmentParser()
        {
            this.postProcessors = new List<MapFragmentPostProcessor>();
            this.processedTemplates = new Dictionary<string, MapFragmentParseInfo>();
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
            var text = ReadAllText(fileName);
            return TryParse(text, out m, fileName);
        }

        protected virtual string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName, Encoding.UTF8);
        }

        bool TryParseFragment(string templateName, string context, out MapFragmentParseInfo result, List<string> visitedContexts)
        {
            if (visitedContexts == null)
            {
                visitedContexts = new List<string>();
            }

            if (string.IsNullOrWhiteSpace(context))
            {
                result = default;
                return false;
            }

            var f = Resolve(context, templateName);
            if (processedTemplates.TryGetValue(f, out result))
            {
                return true;
            }

            if (visitedContexts.Contains(f))
            {
                Logger.Information("Detected a loop in template references at {Context} via {Evidence}", context, string.Join(", ", visitedContexts));
                result = default;
                return false;
            }


            try
            {
                var data = ReadAllText(f);
                var parser = new Parser(new StringReader(data));
                parser.Consume<StreamStart>();
                if (!parser.Accept<DocumentStart>(out _))
                {
                    Logger.Information("YAML structure from {Context} is empty", context);
                    result = default;
                    return false;
                }

                var deserializer = new DeserializerBuilder().Build();
                result = deserializer.Deserialize<MapFragmentParseInfo>(parser);
                if (!string.IsNullOrWhiteSpace(result.Template))
                {
                    if (!TryParseFragment(result.Template, context, out MapFragmentParseInfo mpi, visitedContexts))
                    {
                        result = MapFragmentParseInfo.Merge(mpi, result);
                    }
                }

                processedTemplates[f] = result;
                return true;
            }
            catch (SyntaxErrorException se)
            {
                Log.Error(se, "Failed to parse {Context}", f);
                result = default;
                return false;
            }
        }

        protected virtual string Resolve(string context, string file) => Path.Combine(context, file);

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

                if (!string.IsNullOrWhiteSpace(mapInfo.Template))
                {
                    if (!TryParseFragment(mapInfo.Template, context, out MapFragmentParseInfo mpi, new List<string>()))
                    {
                        mapFragment = default;
                        return false;
                    }

                    mapInfo = MapFragmentParseInfo.Merge(mpi, mapInfo);
                }

                if (mapInfo.Symbols == null || mapInfo.Symbols.Count == 0)
                {
                    Logger.Information("Symbol declaration is mandatory");
                    mapFragment = default;
                    return false;
                }

                IReadOnlyView2D<MapFragmentTagDeclaration> mapData;
                if (mapInfo.Size.Width == 0 && mapInfo.Size.Height == 0)
                {
                    if (!ParseVariableSizeMapData(data.Substring(yamlStart.End.Index), mapInfo, out mapData, out var size))
                    {
                        Logger.Information("Unable to parse map data payload");
                        mapFragment = default;
                        return false;
                    }

                    mapInfo.Size = size;
                }
                else
                {
                    if (mapInfo.Size.Width == 0 || mapInfo.Size.Height == 0)
                    {
                        Logger.Information("Empty map is not allowed");
                        mapFragment = default;
                        return false;
                    }


                    if (!ParseKnownSizeMapData(data.Substring(yamlStart.End.Index), mapInfo, out mapData))
                    {
                        Logger.Information("Unable to parse map data payload");
                        mapFragment = default;
                        return false;
                    }
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

                mapFragment = new MapFragment(id, mapData, mi, new Dimension(mapInfo.Size.Width, mapInfo.Size.Height), new TypedRuleProperties());

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

        static bool ParseKnownSizeMapData(string data, MapFragmentParseInfo info, out IReadOnlyView2D<MapFragmentTagDeclaration> map)
        {
            var width = info.Size.Width;
            var height = info.Size.Height;
            var mapWritable = new BoundedDataView<MapFragmentTagDeclaration>(new Rectangle(0, 0, width, height));
            int y = 0;

            foreach (var line in GetLines(data, true))
            {
                if (y >= height)
                {
                    break;
                }

                var effectiveLength = Math.Min(line.Length, width);
                for (var x = 0; x < effectiveLength; x += 1)
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

        static bool ParseVariableSizeMapData(string data, MapFragmentParseInfo info, out IReadOnlyView2D<MapFragmentTagDeclaration> map, out MapFragmentSize size)
        {
            var mapWritable = new DynamicDataView2D<MapFragmentTagDeclaration>(0, 0, 32, 32);
            int lines = 0;
            int columns = 0;

            foreach (var line in GetLines(data, true))
            {
                columns = Math.Max(columns, line.Length);
                for (var x = 0; x < line.Length; x += 1)
                {
                    var c = line[x];
                    if (!info.Symbols.TryGetValue(c, out var value))
                    {
                        continue;
                    }

                    mapWritable[x, lines] = new MapFragmentTagDeclaration(value);
                }

                lines += 1;
            }

            map = mapWritable;
            size = new MapFragmentSize(columns, lines);
            return true;
        }
    }
}
