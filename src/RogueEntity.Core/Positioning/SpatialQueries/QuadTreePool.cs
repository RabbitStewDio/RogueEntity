using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Utils.SpatialIndex;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.SpatialQueries;

public class QuadTreePool<TComponent>: ISpatialIndex2DPool<TComponent>
{
    readonly ObjectPool<QuadTree2D<TComponent>> quadTreePool;

    public QuadTreePool(DynamicDataViewConfiguration config, 
                        int maxNodesPerElement = 4,
                        int maxDepth = 0)
    {
        if (maxDepth < 1)
        {
            var x = Math.Min(config.TileSizeX, config.TileSizeY);
            maxDepth = (int) Math.Max(1, Math.Ceiling(Math.Log(x, 2)));
        }

        this.quadTreePool = new DefaultObjectPool<QuadTree2D<TComponent>>(new QuadTreePolicy(config, maxDepth, maxNodesPerElement));
    }

    public class QuadTreePolicy : IPooledObjectPolicy<QuadTree2D<TComponent>>
    {
        readonly ObjectPool<List<FreeListIndex>> freeListPool;
        readonly DynamicDataViewConfiguration config;
        readonly int maxDepth;
        readonly int maxNodesPerElement;

        public QuadTreePolicy(DynamicDataViewConfiguration config, int maxDepth, int maxNodesPerElement)
        {
            this.config = config;
            this.maxDepth = maxDepth;
            this.maxNodesPerElement = maxNodesPerElement;
            this.freeListPool = new DefaultObjectPool<List<FreeListIndex>>(new ListObjectPoolPolicy<FreeListIndex>());
        }

        public QuadTree2D<TComponent> Create()
        {
            return new QuadTree2D<TComponent>(freeListPool, config, maxNodesPerElement, maxDepth);
        }

        public bool Return(QuadTree2D<TComponent> obj)
        {
            obj.Clear();
            return true;
        }
    }

    public ISpatialIndex2D<TComponent> Get()
    {
        return quadTreePool.Get();
    }

    public void Return(ISpatialIndex2D<TComponent> obj)
    {
        if (obj is QuadTree2D<TComponent> q)
        {
            quadTreePool.Return(q);
        }
    }
}