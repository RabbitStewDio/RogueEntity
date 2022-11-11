using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class PathfinderRegionView3D: PooledDynamicDataView3D<(TraversableZoneId, DirectionalityInformation)>
{
    public PathfinderRegionView3D(PathfinderRegionDataViewPool pool) : base(pool)
    {
    }

    protected override PooledDynamicDataView2D<(TraversableZoneId, DirectionalityInformation)> CreateView(IBoundedDataViewPool<(TraversableZoneId, DirectionalityInformation)> pool)
    {
        return new PathfinderRegionView2D(pool);
    }

    public bool TryGetRegionView2D(int z, [MaybeNullWhen(false)] out PathfinderRegionView2D view, DataViewCreateMode createMode = DataViewCreateMode.Nothing)
    {
        if (base.TryGetWritableView(z, out var viewRaw, createMode))
        {
            view = (PathfinderRegionView2D)viewRaw;
            return true;
        }

        view = default;
        return false;
    }

}