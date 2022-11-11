using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class PathfinderRegionDataViewPool : IBoundedDataViewPool<(TraversableZoneId, DirectionalityInformation)>
{
    readonly ObjectPool<PathfinderRegionDataView> pool;
        
    public PathfinderRegionDataViewPool(DynamicDataViewConfiguration config,
                                        ObjectPoolProvider? poolProvider = null)
    {
        poolProvider ??= new DefaultObjectPoolProvider
        {
            MaximumRetained = 512
        };
            
        this.pool = poolProvider.Create(new Policy(config));
        this.TileConfiguration = config;
    }

    public DynamicDataViewConfiguration TileConfiguration { get; }

    public IPooledBoundedDataView<(TraversableZoneId, DirectionalityInformation)> Lease(Rectangle bounds, long time)
    {
        var result = this.pool.Get();
        result.ClearData();
        result.Resize(bounds, true);
        result.BeginUseTimePeriod(time);
        return result;
    }

    public void Return(IPooledBoundedDataView<(TraversableZoneId, DirectionalityInformation)> leased)
    {
        if (leased is PathfinderRegionDataView leasedDefault)
        {
            leasedDefault.Clear();
            pool.Return(leasedDefault);
        }
    }

    class Policy : PooledObjectPolicy<PathfinderRegionDataView>
    {
        readonly DynamicDataViewConfiguration config;

        public Policy(DynamicDataViewConfiguration config)
        {
            this.config = config;
        }

        public override PathfinderRegionDataView Create()
        {
            return new PathfinderRegionDataView(new Rectangle(config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY), 0);
        }

        public override bool Return(PathfinderRegionDataView obj)
        {
            obj.Clear();
            return true;
        }
    }
}