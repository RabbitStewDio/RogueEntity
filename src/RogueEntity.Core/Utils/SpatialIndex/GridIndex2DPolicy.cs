using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Utils.SpatialIndex;

public class GridIndex2DPolicy<T>: IPooledObjectPolicy<GridIndex2D<T>>
{
    readonly DynamicDataViewConfiguration config;
    readonly ObjectPool<GridIndex2DCore> pool;

    public GridIndex2DPolicy(DynamicDataViewConfiguration config, 
                             ObjectPool<GridIndex2DCore>? pool = null)
    {
        this.config = config;
        this.pool = pool ?? new DefaultObjectPool<GridIndex2DCore>(new GridIndex2DCorePolicy(config));
    }

    public GridIndex2D<T> Create()
    {
        return new GridIndex2D<T>(pool, config);
    }

    public bool Return(GridIndex2D<T> obj)
    {
        obj.Clear();
        return true;
    }
}