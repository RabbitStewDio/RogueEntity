using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using System;
using System.Collections.Generic;

namespace RogueEntity.Generator
{
    public interface IMapBuilderInstantiationLifter
    {
        public bool ClearPreProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, ref TEntity entityKey) where TEntity: IEntityKey;
        public bool InstantiatePostProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, ref TEntity entityKey) where TEntity: IEntityKey;
    }
    
    public abstract class EntityMapBuilderLayer
    {
        public abstract IItemRegistry ItemRegistry { get; }
        public abstract bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter postProc = null);
        public abstract bool Clear(Position pos, IMapBuilderInstantiationLifter postProc = null);
    }

    public class MapBuilderLayer<TEntity>: EntityMapBuilderLayer
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


        public override IItemRegistry ItemRegistry => resolver.ItemRegistry;

        public override bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter postProc = null)
        {
            var entity = resolver.Instantiate(item);
            if (!resolver.TryUpdateData(entity, pos, out var changedEntity))
            {
                resolver.DiscardUnusedItem(entity);
                return false;
            }

            if (postProc == null || postProc.InstantiatePostProcess(item, pos, resolver, ref changedEntity))
            {
                resolver.Apply(changedEntity);
            }
            return true;
        }

        public override bool Clear(Position pos, IMapBuilderInstantiationLifter postProc = null)
        {
            if (gridMapContext.TryGetView(pos.GridZ, out var view))
            {
                var entity = view[pos.GridX, pos.GridY];
                if (entity.IsEmpty)
                {
                    return true;
                }
                
                if (postProc != null && resolver.TryResolve(entity, out var decl))
                {
                    if (!postProc.ClearPreProcess(decl.Id, pos, resolver, ref entity))
                    {
                        return false;
                    }
                }

                if (placementService.TryRemoveItem(entity, pos))
                {
                    resolver.Destroy(entity);
                    return true;
                }
            }

            return false;
        }
    }
    
    public class MapBuilder
    {
        readonly Dictionary<byte, EntityMapBuilderLayer> layerProcessors;
        readonly List<MapLayer> mapLayers;

        public MapBuilder()
        {
            mapLayers = new List<MapLayer>();
            layerProcessors = new Dictionary<byte, EntityMapBuilderLayer>();

        }

        public bool TryGetItemRegistry(MapLayer layer, out IItemRegistry reg)
        {
            if (layerProcessors.TryGetValue(layer.LayerId, out var layerData))
            {
                reg = layerData.ItemRegistry;
                return true;
            }

            reg = default;
            return false;
        }

        public MapBuilder WithLayer<T>(in MapLayer mapLayer, IServiceResolver r)
            where T : IEntityKey
        {
            return WithLayer(mapLayer, r.Resolve<IItemResolver<T>>(), r.Resolve<IGridMapContext<T>>(), r.Resolve<IItemPlacementServiceContext<T>>());
        }
        
        public MapBuilder WithLayer<T>(in MapLayer mapLayer, 
                                       IItemResolver<T> itemResolver, 
                                       IGridMapContext<T> gridContext,
                                       IItemPlacementServiceContext<T> placementService)
            where T : IEntityKey
        {
            if (mapLayer == MapLayer.Indeterminate)
            {
                throw new ArgumentException();
            }
            
            if (!layerProcessors.TryGetValue(mapLayer.LayerId, out _) && 
                gridContext.TryGetGridDataFor(mapLayer, out var gridData) && 
                placementService.TryGetItemPlacementService(mapLayer, out var ps))
            {
                layerProcessors.Add(mapLayer.LayerId, new MapBuilderLayer<T>(mapLayer, itemResolver, gridData, ps));
                mapLayers.Add(mapLayer);
            }

            return this;
        }

        public ReadOnlyListWrapper<MapLayer> Layers => mapLayers;

        public bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter postProcessor = null)
        {
            if (pos.IsInvalid)
            {
                return false;
            }

            if (!layerProcessors.TryGetValue(pos.LayerId, out var layer))
            {
                return false;
            }

            return layer.Instantiate(item, pos, postProcessor);

        }

        public bool Clear(Position pos, IMapBuilderInstantiationLifter postProcessor = null)
        {
            if (pos.IsInvalid)
            {
                return false;
            }

            if (!layerProcessors.TryGetValue(pos.LayerId, out var layer))
            {
                return false;
            }

            return layer.Clear(pos, postProcessor);
        }
    }
}
