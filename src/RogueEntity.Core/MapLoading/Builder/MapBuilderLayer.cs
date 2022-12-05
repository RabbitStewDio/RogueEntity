using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MapLoading.Builder
{
    public class MapBuilderLayer<TEntity> : IMapBuilderLayer
        where TEntity : struct, IEntityKey
    {
        [UsedImplicitly]
        readonly MapLayer layer;

        readonly IItemResolver<TEntity> resolver;
        readonly IMapDataContext<TEntity> gridMapContext;
        readonly IItemPlacementService<TEntity> placementService;

        public MapBuilderLayer(MapLayer layer,
                               IItemResolver<TEntity> resolver,
                               IMapDataContext<TEntity> gridMapContext,
                               IItemPlacementService<TEntity> placementService)
        {
            this.layer = layer;
            this.resolver = resolver;
            this.gridMapContext = gridMapContext;
            this.placementService = placementService;
        }


        public IItemRegistry ItemRegistry => resolver.ItemRegistry;

        public bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter? postProc = null)
        {
            var entity = resolver.Instantiate(item);
            if (!placementService.TryPlaceItem(entity, pos))
            {
                resolver.DiscardUnusedItem(entity);
                return false;
            }

            if (postProc == null)
            {
                resolver.Apply(entity);
                return true;
            }

            if (postProc.InstantiatePostProcess(item, pos, resolver, entity).TryGetValue(out var e))
            {
                resolver.Apply(e);
                return true;
            }

            resolver.DiscardUnusedItem(entity);
            return false;
        }

        public bool Clear(Position pos, IMapBuilderInstantiationLifter? postProc = null)
        {
            using var buffer = BufferListPool<(TEntity, EntityGridPosition)>.GetPooled();
            var result = gridMapContext.QueryItemTile<EntityGridPosition>(EntityGridPosition.From(pos), buffer);
            if (result.Count == 0)
            {
                return true;
            }

            foreach (var (entity, _) in result.Data)
            {
                if (postProc != null && resolver.TryResolve(entity, out var decl))
                {
                    if (!postProc.ClearPreProcess(decl.Id, pos, resolver, entity).TryGetValue(out _))
                    {
                        return false;
                    }
                }

                if (placementService.TryRemoveItem(entity, pos))
                {
                    resolver.Destroy(entity);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}