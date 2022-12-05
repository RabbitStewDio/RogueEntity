using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using System.Collections.Generic;

namespace RogueEntity.SadCons.MapRendering
{
    public class ConsoleRenderLayer<TMapData> : IConsoleRenderLayer
        where TMapData : struct, IEntityKey
    {
        readonly IMapDataContext<TMapData> layer;
        readonly IItemResolver<TMapData> itemResolver;
        readonly Dictionary<WorldEntityTag, ConsoleRenderData> rendererForTags;
        readonly BufferList<(TMapData, EntityGridPosition)> buffer;
        
        public ConsoleRenderLayer(IMapDataContext<TMapData> layer,
                                  IItemResolver<TMapData> itemResolver)
        {
            this.layer = layer;
            this.itemResolver = itemResolver;
            this.rendererForTags = new Dictionary<WorldEntityTag, ConsoleRenderData>();
            this.buffer = new BufferList<(TMapData, EntityGridPosition)>();
        }

        public ConsoleRenderLayer<TMapData> WithRenderTemplate(WorldEntityTag tag, ConsoleRenderData cell)
        {
            this.rendererForTags[tag] = cell;
            return this;
        }

        public Optional<ConsoleRenderData> Get(Position p)
        {
            layer.QueryItemTile(EntityGridPosition.From(p), buffer);
            
            foreach (var (item, _) in buffer)
            {
                if (!itemResolver.TryResolve(item, out var itemDeclaration) ||
                    !rendererForTags.TryGetValue(itemDeclaration.Tag, out var cellTemplate))
                {
                    continue;
                }
                return cellTemplate;
            }

            return default;
        }
    }
}
