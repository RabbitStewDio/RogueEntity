using EnTTSharp;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;

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
    public readonly struct SensoryReceptorState<TReceptorSense, TSourceSense>
        where TReceptorSense : ISense
        where TSourceSense : ISense
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public Optional<SenseSourceData> SenseSource { get; }
        [Key(1)]
        [DataMember(Order = 1)]
        public SenseSourceDirtyState State { get;  }
        [Key(2)]
        [DataMember(Order = 2)]
        public Position LastPosition { get; }
        [Key(3)]
        [DataMember(Order = 3)]
        public float LastIntensity { get; }

        public SensoryReceptorState(Optional<SenseSourceData> senseSource, SenseSourceDirtyState state, Position lastPosition, float lastIntensity)
        {
            LastIntensity = lastIntensity;
            SenseSource = senseSource;
            State = state;
            LastPosition = lastPosition;
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithoutPosition()
        {
            if (LastPosition.IsInvalid)
            {
                return this;
            }

            return new SensoryReceptorState<TReceptorSense, TSourceSense>(SenseSource, State, Position.Invalid, LastIntensity);
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithPosition(Position position)
        {
            if (position == LastPosition)
            {
                return this;
            }

            return new SensoryReceptorState<TReceptorSense, TSourceSense>(SenseSource, State, position, LastIntensity);
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithIntensity(float intensity)
        {
            return new SensoryReceptorState<TReceptorSense, TSourceSense>(SenseSource, State, LastPosition, intensity);
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithSenseState(SenseSourceData state)
        {
            if (state == SenseSource)
            {
                return this;
            }

            return new SensoryReceptorState<TReceptorSense, TSourceSense>(state, State, LastPosition, LastIntensity);
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithOutSenseState()
        {
            if (!SenseSource.HasValue)
            {
                return this;
            }

            return new SensoryReceptorState<TReceptorSense, TSourceSense>(Optional.Empty(), State, LastPosition, LastIntensity);
        }

        public SensoryReceptorState<TReceptorSense, TSourceSense> WithDirtyState(SenseSourceDirtyState p)
        {
            return new SensoryReceptorState<TReceptorSense, TSourceSense>(SenseSource, p, LastPosition, LastIntensity);
        }
    }
}