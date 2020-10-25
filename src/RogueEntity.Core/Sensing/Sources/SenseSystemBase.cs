using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Sensing.Sources
{
    public abstract class SenseSystemBase<TSense, TSenseSourceDefinition>
        where TSenseSourceDefinition : ISenseDefinition
        where TSense : ISense
    {
        static readonly ILogger Logger = SLog.ForContext<SenseSystemBase<TSense, TSenseSourceDefinition>>();
        const int ZLayerTimeToLive = 50;

        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider;
        readonly Lazy<ITimeSource> timeSource;

        readonly ISenseStateCacheControl senseCacheControl;
        readonly ISensePropagationAlgorithm sensePropagationAlgorithm;
        readonly ISensePhysics physics;
        readonly List<int> zLevelBuffer;
        Optional<ISenseStateCacheView> cacheView;
        int currentTime;

        public SenseSystemBase([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                               [NotNull] Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider,
                               [NotNull] Lazy<ITimeSource> timeSource,
                               [NotNull] ISenseStateCacheControl senseCacheControl,
                               [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                               [NotNull] ISensePhysics physics)
        {
            this.senseProperties = senseProperties ?? throw new ArgumentNullException(nameof(senseProperties));
            this.sensePropagationAlgorithm = sensePropagationAlgorithm ?? throw new ArgumentNullException(nameof(sensePropagationAlgorithm));
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
            this.activeLightsPerLevel = new Dictionary<int, SenseDataLevel>();
            this.zLevelBuffer = new List<int>();
            this.senseCacheProvider = senseCacheProvider ?? throw new ArgumentNullException(nameof(senseCacheProvider));
            this.timeSource = timeSource;
            this.senseCacheControl = senseCacheControl ?? throw new ArgumentNullException(nameof(senseCacheControl));
        }

        /// <summary>
        ///   To be called once during initialization.
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TGameContext"></typeparam>
        public void EnsureSenseCacheAvailable<TGameContext>(TGameContext context)
        {
            var provider = senseCacheProvider.Value;
            if (provider.TryGetGlobalSenseCache(out var cache))
            {
                cacheView = Optional.ValueOf(cache);
            }
            else
            {
                Logger.Verbose("SenseCacheProvider did not return a global sense cache. The sense system will not react to map changes");
            }
        }

        /// <summary>
        ///   To be called during system shutdown.
        /// </summary>
        /// <param name="ctx"></param>
        /// <typeparam name="TGameContext"></typeparam>
        public void ShutDown<TGameContext>(TGameContext ctx)
        {
            activeLightsPerLevel.Clear();
        }

        /// <summary>
        ///  Step 0: Clear any previously left-over state
        /// </summary>
        public void BeginSenseCalculation<TGameContext>(TGameContext ctx)
        {
            currentTime = timeSource.Value.FixedStepTime;
        }

        /// <summary>
        ///  Step 1: Collect all active sense sources that requires a refresh.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <param name="pos"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        /// <typeparam name="TPosition"></typeparam>
        public void FindDirtySenseSources<TItemId, TGameContext, TPosition>(IEntityViewControl<TItemId> v,
                                                                            TGameContext context,
                                                                            TItemId k,
                                                                            in TSenseSourceDefinition definition,
                                                                            in SenseSourceState<TSense> state,
                                                                            in TPosition pos)
            where TItemId : IEntityKey
            where TPosition : IPosition
        {
            if (definition.Enabled)
            {
                var position = Position.From(pos);
                var localIntensity = ComputeIntensity(definition.SenseDefinition, in position);
                if (state.LastPosition != position ||
                    Math.Abs(state.LastIntensity - localIntensity) > 0.05f)
                {
                    var nstate = state.WithPosition(position).WithIntensity(localIntensity).WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.AssignOrReplace(k, new SenseDirtyFlag<TSense>());

                    senseCacheControl.MarkDirty<TSense>(position);
                }
                else if (state.State != SenseSourceDirtyState.Active ||
                         (cacheView.TryGetValue(out var cache) && cache.IsDirty(pos, physics.SignalRadiusForIntensity(localIntensity))))
                {
                    var nstate = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.AssignOrReplace(k, new SenseDirtyFlag<TSense>());

                    senseCacheControl.MarkDirty<TSense>(position);
                }

                return;
            }

            if (state.State != SenseSourceDirtyState.Inactive)
            {
                // Light has been disabled since the last calculation.
                var nstate = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                v.WriteBack(k, in nstate);
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
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <param name="dirtyMarker"></param>
        /// <param name="pos"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        /// <typeparam name="TPosition"></typeparam>
        public void RefreshLocalSenseState<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                  TGameContext context,
                                                                  TItemId k,
                                                                  in TSenseSourceDefinition definition,
                                                                  in SenseSourceState<TSense> state,
                                                                  in SenseDirtyFlag<TSense> dirtyMarker,
                                                                  in ObservedSenseSource<TSense> observed)
            where TItemId : IEntityKey
        {
            if (!definition.Enabled)
            {
                state.WithDirtyState(SenseSourceDirtyState.Inactive);
                if (state.SenseSource.TryGetValue(out var dataIn))
                {
                    dataIn.Reset();
                }
                return;
            }
            
            var pos = state.LastPosition;
            if (TryGetResistanceView(pos.GridZ, out var resistanceView))
            {
                state.SenseSource.TryGetValue(out var dataIn);
                var data = RefreshSenseState(definition, state.LastIntensity, pos, resistanceView, dataIn);
                state.WithDirtyState(SenseSourceDirtyState.Active).WithSenseState(data);
            }
        }

        protected virtual SenseSourceData RefreshSenseState<TPosition>(in TSenseSourceDefinition definition,
                                                                       float intensity,
                                                                       in TPosition pos,
                                                                       IReadOnlyView2D<float> resistanceView,
                                                                       SenseSourceData data)
            where TPosition : IPosition
        {
            var position = new Position2D(pos.GridX, pos.GridY);
            data = sensePropagationAlgorithm.Calculate(definition.SenseDefinition, intensity, position, resistanceView, data);
            data.MarkWritten();
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
                                                                      in TSenseSourceDefinition definition,
                                                                      in SenseSourceState<TSense> state,
                                                                      in SenseDirtyFlag<TSense> dirtyFlag)
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

            v.RemoveComponent<SenseDirtyFlag<TSense>>(k);
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

        protected bool TryGetResistanceView(int z, out IReadOnlyView2D<float> resistanceView)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var level))
            {
                resistanceView = level.ResistanceView;
                level.MarkUsed(currentTime);
                return true;
            }

            if (senseProperties.Value.TryGet(z, out var sensePropertiesForLevel))
            {
                level = new SenseDataLevel(CreateSensoryResistanceView(sensePropertiesForLevel));
                level.MarkUsed(currentTime);
                activeLightsPerLevel[z] = level;
                resistanceView = level.ResistanceView;
                return true;
            }

            resistanceView = default;
            return false;
        }

        protected abstract IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap);

        class SenseDataLevel
        {
            public readonly IReadOnlyView2D<float> ResistanceView;
            public int LastRecentlyUsedTime { get; private set; }

            public SenseDataLevel(IReadOnlyView2D<float> resistanceMap)
            {
                this.ResistanceView = resistanceMap;
            }

            public void MarkUsed(int currentTime)
            {
                LastRecentlyUsedTime = currentTime;
            }
        }
    }
}