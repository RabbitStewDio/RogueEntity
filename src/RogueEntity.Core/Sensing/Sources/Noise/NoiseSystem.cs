using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using GoRogue.SenseMapping;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSystem<TGameContext>
    {
        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<ISenseStateCacheProvider> senseCacheProvider;
        readonly INoisePhysicsContext physicsConfiguration;
        readonly ISenseSourceBlitterFactory blitterFactory;
        readonly Dictionary<int, NoisesPerLevel> noiseSources;

        public NoiseSystem(Lazy<ISensePropertiesSource> senseProperties,
                           Lazy<ISenseStateCacheProvider> senseCacheProvider,
                           INoisePhysicsContext physicsConfiguration, 
                           ISenseSourceBlitterFactory blitterFactory)
        {
            this.physicsConfiguration = physicsConfiguration;
            this.senseProperties = senseProperties;
            this.senseCacheProvider = senseCacheProvider;
            this.blitterFactory = blitterFactory;
            this.noiseSources = new Dictionary<int, NoisesPerLevel>();
        }

        public void ClearNoiseData(TGameContext ctx)
        {
        }

        public void CollectNoiseData<TItemId, TPosition>(IEntityViewControl<TItemId> v,
                                                         TGameContext context,
                                                         TItemId k,
                                                         in NoiseSourceState state,
                                                         in TPosition position)
            where TItemId : IEntityKey
            where TPosition: IPosition
        {
            if (v.GetComponent<NoiseSoundClip>(k, out var clip) &&
                !position.IsInvalid)
            {
                state.SenseSource.Enabled = true;
                state.SenseSource.Intensity = clip.Intensity;
                state.SenseSource.UpdatePosition(position.GridX, position.GridY);
                state.SenseSource.UpdateStrength(physicsConfiguration.NoiseSignalRadiusForIntensity(clip.Intensity), clip.Intensity);

                var pos = Position.From(position);
                if (!state.LastSeenAsActive ||
                    state.LastPosition != pos)
                {
                    state.SenseSource.UpdatePosition(pos.GridX, pos.GridY);
                    var nstate = state.WithLastSeenAsActive(pos);
                    v.WriteBack(k, in nstate);
                    AddNoise(pos.GridZ, in nstate);
                }
                else
                {
                    AddNoise(pos.GridZ, in state);
                }
            }
            else if (state.LastSeenAsActive)
            {
                // Light has been disabled since the last calculation.

                var nstate = state.WithLastSeenAsNotActive();
                nstate.SenseSource.Enabled = false;
                v.WriteBack(k, in nstate);
                
                var pos = Position.From(position);
                AddNoise(pos.GridZ, in nstate);
            }
        }

        public void ComputeLights(TGameContext context)
        {
            foreach (var v in noiseSources.Values)
            {
                v.Compute();
            }
        }

        void AddNoise(int level, in NoiseSourceState state)
        {
            if (!noiseSources.TryGetValue(level, out var noiseSource))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                noiseSource = new NoisesPerLevel(senseData, senseCacheProvider.Value, blitterFactory);
                noiseSources[level] = noiseSource;
            }

            noiseSource.Add(in state);
        }

        public class NoisesPerLevel
        {
            readonly IReadOnlyMapData<SenseProperties> senseData;
            readonly ISenseStateCacheProvider senseStateCacheProvider;
            readonly ISenseSourceBlitterFactory senseSourceBlitterFactory;

            public NoisesPerLevel(IReadOnlyMapData<SenseProperties> senseData, 
                                  ISenseStateCacheProvider senseStateCacheProvider, 
                                  ISenseSourceBlitterFactory senseSourceBlitterFactory)
            {
                this.senseData = senseData;
                this.senseStateCacheProvider = senseStateCacheProvider;
                this.senseSourceBlitterFactory = senseSourceBlitterFactory;
            }

            public void Add(in NoiseSourceState state)
            {
                
            }

            public void Compute()
            {
                
            }
        }
    }
}