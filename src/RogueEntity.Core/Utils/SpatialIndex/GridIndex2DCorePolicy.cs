using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Utils.SpatialIndex;

public class GridIndex2DCorePolicy : IPooledObjectPolicy<GridIndex2DCore>
{
    readonly DynamicDataViewConfiguration config;

    public GridIndex2DCorePolicy(DynamicDataViewConfiguration config)
    {
        this.config = config;
    }

    public GridIndex2DCore Create()
    {
        return new GridIndex2DCore(config.GetDefaultBounds());
    }

    public bool Return(GridIndex2DCore obj)
    {
        obj.Clear();
        obj.Return();
        return true;
    }
}