using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    public class PathfinderRegionDataViewPool : IBoundedDataViewPool<(TraversableZoneId, DirectionalityInformation)>
    {
        readonly ObjectPool<PathfinderRegionDataView> pool;
        readonly ObjectPool<List<PathfinderRegionEdge>> listPool;
        
        public PathfinderRegionDataViewPool(DynamicDataViewConfiguration config,
                                            ObjectPoolProvider? poolProvider = null)
        {
            if (poolProvider == null)
            {
                var provider = new DefaultObjectPoolProvider();
                provider.MaximumRetained = 512;
                poolProvider = provider;
            }

            this.listPool = poolProvider.Create(new ListObjectPoolPolicy<PathfinderRegionEdge>());
            
            this.pool = poolProvider.Create(new Policy(listPool, config));
            this.TileConfiguration = config;
        }

        public DynamicDataViewConfiguration TileConfiguration { get; }

        public IPooledBoundedDataView<(TraversableZoneId, DirectionalityInformation)> Lease(Rectangle bounds, long time)
        {
            var result = this.pool.Get();
            result.IsDirty = true;
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
            readonly ObjectPool<List<PathfinderRegionEdge>> objectPool;
            readonly DynamicDataViewConfiguration config;

            public Policy(ObjectPool<List<PathfinderRegionEdge>> objectPool, DynamicDataViewConfiguration config)
            {
                this.objectPool = objectPool;
                this.config = config;
            }

            public override PathfinderRegionDataView Create()
            {
                return new PathfinderRegionDataView(objectPool, new Rectangle(0, 0, config.TileSizeX, config.TileSizeY), 0);
            }

            public override bool Return(PathfinderRegionDataView obj)
            {
                obj.Clear();
                return true;
            }
        }
    }
}