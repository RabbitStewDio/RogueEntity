using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Cache
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public class SensePropertiesConnectorSystem<TSense>
    {
        readonly IAggregationLayerSystem<float> sensePropertiesSystem;
        readonly SenseStateCache cache;

        public SensePropertiesConnectorSystem([NotNull] IAggregationLayerSystem<float> sensePropertiesSystem,
                                              [NotNull] SenseStateCache cache)
        {
            this.sensePropertiesSystem = sensePropertiesSystem ?? throw new ArgumentNullException(nameof(sensePropertiesSystem));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void Start()
        {
            this.sensePropertiesSystem.PositionDirty += OnSensePropertiesDirty;
        }

        public void Stop()
        {
            this.sensePropertiesSystem.PositionDirty += OnSensePropertiesDirty;
        }

        void OnSensePropertiesDirty(object sender, PositionDirtyEventArgs e)
        {
            cache.MarkAllSensesDirty(e.Position);
        }
    }
}
