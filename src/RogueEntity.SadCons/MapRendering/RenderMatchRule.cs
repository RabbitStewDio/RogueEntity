using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace RogueEntity.SadCons.MapRendering
{
    public class RenderMatchRule
    {
        readonly ImmutableDictionary<MapLayer, ImmutableList<WorldEntityTag>> matchers;
        readonly ConsoleRenderData result;

        public RenderMatchRule(ConsoleRenderData result, ImmutableDictionary<MapLayer, ImmutableList<WorldEntityTag>> matchers)
        {
            this.result = result;
            this.matchers = matchers;

            if (matchers.Count == 0)
            {
                throw new ArgumentException();
            }
            
            var rank = 0;
            foreach (var k in matchers.Keys)
            {
                rank = Math.Max(rank, k.LayerId);
            }
            
            this.Rank = rank << 8 | matchers.Count;
        }

        public int Rank { get; }
        
        public bool TryMatch(List<(MapLayer, WorldEntityTag)> queryResults, out ConsoleRenderData rd)
        {
            if (queryResults.Count == 0)
            {
                rd = default;
                return false;
            }
            
            foreach (var (layer, tag) in queryResults)
            {
                if (!matchers.TryGetValue(layer, out var tags))
                {
                    continue;
                }

                if (!IsAnyMatch(tags, tag))
                {
                    rd = default;
                    return false;
                }
            }
            
            rd = result;
            return true;
        }

        bool IsAnyMatch(ImmutableList<WorldEntityTag> expectedTags, WorldEntityTag tag)
        {
            foreach (var t in expectedTags)
            {
                if (t == tag)
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(nameof(RenderMatchRule));
            sb.AppendLine($"({Rank})");
            foreach (var m in matchers)
            {
                sb.Append("  ");
                sb.AppendLine(m.Key.ToString());
                foreach (var x in m.Value)
                {
                    sb.AppendLine($"      {x}");
                }
            }
            return sb.ToString();
        }
    }
}
