using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using System;
using System.Collections.Generic;

namespace RogueEntity.SadCons.MapRendering
{
    public class BasicMapRenderer: MapRendererBase
    {
        readonly List<RenderMatchRule> renderRules;
        readonly List<MapLayer> layers;
        readonly List<IEntityToTagConverter> cachedMaps;
        readonly List<(MapLayer, WorldEntityTag)> queryResults;
        bool rulesDirty;

        public BasicMapRenderer()
        {
            renderRules = new List<RenderMatchRule>();
            layers = new List<MapLayer>();
            cachedMaps = new List<IEntityToTagConverter>();
            queryResults = new List<(MapLayer, WorldEntityTag)>();
        }

        public bool TryRenderAt(Position pos, out ConsoleRenderData rd)
        {
            queryResults.Clear();
            foreach (var t in cachedMaps)
            {
                if (t.TryFetchTag(pos, out var result))
                {
                    queryResults.Add((t.Layer, result));
                }
            }

            if (queryResults.Count == 0)
            {
                rd = default;
                return false;
            }
            
            if (rulesDirty)
            {
                renderRules.Sort((a,b) => b.Rank.CompareTo(a.Rank));
                rulesDirty = false;
            }
            
            // match 
            foreach (var r in renderRules)
            {
                if (r.TryMatch(queryResults, out rd))
                {
                    return true;
                }
            }

            rd = default;
            return false;
        }

        protected override ConsoleRenderData QueryCell(Position pos)
        {
            if (TryRenderAt(pos, out var data))
            {
                return data;
            }

            return default;
        }

        public void Add(RenderMatchRule rule)
        {
            renderRules.Add(rule);
            rulesDirty = true;
        }
        
        public void DefineRenderLayer(IEntityToTagConverter build)
        {
            if (layers.Contains(build.Layer))
            {
                throw new ArgumentException();
            }

            layers.Add(build.Layer);
            cachedMaps.Add(build);
        }
    }
}
