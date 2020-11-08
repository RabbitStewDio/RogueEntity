using System;
using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Resistance;

namespace RogueEntity.Core.Sensing.Cache
{
    public class SensePropertiesConnectorSystem<TGameContext, TSense>
    {
        readonly IAggregationLayerSystem<TGameContext, SensoryResistance<TSense>> sensePropertiesSystem;
        readonly SenseStateCache cache;

        public SensePropertiesConnectorSystem([NotNull] IAggregationLayerSystem<TGameContext, SensoryResistance<TSense>> sensePropertiesSystem,
                                              [NotNull] SenseStateCache cache)
        {
            this.sensePropertiesSystem = sensePropertiesSystem ?? throw new ArgumentNullException(nameof(sensePropertiesSystem));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void Start(TGameContext context)
        {
            this.sensePropertiesSystem.PositionDirty += OnSensePropertiesDirty;
        }
        
        public void Stop(TGameContext context)
        {
            this.sensePropertiesSystem.PositionDirty += OnSensePropertiesDirty;
        }

        void OnSensePropertiesDirty(object sender, PositionDirtyEventArgs e)
        {
            cache.MarkAllSensesDirty(e.Position);
        }
    }
}