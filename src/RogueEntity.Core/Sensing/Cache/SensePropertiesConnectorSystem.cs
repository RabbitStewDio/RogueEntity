using System;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Cache
{
    public class SensePropertiesConnectorSystem<TGameContext, TSense>
    {
        readonly SensePropertiesSystem<TGameContext, TSense> sensePropertiesSystem;
        readonly SenseStateCache cache;

        public SensePropertiesConnectorSystem([NotNull] SensePropertiesSystem<TGameContext, TSense> sensePropertiesSystem,
                                              [NotNull] SenseStateCache cache)
        {
            this.sensePropertiesSystem = sensePropertiesSystem ?? throw new ArgumentNullException(nameof(sensePropertiesSystem));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void Start(TGameContext context)
        {
            this.sensePropertiesSystem.SenseResistancePositionDirty += OnSensePropertiesDirty;
        }
        
        public void Stop(TGameContext context)
        {
            this.sensePropertiesSystem.SenseResistancePositionDirty += OnSensePropertiesDirty;
        }

        void OnSensePropertiesDirty(object sender, PositionDirtyEventArgs e)
        {
            cache.MarkAllSensesDirty(e.Position);
        }
    }
}