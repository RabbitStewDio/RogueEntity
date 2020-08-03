using EnTTSharp.Annotations;

namespace RogueEntity.Core.Sensing.Vision
{
    [EntityComponent]
    public readonly struct LightSourceData
    {
        public readonly SmartSenseSource SenseSource;
        public readonly bool Enabled;

        public LightSourceData(SmartSenseSource senseSource, bool enabled)
        {
            SenseSource = senseSource;
            Enabled = enabled;
        }

        public LightSourceData WithState(bool enabled)
        {
            return new LightSourceData(SenseSource, enabled);
        }
    }
}