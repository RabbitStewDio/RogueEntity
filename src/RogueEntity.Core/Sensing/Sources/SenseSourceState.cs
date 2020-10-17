using EnTTSharp.Entities.Attributes;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Sources
{
    /// <summary>
    ///   Volatile state that is not transferred over the network or saved to disk.
    /// </summary>
    [EntityComponent]
    public class SenseSourceState<TSense>
    {
        public Position LastPosition { get; private set; }
        public Optional<SenseSourceData> SenseSource { get; private set; }
        public SenseSourceDirtyState State { get; private set; }

        public SenseSourceState(Optional<SenseSourceData> senseSource, SenseSourceDirtyState state, Position lastPosition)
        {
            SenseSource = senseSource;
            State = state;
            LastPosition = lastPosition;
        }

        public SenseSourceState<TSense> WithPosition(Position position)
        {
            if (position == LastPosition)
            {
                return this;
            }

            LastPosition = position;
            return this;
        }
        
        public SenseSourceState<TSense> WithSenseState(SenseSourceData state)
        {
            if (state == SenseSource)
            {
                return this;
            }

            SenseSource = state;
            return this;
        }
        
        public SenseSourceState<TSense> WithOutSenseState()
        {
            if (!SenseSource.HasValue)
            {
                return this;
            }

            SenseSource = Optional.Empty<SenseSourceData>();
            return this;
        }
        
        public SenseSourceState<TSense> WithDirtyState(SenseSourceDirtyState p)
        {
            State = p; 
            return this;
        }
    }
}