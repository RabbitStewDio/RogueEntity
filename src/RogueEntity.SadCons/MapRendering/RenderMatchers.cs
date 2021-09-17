using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using System.Collections.Immutable;

namespace RogueEntity.SadCons.MapRendering
{
    public static class RenderMatchers
    {
        public readonly struct MatchBuilder
        {
            readonly ImmutableDictionary<MapLayer, ImmutableList<WorldEntityTag>> matchers;

            public MatchBuilder(MapLayer l, WorldEntityTag t)
            {
                matchers = ImmutableDictionary.Create<MapLayer, ImmutableList<WorldEntityTag>>()
                                              .Add(l, ImmutableList.Create(t));
            }

            MatchBuilder(ImmutableDictionary<MapLayer, ImmutableList<WorldEntityTag>> matchers)
            {
                this.matchers = matchers;
            }

            public MatchBuilder And(MapLayer l, WorldEntityTag t)
            {
                if (matchers.TryGetValue(l, out var existing))
                {
                    existing = existing.Add(t);
                }
                else
                {
                    existing = ImmutableList.Create(t);
                }

                var m = matchers.SetItem(l, existing);
                return new MatchBuilder(m);
            }

            public RenderMatchRule As(ConsoleRenderData rd) => new RenderMatchRule(rd, matchers);
        }

        public static MatchBuilder Match(MapLayer l, WorldEntityTag t)
        {
            return new MatchBuilder(l, t);
        }
    }
}
