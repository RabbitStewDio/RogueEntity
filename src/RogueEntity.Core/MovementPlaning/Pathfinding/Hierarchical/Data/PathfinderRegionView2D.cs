using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class PathfinderRegionView2D : PooledDynamicDataView2D<(TraversableZoneId zone, DirectionalityInformation edge)>
{
    public PathfinderRegionView2D(IBoundedDataViewPool<(TraversableZoneId, DirectionalityInformation)> pool) : base(pool)
    {
    }

    public bool TryGetRegion(int x, int y, [MaybeNullWhen(false)] out PathfinderRegionDataView raw, DataViewCreateMode mode = DataViewCreateMode.Nothing)
    {
        if (TryGetWriteAccess(x, y, out var view, mode))
        {
            raw = (PathfinderRegionDataView)view;
            return true;
        }

        raw = default;
        return false;
    }

    public bool TryGetRegionData(int x, int y, [MaybeNullWhen(false)] out PathfinderRegionDataView raw, out TraversableZoneId zoneId)
    {
        if (TryGetData(x, y, out var view) && view.TryGet(x, y, out var data))
        {
            raw = (PathfinderRegionDataView)view;
            zoneId = data.zone;
            return true;
        }

        zoneId = default;
        raw = default;
        return false;
    }

}