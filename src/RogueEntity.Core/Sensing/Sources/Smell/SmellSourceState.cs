using EnTTSharp.Entities.Attributes;
using GoRogue.SenseMapping;

namespace RogueEntity.Core.Sensing.Sources
{
    [EntityComponent]
    public struct SmellSourceState
    {
        public readonly SmartSenseSource SenseSource;

        public SmellSourceState(SmartSenseSource senseSource)
        {
            SenseSource = senseSource;
        }

    }
}