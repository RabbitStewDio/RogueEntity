using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Caching;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Sources
{
    public class SenseSourceSystem<TSense, TSenseSourceDefinition>
        where TSenseSourceDefinition : ISenseDefinition
        where TSense : ISense
    {
        static readonly ILogger logger = SLog.ForContext<SenseSourceSystem<TSense, TSenseSourceDefinition>>();
        const int ZLayerTimeToLive = 50;

        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly Lazy<IReadOnlyDynamicDataView3D<float>> senseProperties;
        readonly Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider;
        readonly Lazy<ITimeSource> timeSource;
        readonly ISenseStateCacheControl senseCacheControl;
        readonly ISensePropagationAlgorithm sensePropagationAlgorithm;
        readonly ISensePhysics physics;
        readonly List<int> zLevelBuffer;
        readonly ISensoryResistanceDirectionView<TSense> directionalitySystem;

        IReadOnlyDynamicDataView3D<float>? resistanceSource;
        Optional<IGridStateCache> cacheView;
        int currentTime;

        public SenseSourceSystem(Lazy<IReadOnlyDynamicDataView3D<float>> senseProperties,
                                 Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider,
                                 Lazy<ITimeSource> timeSource,
                                 ISensoryResistanceDirectionView<TSense> directionalitySystem,
                                 ISenseStateCacheControl senseCacheControl,
                                 ISensePropagationAlgorithm sensePropagationAlgorithm,
                                 ISensePhysics physics)
        {
            this.senseProperties = senseProperties ?? throw new ArgumentNullException(nameof(senseProperties));
            this.sensePropagationAlgorithm = sensePropagationAlgorithm ?? throw new ArgumentNullException(nameof(sensePropagationAlgorithm));
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
            this.activeLightsPerLevel = new Dictionary<int, SenseDataLevel>();
            this.zLevelBuffer = new List<int>();
            this.senseCacheProvider = senseCacheProvider ?? throw new ArgumentNullException(nameof(senseCacheProvider));
            this.timeSource = timeSource;
            this.directionalitySystem = directionalitySystem ?? throw new ArgumentNullException(nameof(directionalitySystem));
            this.senseCacheControl = senseCacheControl ?? throw new ArgumentNullException(nameof(senseCacheControl));
        }

        /// <summary>
        ///   To be called once during initialization.
        /// </summary>
        public void EnsureSenseCacheAvailable()
        {
            var provider = senseCacheProvider.Value;
            if (provider.TryGetGlobalSenseCache(out var cache))
            {
                cacheView = Optional.ValueOf(cache);
            }
            else
            {
                logger.Verbose("SenseCacheProvider did not return a global sense cache. The sense system will not react to map changes");
            }

            if (resistanceSource == null)
            {
                this.resistanceSource = senseProperties.Value;
            }
        }

        /// <summary>
        ///   To be called during system shutdown.
        /// </summary>
        public void ShutDown()
        {
            activeLightsPerLevel.Clear();
        }

        /// <summary>
        ///  Step 0: Clear any previously left-over state
        /// </summary>
        public void BeginSenseCalculation()
        {
            currentTime = timeSource.Value.FixedStepFrameCounter;
        }

        /// <summary>
        ///  Step 1: Collect all active sense sources that requires a refresh.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <param name="pos"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TPosition"></typeparam>
        public void FindDirtySenseSources<TItemId, TPosition>(IEntityViewControl<TItemId> v,
                                                              TItemId k,
                                                              in TSenseSourceDefinition definition,
                                                              in TPosition pos,
                                                              ref SenseSourceState<TSense> state)
            where TItemId : struct, IEntityKey
            where TPosition : IPosition<TPosition>
        {
            if (definition.Enabled && !pos.IsInvalid)
            {
                var position = Position.From(pos);
                var localIntensity = ComputeIntensity(definition.SenseDefinition, in position);
                if (state.LastPosition != position ||
                    Math.Abs(state.LastIntensity - localIntensity) > 0.05f)
                {
                    state = state.WithPosition(position)
                                 .WithIntensity(localIntensity)
                                 .WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.AssignOrReplace(k, new SenseDirtyFlag<TSense>());

                    senseCacheControl.MarkDirty<TSense>(position);
                }
                else
                {
                    var isCacheDirty = cacheView.TryGetValue(out var cache) && cache.IsDirty(pos, physics.SignalRadiusForIntensity(localIntensity));
                    if (state.State != SenseSourceDirtyState.Active || isCacheDirty)
                    {
                        state = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                        v.AssignOrReplace(k, new SenseDirtyFlag<TSense>());

                        senseCacheControl.MarkDirty<TSense>(position);
                    }
                }

                return;
            }

            if (state.State != SenseSourceDirtyState.Inactive)
            {
                // Light has been disabled since the last calculation.
                state = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                v.AssignOrReplace(k, new SenseDirtyFlag<TSense>());

                if (!state.LastPosition.IsInvalid)
                {
                    senseCacheControl.MarkDirty<TSense>(state.LastPosition);
                }
            }

            // lights that are not enabled and have not been enabled in the last
            // turn can be safely ignored.
        }

        protected virtual float ComputeIntensity(in SenseSourceDefinition sd, in Position p)
        {
            return sd.Intensity;
        }

        /// <summary>
        ///   Step 2: Compute the local light state. This radiates the light from the light source and
        ///   stores the lit area in a local map. This can safely run in parallel.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <param name="dirtyMarker"></param>
        /// <param name="observed"></param>
        /// <typeparam name="TItemId"></typeparam>
        public void RefreshLocalSenseState<TItemId>(IEntityViewControl<TItemId> v,
                                                    TItemId k,
                                                    in TSenseSourceDefinition definition,
                                                    in SenseDirtyFlag<TSense> dirtyMarker,
                                                    in ObservedSenseSource<TSense> observed,
                                                    ref SenseSourceState<TSense> state)
            where TItemId : struct, IEntityKey
        {
            if (!definition.Enabled)
            {
                state = state.WithDirtyState(SenseSourceDirtyState.Inactive);
                if (state.SenseSource.TryGetValue(out var dataIn))
                {
                    dataIn.Reset();
                }

                Console.WriteLine($"Pos {state.LastPosition}/{k} not enabled");
                return;
            }

            var pos = state.LastPosition;
            if (TryGetResistanceView(pos.GridZ, out var resistanceView, out var directionMap))
            {
                state.SenseSource.TryGetValue(out var dataIn);
                var data = RefreshSenseState(definition, state.LastIntensity, pos, resistanceView, directionMap, dataIn);
                state = state.WithDirtyState(SenseSourceDirtyState.Active)
                             .WithSenseState(data);

                Console.WriteLine($"Pos {state.LastPosition} active");
            }
            else
            {
                state = state.WithDirtyState(SenseSourceDirtyState.Inactive);
                if (state.SenseSource.TryGetValue(out var dataIn))
                {
                    dataIn.Reset();
                }

                Console.WriteLine($"Pos {state.LastPosition} no resistance view, ignored");
            }
        }

        protected virtual SenseSourceData RefreshSenseState<TPosition>(in TSenseSourceDefinition definition,
                                                                       float intensity,
                                                                       in TPosition pos,
                                                                       IReadOnlyDynamicDataView2D<float> resistanceView,
                                                                       IReadOnlyDynamicDataView2D<DirectionalityInformation> directionView,
                                                                       SenseSourceData data)
            where TPosition : IPosition<TPosition>
        {
            var position = new Position2D(pos.GridX, pos.GridY);
            var sourceDefinition = definition.SenseDefinition;
            data = sensePropagationAlgorithm.Calculate(sourceDefinition, intensity, position, resistanceView, directionView, data);
            data.MarkWritten();
            return data;
        }


        /// <summary>
        ///   Step 4: Mark sense source as clean
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <param name="dirtyFlag"></param>
        /// <typeparam name="TItemId"></typeparam>
        public void ResetSenseSourceCacheState<TItemId>(IEntityViewControl<TItemId> v,
                                                        TItemId k,
                                                        in TSenseSourceDefinition definition,
                                                        in SenseDirtyFlag<TSense> dirtyFlag,
                                                        ref SenseSourceState<TSense> state)
            where TItemId : struct, IEntityKey
        {
            // we only ever clear the dirty state of observed components. This 
            // ensures that unobserved entities remain eligible for processing 
            // once an observer comes into range.
            if (v.GetComponent(k, out ObservedSenseSource<TSense> _))
            {
                if (definition.Enabled && state.State != SenseSourceDirtyState.Active)
                {
                    if (state.LastPosition.IsInvalid)
                    {
                        logger.Debug("Pos is invalid, entity sense marked inactive");
                        state = state.WithDirtyState(SenseSourceDirtyState.Inactive);
                    }
                    else
                    {
                        logger.Debug("Pos {LastPosition} marked active", state.LastPosition);
                        state = state.WithDirtyState(SenseSourceDirtyState.Active);
                    }
                }

                if (!definition.Enabled && state.State != SenseSourceDirtyState.Inactive)
                {
                    logger.Debug("Pos {LastPosition} marked inactive", state.LastPosition);
                    state = state.WithDirtyState(SenseSourceDirtyState.Inactive);
                }
            }

            v.RemoveComponent<SenseDirtyFlag<TSense>>(k);
        }

        /// <summary>
        ///  Step 5: Clear the collected sense sources
        /// </summary>
        public void EndSenseCalculation()
        {
            zLevelBuffer.Clear();
            foreach (var l in activeLightsPerLevel)
            {
                var level = l.Value;
                if ((currentTime - level.LastRecentlyUsedTime) > ZLayerTimeToLive)
                {
                    zLevelBuffer.Add(l.Key);
                }
            }

            foreach (var z in zLevelBuffer)
            {
                activeLightsPerLevel.Remove(z);
            }

            zLevelBuffer.Clear();
        }

        protected bool TryGetResistanceView(int z, 
                                            [MaybeNullWhen(false)] out IReadOnlyDynamicDataView2D<float> resistanceView, 
                                            [MaybeNullWhen(false)] out IReadOnlyDynamicDataView2D<DirectionalityInformation> directionMap)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var level))
            {
                resistanceView = level.ResistanceView;
                directionMap = level.DirectionMap;
                level.MarkUsed(currentTime);
                return true;
            }

            if (resistanceSource == null) throw new InvalidOperationException();
            
            if (resistanceSource.TryGetView(z, out var sensePropertiesForLevel) &&
                directionalitySystem.ResultView.TryGetView(z, out var directionMapForLevel))
            {
                level = new SenseDataLevel(sensePropertiesForLevel, directionMapForLevel);
                level.MarkUsed(currentTime);
                activeLightsPerLevel[z] = level;
                resistanceView = level.ResistanceView;
                directionMap = level.DirectionMap;
                return true;
            }

            resistanceView = default;
            directionMap = default;
            return false;
        }

        class SenseDataLevel
        {
            public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> DirectionMap;
            public readonly IReadOnlyDynamicDataView2D<float> ResistanceView;
            public int LastRecentlyUsedTime { get; private set; }

            public SenseDataLevel(IReadOnlyDynamicDataView2D<float> resistanceMap, IReadOnlyDynamicDataView2D<DirectionalityInformation> directionMap)
            {
                DirectionMap = directionMap;
                this.ResistanceView = resistanceMap;
            }

            public void MarkUsed(int currentTime)
            {
                LastRecentlyUsedTime = currentTime;
            }
        }
    }
}
