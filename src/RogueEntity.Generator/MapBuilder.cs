using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
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
        readonly MapLayer layer;
        readonly IItemResolver<TEntity> resolver;
        readonly IGridMapContext<TEntity> gridMapContext;

        public MapBuilderLayer(MapLayer layer, IItemResolver<TEntity> resolver, IGridMapContext<TEntity> gridMapContext)
        {
            this.layer = layer;
            this.resolver = resolver;
            this.gridMapContext = gridMapContext;
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
            if (!gridMapContext.TryGetGridDataFor(layer, out var gridData))
            {
                return false;
            }

            if (gridData.TryGetView(pos.GridZ, out var view))
            {
                var entity = view[pos.GridX, pos.GridY];
                if (postProc != null && resolver.TryResolve(entity, out var decl))
                {
                    if (!postProc.ClearPreProcess(decl.Id, pos, resolver, ref entity))
                    {
                        return false;
                    }
                }
                
                resolver.Destroy(entity);
                return true;
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
        
        public void WithLayer<T>(in MapLayer mapLayer, IItemResolver<T> itemResolver, IGridMapContext<T> gridContext)
            where T : IEntityKey
        {
            if (mapLayer == MapLayer.Indeterminate)
            {
                throw new ArgumentException();
            }
            
            if (!layerProcessors.TryGetValue(mapLayer.LayerId, out var proc))
            {
                layerProcessors.Add(mapLayer.LayerId, new MapBuilderLayer<T>(mapLayer, itemResolver, gridContext));
                mapLayers.Add(mapLayer);
            }
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
