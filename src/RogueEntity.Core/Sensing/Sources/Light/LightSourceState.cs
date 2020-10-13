using EnTTSharp.Entities.Attributes;
using GoRogue.SenseMapping;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    /// <summary>
    ///   Volatile state that is not transferred over the network or saved to disk.
    /// </summary>
    [EntityComponent]
    public readonly struct LightSourceState
    {
        public readonly Position LastPosition;
        public readonly SmartSenseSource SenseSource;
        public readonly bool LastSeenAsActive;

        public LightSourceState(SmartSenseSource senseSource)
        {
            SenseSource = senseSource;
            LastSeenAsActive = false;
            LastPosition = default;
        }

        public LightSourceState(SmartSenseSource senseSource, bool lastSeenAsActive, Position lastPosition)
        {
            SenseSource = senseSource;
            LastSeenAsActive = lastSeenAsActive;
            LastPosition = lastPosition;
        }

        public LightSourceState WithLastSeenAsActive(Position p)
        {
            return new LightSourceState(SenseSource, true, p);
        }
        
        public LightSourceState WithLastSeenAsNotActive()
        {
            return new LightSourceState(SenseSource, false, Position.Invalid);
        }
    }
}