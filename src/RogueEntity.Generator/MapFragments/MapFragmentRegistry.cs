using RogueEntity.Api.Utils;
using Serilog;
using System.Collections.Generic;
using System.IO;

namespace RogueEntity.Generator.MapFragments
{
    public class MapFragmentRegistry
    {
        readonly ILogger logger = SLog.ForContext<MapFragmentRegistry>();
        readonly List<MapFragment> fragments;

        public MapFragmentRegistry()
        {
            fragments = new List<MapFragment>();
        }

        public void LoadAll(MapFragmentParser parser, string path)
        {
            path = Path.GetFullPath(path);
            logger.Debug("Loading map fragments from {Path}", path);
            var files = Directory.GetFiles(path, "*.tilemap", SearchOption.AllDirectories);
            
            foreach (var f in files)
            {
                if (parser.TryParseFromFile(f, out var frag))
                {
                    logger.Debug("Loaded map fragment {File}", f);
                    Add(frag);
                }
                else
                {
                    logger.Warning("Failed to load map fragment {File}", f);
                }
            }
        }

        public void Add(MapFragment mf)
        {
            fragments.Add(mf);

            var mirrorModifier = mf.Info.QueryMapFragmentMirror();
            if (mirrorModifier == MapFragmentMirror.Both)
            {
                var m = mf.MirrorHorizontally();
                fragments.Add(m);
                fragments.Add(m.MirrorVertically());
                fragments.Add(mf.MirrorVertically());
            }
            else if (mirrorModifier == MapFragmentMirror.Horizontal)
            {
                var m = mf.MirrorHorizontally();
                fragments.Add(m);
            }
            else if (mirrorModifier == MapFragmentMirror.Vertical)
            {
                var m = mf.MirrorVertically();
                fragments.Add(m);
            }
        }

        public ReadOnlyListWrapper<MapFragment> Items => fragments;
    }
}