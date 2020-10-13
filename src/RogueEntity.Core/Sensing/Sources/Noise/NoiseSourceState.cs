using EnTTSharp.Entities.Attributes;
using GoRogue.SenseMapping;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Sources
{
    [EntityComponent]
    public readonly struct NoiseSourceState
    {
        public readonly Position LastPosition;
        public readonly SmartSenseSource SenseSource;
        public readonly bool LastSeenAsActive;

        public NoiseSourceState(SmartSenseSource senseSource)
        {
            SenseSource = senseSource;
            LastSeenAsActive = false;
            LastPosition = default;
        }

        public NoiseSourceState(SmartSenseSource senseSource, bool lastSeenAsActive, Position lastPosition)
        {
            SenseSource = senseSource;
            LastSeenAsActive = lastSeenAsActive;
            LastPosition = lastPosition;
        }

        public NoiseSourceState WithLastSeenAsActive(Position p)
        {
            return new NoiseSourceState(SenseSource, true, p);
        }
        
        public NoiseSourceState WithLastSeenAsNotActive()
        {
            return new NoiseSourceState(SenseSource, false, Position.Invalid);
        }
    }
}