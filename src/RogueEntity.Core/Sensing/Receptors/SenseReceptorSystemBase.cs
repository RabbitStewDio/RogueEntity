using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class SenseReceptorSystemBase<TReceptorSense, TSourceSense>
        where TReceptorSense : ISense
    {
        static readonly ILogger Logger = SLog.ForContext<SenseReceptorSystemBase<TReceptorSense, TSourceSense>>();
        const int ZLayerTimeToLive = 50;

        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<ISenseStateCacheProvider> senseCacheProvider;
        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly ISensePropagationAlgorithm sensePropagationAlgorithm;
        readonly List<int> zLevelBuffer;
        readonly ISensePhysics physics;
        
        Optional<ISenseStateCacheView> cacheView;
        int currentTime;

        public SenseReceptorSystemBase([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                       [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                       [NotNull] ISensePhysics physics,
                                       [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm)
        {
            this.sensePropagationAlgorithm = sensePropagationAlgorithm ?? throw new ArgumentNullException(nameof(sensePropagationAlgorithm));
            this.senseProperties = senseProperties ?? throw new ArgumentNullException(nameof(senseProperties));
            this.senseCacheProvider = senseCacheProvider ?? throw new ArgumentNullException(nameof(senseCacheProvider));
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));

            this.activeLightsPerLevel = new Dictionary<int, SenseDataLevel>();
            this.zLevelBuffer = new List<int>();
        }

        protected bool TryGetLevel(int z, out SenseDataLevel level) => activeLightsPerLevel.TryGetValue(z, out level);

        /// <summary>
        ///   To be called once during initialization.
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TGameContext"></typeparam>
        public void EnsureSenseCacheAvailable<TGameContext>(TGameContext context)
        {
            var provider = senseCacheProvider.Value;
            if (!provider.TryGetSenseCache<TReceptorSense>(out var cache))
            {
                cacheView = Optional.ValueOf(cache);
            }
            else
            {
                Logger.Verbose("This light system will not react to map changes");
            }
        }

        /// <summary>
        ///  Step 0: Clear any previously left-over state
        /// </summary>
        public void BeginSenseCalculation<TGameContext>(TGameContext ctx)
            where TGameContext : ITimeContext
        {
            currentTime = ctx.TimeSource.FixedStepTime;
        }

        public void CollectReceptor<TItemId, TGameContext, TPosition>(IEntityViewControl<TItemId> v,
                                                                      TGameContext context,
                                                                      TItemId k,
                                                                      in SensoryReceptorData<TReceptorSense> definition,
                                                                      in SensoryReceptorState<TReceptorSense> state,
                                                                      in TPosition pos)
            where TItemId : IEntityKey
            where TPosition : IPosition
        {
            if (definition.Enabled)
            {
                var position = Position.From(pos);
                if (state.LastPosition != position)
                {
                    var nstate = state.WithPosition(position).WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.WriteBack(k, new SenseReceptorDirtyFlag<VisionSense>());
                }
                else if (state.State != SenseSourceDirtyState.Active ||
                         (cacheView.TryGetValue(out var cache) && 
                          cache.IsDirty(pos, physics.SignalRadiusForIntensity(definition.SenseDefinition.Intensity))))
                {
                    var nstate = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.WriteBack(k, new SenseReceptorDirtyFlag<VisionSense>());
                }

                AddActiveSense(state);
                return;
            }

            if (state.State != SenseSourceDirtyState.Inactive)
            {
                // Light has been disabled since the last calculation.
                var nstate = state.WithPosition(Position.Invalid).WithDirtyState(SenseSourceDirtyState.Dirty);
                v.WriteBack(k, in nstate);
                if (state.SenseSource.TryGetValue(out var data))
                {
                    data.Reset();
                }
            }

            // lights that are not enabled and have not been enabled in the last
            // turn can be safely ignored.
        }

        void AddActiveSense(SensoryReceptorState<TReceptorSense> s)
        {
            var level = s.LastPosition.GridZ;
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new SenseDataLevel(level, cacheView, senseData);
                activeLightsPerLevel[level] = lights;
            }

            lights.MarkUsed(currentTime);
        }

        public void CollectSenseSource<TItemId, TGameContext, TSenseSource>(IEntityViewControl<TItemId> v,
                                                                            TGameContext context,
                                                                            TItemId k,
                                                                            in TSenseSource senseDefinition,
                                                                            in SenseSourceState<TSourceSense> senseState)
            where TItemId : IEntityKey
            where TSenseSource : ISenseDefinition
        {
            if (senseDefinition.Enabled && !senseState.LastPosition.IsInvalid)
            {
                AddActiveSenseSource(senseState);
            }
        }

        void AddActiveSenseSource(SenseSourceState<TSourceSense> s)
        {
            var level = s.LastPosition.GridZ;
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new SenseDataLevel(level, cacheView, senseData);
                activeLightsPerLevel[level] = lights;
            }

            lights.AddSenseSource(s);
        }

        /// <summary>
        ///   Step 2: Compute the local light state. This radiates the light from the light source and
        ///   stores the lit area in a local map. This can safely run in parallel.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <param name="dirtyMarker"></param>
        /// <param name="pos"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        /// <typeparam name="TPosition"></typeparam>
        public void RefreshLocalSenseState<TItemId, TGameContext, TPosition>(IEntityViewControl<TItemId> v,
                                                                             TGameContext context,
                                                                             TItemId k,
                                                                             in SensoryReceptorData<TReceptorSense> definition,
                                                                             in SensoryReceptorState<TReceptorSense> state,
                                                                             in SenseReceptorDirtyFlag<TReceptorSense> dirtyMarker,
                                                                             in TPosition pos)
            where TItemId : IEntityKey
            where TPosition : IPosition
        {
            if (TryGetResistanceView(pos.GridZ, out var resistanceView))
            {
                state.SenseSource.TryGetValue(out var dataIn);
                var data = RefreshSenseState(definition, pos, resistanceView, dataIn);
                state.WithDirtyState(SenseSourceDirtyState.Active).WithSenseState(data);
            }
        }

        protected virtual SenseSourceData RefreshSenseState<TPosition>(SensoryReceptorData<TReceptorSense> definition,
                                                                       TPosition pos,
                                                                       IReadOnlyView2D<float> resistanceView,
                                                                       SenseSourceData data)
            where TPosition : IPosition
        {
            var position = new Position2D(pos.GridX, pos.GridY);
            data = sensePropagationAlgorithm.Calculate(definition.SenseDefinition, position, resistanceView, data);
            return data;
        }
        
        

        /// <summary>
        ///   Step 4: Mark sense source as clean
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        public void ResetSenseSourceCacheState<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                      TGameContext context,
                                                                      TItemId k,
                                                                      in SensoryReceptorData<TReceptorSense> definition,
                                                                      in SensoryReceptorState<TReceptorSense> state,
                                                                      in SenseReceptorDirtyFlag<TReceptorSense> dirty)
            where TItemId : IEntityKey
        {
            if (definition.Enabled && state.State != SenseSourceDirtyState.Active)
            {
                v.WriteBack(k, state.WithDirtyState(SenseSourceDirtyState.Active));
            }

            if (!definition.Enabled && state.State != SenseSourceDirtyState.Inactive)
            {
                v.WriteBack(k, state.WithDirtyState(SenseSourceDirtyState.Inactive));
            }
        }

        /// <summary>
        ///  Step 5: Clear the collected sense sources
        /// </summary>
        public void EndSenseCalculation<TGameContext>(TGameContext ctx)
        {
            zLevelBuffer.Clear();
            foreach (var l in activeLightsPerLevel)
            {
                var level = l.Value;
                level.ClearSources();
                if ((currentTime - level.LastRecentlyUsedTime) > ZLayerTimeToLive)
                {
                    zLevelBuffer.Add(l.Key);
                }
            }

            foreach (var z in zLevelBuffer)
            {
                activeLightsPerLevel.Remove(z);
            }
        }

        protected bool TryGetResistanceView(int z, out IReadOnlyView2D<float> resistanceView)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var level))
            {
                resistanceView = level.ResistanceView;
                return true;
            }

            resistanceView = default;
            return false;
        }


        class BlockVisionMap : IReadOnlyView2D<float>
        {
            readonly IReadOnlyMapData<SenseProperties> cellProperties;

            public BlockVisionMap(IReadOnlyMapData<SenseProperties> cellProperties)
            {
                this.cellProperties = cellProperties;
            }

            public float this[int x, int y]
            {
                get
                {
                    if (x < 0 || x >= cellProperties.Width)
                    {
                        return 0;
                    }

                    if (y < 0 || x >= cellProperties.Height)
                    {
                        return 0;
                    }

                    return cellProperties[x, y].blocksLight;
                }
            }
        }

        protected class SenseDataLevel
        {
            public readonly IReadOnlyView2D<float> ResistanceView;
            public int LastRecentlyUsedTime { get; private set; }
            public readonly ISenseDataBlitter Blitter;
            
            readonly List<SenseSourceState<TSourceSense>> sources;
            readonly SenseDataMapServices senseMapServices;

            public SenseDataLevel(int z, 
                                  Optional<ISenseStateCacheView> senseCache, 
                                  IReadOnlyMapData<SenseProperties> resistanceMap, 
                                  ISenseDataBlitter blitter = null)
            {
                this.Blitter = blitter ?? new DefaultSenseDataBlitter();
                this.senseMapServices = new SenseDataMapServices(z, senseCache);
                this.ResistanceView = new BlockVisionMap(resistanceMap);
                this.sources = new List<SenseSourceState<TSourceSense>>();
            }

            public void MarkUsed(int currentTime)
            {
                LastRecentlyUsedTime = currentTime;
            }

            public ReadOnlyListWrapper<SenseSourceState<TSourceSense>> Sources => sources;  
            
            public void AddSenseSource(SenseSourceState<TSourceSense> senseSourceState)
            {
                sources.Add(senseSourceState);
            }

            public void Process(in SenseDataMap brightnessMap)
            {
                var blittableSenses = new List<(Position2D, SenseSourceData)>();
                try
                {
                    foreach (var collectedSense in Sources)
                    {
                        if (collectedSense.SenseSource.TryGetValue(out var sd))
                        {
                            var pos = collectedSense.LastPosition;
                            blittableSenses.Add((new Position2D(pos.GridX, pos.GridY), sd));
                        }
                    }

                    senseMapServices.ProcessSenseSources(brightnessMap, Blitter, blittableSenses);
                }
                finally
                {
                    blittableSenses.Clear();
                }
            }

            public void ClearSources()
            {
                sources.Clear();
            }
        }
    }
}