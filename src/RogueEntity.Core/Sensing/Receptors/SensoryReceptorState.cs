using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Receptors
{
    public static class SensoryReceptorState
    {
        public static SensoryReceptorState<TReceptorSense, TSourceSense> Create<TReceptorSense, TSourceSense>()
            where TReceptorSense : ISense
            where TSourceSense : ISense
        {
            return new SensoryReceptorState<TReceptorSense, TSourceSense>(default, SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid, 0);
        }
    }
    
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    public class SensoryReceptorState<TReceptorSense, TSourceSense>
        where TReceptorSense : ISense
        where TSourceSense : ISense
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public Optional<SenseSourceData> SenseSource { get; private set; }
        [Key(1)]
        [DataMember(Order = 1)]
        public SenseSourceDirtyState State { get; private set; }
        [Key(2)]
        [DataMember(Order = 2)]
        public Position LastPosition { get; private set; }
        [Key(3)]
        [DataMember(Order = 3)]
        public float LastIntensity { get; private set; }

        public SensoryReceptorState(Optional<SenseSourceData> senseSource, SenseSourceDirtyState state, Position lastPosition, float lastIntensity)
        {
            LastIntensity = lastIntensity;
            SenseSource = senseSource;
            State = state;
            LastPosition = lastPosition;
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithPosition(Position position)
        {
            if (position == LastPosition)
            {
                return this;
            }

            LastPosition = position;
            return this;
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithIntensity(float intensity)
        {
            LastIntensity = intensity;
            return this;
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithSenseState(SenseSourceData state)
        {
            if (state == SenseSource)
            {
                return this;
            }

            SenseSource = state;
            return this;
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithOutSenseState()
        {
            if (!SenseSource.HasValue)
            {
                return this;
            }

            SenseSource = Optional.Empty<SenseSourceData>();
            return this;
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithDirtyState(SenseSourceDirtyState p)
        {
            State = p;
            return this;
        }
    }
}