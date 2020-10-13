using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using GoRogue.MapViews;
using GoRogue.SenseMapping;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    /// <summary>
    ///   Performs light calculations by collecting all active lights and aggregating their affected areas into a global brightness map.
    /// </summary>
    public class LightSystem : IBrightnessSource
    {
        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<ISenseStateCacheProvider> senseCacheProvider;
        readonly ILightPhysicsConfiguration lightPhysicsConfiguration;
        readonly ISenseSourceBlitterFactory blitterFactory;
        readonly Dictionary<int, LightDataPerLevel> activeLightsPerLevel;

        public LightSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                           [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                           [NotNull] ILightPhysicsConfiguration lightPhysicsConfiguration,
                           ISenseSourceBlitterFactory blitterFactory = null)
        {
            this.senseProperties = senseProperties ?? throw new ArgumentNullException(nameof(senseProperties));
            this.senseCacheProvider = senseCacheProvider ?? throw new ArgumentNullException(nameof(senseCacheProvider));
            this.lightPhysicsConfiguration = lightPhysicsConfiguration ?? throw new ArgumentNullException(nameof(lightPhysicsConfiguration));
            
            this.blitterFactory = blitterFactory ?? new DefaultSenseSourceBlitterFactory();
            this.activeLightsPerLevel = new Dictionary<int, LightDataPerLevel>();
        }

        void AddLight(int level, in LightSourceState light)
        {
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new LightDataPerLevel(senseData, senseCacheProvider.Value, blitterFactory);
                activeLightsPerLevel[level] = lights;
            }

            lights.Add(light);
        }

        public bool TryGetLightData(int z, out IBrightnessView brightnessMap)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var data))
            {
                brightnessMap = data;
                return true;
            }

            brightnessMap = default;
            return false;
        }

        public void ResetCollectedLights<TGameContext>(TGameContext context)
        {
            foreach (var l in activeLightsPerLevel)
            {
                l.Value.Clear();
            }
        }

        public void CollectLights<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                         TGameContext context,
                                                         TItemId k,
                                                         in LightSourceData data,
                                                         in LightSourceState state,
                                                         in EntityGridPosition pos)
            where TItemId : IEntityKey
        {
            CollectLightsImpl(v, context, k, in data, in state, in pos);
        }

        public void CollectLights<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                         TGameContext context,
                                                         TItemId k,
                                                         in LightSourceData data,
                                                         in LightSourceState state,
                                                         in ContinuousMapPosition pos)
            where TItemId : IEntityKey
        {
            CollectLightsImpl(v, context, k, in data, in state, in pos);
        }

        protected virtual void CollectLightsImpl<TItemId, TGameContext, TPosition>(IEntityViewControl<TItemId> v,
                                                                               TGameContext context,
                                                                               TItemId k,
                                                                               in LightSourceData data,
                                                                               in LightSourceState state,
                                                                               in TPosition pos)
            where TItemId : IEntityKey
            where TPosition : IPosition
        {
            var radius = lightPhysicsConfiguration.LightSignalRadiusForIntensity(data.Intensity);
            if (data.Enabled)
            {
                state.SenseSource.UpdateStrength(radius, data.Intensity);
                var position = Position.From(pos);
                if (state.LastPosition != position ||
                    state.LastSeenAsActive == false)
                {
                    state.SenseSource.UpdatePosition(pos.GridX, pos.GridY);
                    var nstate = state.WithLastSeenAsActive(position);
                    v.WriteBack(k, in nstate);
                    AddLight(pos.GridZ, in nstate);
                }
                else
                {
                    AddLight(pos.GridZ, in state);
                }
            }
            else if (state.LastSeenAsActive)
            {
                // Light has been disabled since the last calculation.

                var nstate = state.WithLastSeenAsNotActive();
                v.WriteBack(k, in nstate);
                AddLight(pos.GridZ, in nstate);
            }

            // lights that are not enabled and have not been enabled in the last
            // turn can be safely ignored.
        }

        public void ComputeLights<TGameContext>(TGameContext context)
        {
            foreach (var v in activeLightsPerLevel.Values)
            {
                v.Compute();
            }
        }

        class BlockVisionMap : IMapView<float>
        {
            readonly IReadOnlyMapData<SenseProperties> cellProperties;

            public BlockVisionMap(IReadOnlyMapData<SenseProperties> cellProperties)
            {
                this.cellProperties = cellProperties;
            }

            public int Height => cellProperties.Height;
            public int Width => cellProperties.Width;

            public float this[int x, int y] => cellProperties[x, y].blocksLight;
        }

        class LightDataPerLevel : IBrightnessView
        {
            readonly ISenseStateCacheProvider cacheProvider;
            readonly List<LightSourceState> sources;
            readonly SenseMap<SmartSenseSource> senseMap;

            public LightDataPerLevel(IReadOnlyMapData<SenseProperties> resistanceMap,
                                     ISenseStateCacheProvider cacheProvider,
                                     ISenseSourceBlitterFactory blitterFactory = null)
            {
                this.cacheProvider = cacheProvider;
                sources = new List<LightSourceState>();
                senseMap = new SenseMap<SmartSenseSource>(new BlockVisionMap(resistanceMap), blitterFactory);
            }

            public void Add(LightSourceState light)
            {
                sources.Add(light);
            }

            public void Clear()
            {
                sources.Clear();
            }

            public float this[int x, int y]
            {
                get
                {
                    if (x < 0) return 0;
                    if (y < 0) return 0;
                    if (x >= senseMap.Width) return 0;
                    if (y >= senseMap.Height) return 0;

                    return senseMap[x, y];
                }
            }

            public void Compute()
            {
                var dirty = false;
                cacheProvider.TryGetSenseCache<VisionSense>(out var dirtyMap);

                senseMap.ClearSenseSources();

                foreach (var s in sources)
                {
                    if (!s.LastSeenAsActive)
                    {
                        dirty = true;
                    }
                    else
                    {
                        senseMap.AddSenseSource(s.SenseSource);
                        dirty |= dirtyMap?.IsDirty(s.LastPosition, s.SenseSource.Radius) ?? true;
                    }
                }

                if (!dirty)
                {
                    return;
                }

                senseMap.Calculate();
            }
        }
    }
}