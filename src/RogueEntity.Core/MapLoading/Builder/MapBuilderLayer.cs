using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.MapLoading.Builder
{
    public class MapBuilderLayer<TEntity>: IMapBuilderLayer
        where TEntity : IEntityKey
    {
        [UsedImplicitly]
        readonly MapLayer layer;
        readonly IItemResolver<TEntity> resolver;
        readonly IGridMapDataContext<TEntity> gridMapContext;
        readonly IItemPlacementService<TEntity> placementService;

        public MapBuilderLayer(MapLayer layer, 
                               IItemResolver<TEntity> resolver, 
                               IGridMapDataContext<TEntity> gridMapContext,
                               IItemPlacementService<TEntity> placementService)
        {
            this.layer = layer;
            this.resolver = resolver;
            this.gridMapContext = gridMapContext;
            this.placementService = placementService;
        }


        public IItemRegistry ItemRegistry => resolver.ItemRegistry;

        public bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter postProc = null)
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

        public bool Clear(Position pos, IMapBuilderInstantiationLifter postProc = null)
        {
            if (!gridMapContext.TryGetView(pos.GridZ, out var view))
            {
                return false;
            }

            var entity = view[pos.GridX, pos.GridY];
            if (entity.IsEmpty)
            {
                return true;
            }
                
            if (postProc != null && resolver.TryResolve(entity, out var decl))
            {
                if (!postProc.ClearPreProcess(decl.Id, pos, resolver, entity).TryGetValue(out entity))
                {
                    return false;
                }
            }

            if (placementService.TryRemoveItem(entity, pos))
            {
                resolver.Destroy(entity);
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
