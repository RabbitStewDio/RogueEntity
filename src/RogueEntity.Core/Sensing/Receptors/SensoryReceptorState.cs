using System;
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
        public static SensoryReceptorState<TSense> Create<TSense>()
            where TSense : ISense
        {
            return new SensoryReceptorState<TSense>(default, SenseSourceDirtyState.UnconditionallyDirty, Position.Invalid);
        }
    }
    
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    public class SensoryReceptorState<TSense>
        where TSense : ISense
    {
        public float LastIntensity { get; private set; }
        public Position LastPosition { get; private set; }
        public Optional<SenseSourceData> SenseSource { get; private set; }
        public SenseSourceDirtyState State { get; private set; }

        public SensoryReceptorState(Optional<SenseSourceData> senseSource, SenseSourceDirtyState state, Position lastPosition)
        {
            SenseSource = senseSource;
            State = state;
            LastPosition = lastPosition;
        }

        public SensoryReceptorState<TSense> WithPosition(Position position)
        {
            if (position == LastPosition)
            {
                return this;
            }

            LastPosition = position;
            return this;
        }

        public SensoryReceptorState<TSense> WithIntensity(float intensity)
        {
            LastIntensity = intensity;
            return this;
        }

        public SensoryReceptorState<TSense> WithSenseState(SenseSourceData state)
        {
            if (state == SenseSource)
            {
                return this;
            }

            SenseSource = state;
            return this;
        }

        public SensoryReceptorState<TSense> WithOutSenseState()
        {
            if (!SenseSource.HasValue)
            {
                return this;
            }

            SenseSource = Optional.Empty<SenseSourceData>();
            return this;
        }

        public SensoryReceptorState<TSense> WithDirtyState(SenseSourceDirtyState p)
        {
            State = p;
            return this;
        }
    }
}