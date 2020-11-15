using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning
{
    public static class PositionModuleServices
    {
        public static IGridMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreateGridMapContext<TEntityId>(serviceResolver, new DynamicDataViewConfiguration(0, 0, 32, 32));
        }

        public static IGridMapContext<TEntityId> GetOrCreateGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration defaultConfig)
        {
            if (serviceResolver.TryResolve(out IGridMapContext<TEntityId> map))
            {
                return map;
            }

            if (serviceResolver.TryResolve(out IGridMapConfiguration<TEntityId> mapConfig))
            {
                map = new DefaultGridPositionContextBackend<TEntityId>(mapConfig);
            }
            else if (serviceResolver.TryResolve(out DynamicDataViewConfiguration viewConfig))
            {
                map = new DefaultGridPositionContextBackend<TEntityId>(viewConfig);
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(map);
            }
            else
            {
                map = new DefaultGridPositionContextBackend<TEntityId>(defaultConfig);
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(map);
            }
            
            serviceResolver.Store(map);
            return map;
        }
    }
}