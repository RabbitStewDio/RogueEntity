using System;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Cache
{
    public class SenseCacheSetUpSystem<TGameContext, TItemId>
        where TGameContext: IGridMapContext<TGameContext, TItemId>
    {
        readonly Lazy<SenseStateCacheProvider> cacheProvider;
        readonly MapLayer[] layers;

        public SenseCacheSetUpSystem([NotNull] Lazy<SenseStateCacheProvider> cacheProvider, params MapLayer[] layers)
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            this.layers = layers;
        }

        public void Start(TGameContext context)
        {
            foreach (var l in layers)
            {
                if (context.TryGetGridDataFor(l, out var data))
                {
                    data.PositionDirty += OnDirty;
                }
            }
        }
        
        public void Stop(TGameContext context)
        {
            foreach (var l in layers)
            {
                if (context.TryGetGridDataFor(l, out var data))
                {
                    data.PositionDirty -= OnDirty;
                }
            }
        }

        void OnDirty(object o, PositionDirtyEventArgs args)
        {
            cacheProvider.Value.MarkDirty(args.Position);
        }
    }
}