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
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Receptors
{
    /// <summary>
    ///   A system implementation that coordinates sense and receptor calculations.
    ///   This system collects active sense receptors, matches them with overlapping
    ///   sense sources (and marks them for use in the sense source systems) and
    ///   finally updates the sense receptor's local sense data if needed. 
    /// </summary>
    /// <typeparam name="TReceptorSense"></typeparam>
    /// <typeparam name="TSourceSense"></typeparam>
    public abstract class SenseReceptorSystem<TReceptorSense, TSourceSense>
        where TReceptorSense : ISense
        where TSourceSense : ISense
    {
        static readonly ILogger Logger = SLog.ForContext<SenseReceptorSystem<TReceptorSense, TSourceSense>>();
        const int ZLayerTimeToLive = 50;

        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<ISenseStateCacheProvider> senseCacheProvider;
        readonly Lazy<IGlobalSenseStateCacheProvider> globalSenseCacheProvider;
        readonly Lazy<ITimeSource> timeSource;
        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly ISensePropagationAlgorithm sensePropagationAlgorithm;
        readonly List<int> zLevelBuffer;
        readonly ISensePhysics physics;
        readonly ISenseDataBlitter blitter;

        Optional<ISenseStateCacheView> cacheView;
        Optional<ISenseStateCacheView> globalCacheView;
        int currentTime;

        public SenseReceptorSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                   [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                   [NotNull] Lazy<IGlobalSenseStateCacheProvider> globalSenseCacheProvider,
                                   [NotNull] Lazy<ITimeSource> timeSource,
                                   [NotNull] ISensePhysics physics,
                                   [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm)
        {
            this.sensePropagationAlgorithm = sensePropagationAlgorithm ?? throw new ArgumentNullException(nameof(sensePropagationAlgorithm));
            this.blitter = blitter ?? throw new ArgumentNullException(nameof(blitter));
            this.senseProperties = senseProperties ?? throw new ArgumentNullException(nameof(senseProperties));
            this.senseCacheProvider = senseCacheProvider ?? throw new ArgumentNullException(nameof(senseCacheProvider));
            this.globalSenseCacheProvider = globalSenseCacheProvider;
            this.timeSource = timeSource;
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));

            this.activeLightsPerLevel = new Dictionary<int, SenseDataLevel>();
            this.zLevelBuffer = new List<int>();
        }

        public bool TryGetLevel(int z, out ISenseReceptorProcessor level)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var levelRaw))
            {
                level = levelRaw;
                return true;
            }

            level = default;
            return false;
        }

        /// <summary>
        ///   To be called once during initialization.
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TGameContext"></typeparam>
        public void EnsureSenseCacheAvailable<TGameContext>(TGameContext context)
        {
            var provider = senseCacheProvider.Value;
            if (!provider.TryGetSenseCache<TSourceSense>(out var cache))
            {
                cacheView = Optional.ValueOf(cache);
            }
            else
            {
                Logger.Verbose("This light system will not react to map changes");
            }

            var globalProvider = globalSenseCacheProvider.Value;
            if (!globalProvider.TryGetGlobalSenseCache(out var gcache))
            {
                globalCacheView = Optional.ValueOf(gcache);
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
        {
            currentTime = timeSource.Value.FixedStepTime;
        }

        /// <summary>
        ///   Step 1: Collect all sense receptors currently active in the game.
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
                var localIntensity = ComputeIntensity(definition.SenseDefinition, in position);
                if (state.LastPosition != position ||
                    Math.Abs(state.LastIntensity - localIntensity) > 0.05f)
                {
                    // todo
                    var nstate = state.WithPosition(position).WithIntensity(localIntensity).WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.AssignOrReplace(k, new SenseReceptorDirtyFlag<TReceptorSense>());
                }
                else if (state.State != SenseSourceDirtyState.Active ||
                         IsCacheDirty(in position, physics.SignalRadiusForIntensity(localIntensity)))
                {
                    var nstate = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.AssignOrReplace(k, new SenseReceptorDirtyFlag<TReceptorSense>());
                }

                AddActiveSenseReceptor(localIntensity, position);
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

        protected virtual float ComputeIntensity(in SenseSourceDefinition sd, in Position p)
        {
            return sd.Intensity;
        }
        
        bool IsCacheDirty(in Position pos, float radius)
        {
            bool haveTestedCache = false;
            if (globalCacheView.TryGetValue(out var gcache))
            {
                if (gcache.IsDirty(pos, radius))
                {
                    return true;
                }

                haveTestedCache = true;
            }

            if (cacheView.TryGetValue(out var cache))
            {
                if (gcache.IsDirty(pos, radius))
                {
                    return true;
                }

                haveTestedCache = true;
            }

            // if there are no caches at all, assume that everything is dirty.
            return !haveTestedCache;
        }

        void AddActiveSenseReceptor(float intensity, in Position pos)
        {
            var level = pos.GridZ;
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new SenseDataLevel(level, cacheView, CreateSensoryResistanceView(senseData), physics);
                activeLightsPerLevel[level] = lights;
            }

            lights.AddReceptor(intensity, in pos);
            lights.MarkUsed(currentTime);
        }

        /// <summary>
        ///    Step 2: Collect all sense sources, filtering out those that do not overlap
        ///            at least one sense source.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="senseDefinition"></param>
        /// <param name="senseState"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        /// <typeparam name="TSenseSource"></typeparam>
        public void CollectSenseSource<TItemId, TGameContext, TSenseSource>(IEntityViewControl<TItemId> v,
                                                                            TGameContext context,
                                                                            TItemId k,
                                                                            in TSenseSource senseDefinition,
                                                                            in SenseSourceState<TSourceSense> senseState)
            where TItemId : IEntityKey
            where TSenseSource : ISenseDefinition
        {
            if (senseDefinition.Enabled &&
                !senseState.LastPosition.IsInvalid &&
                activeLightsPerLevel.TryGetValue(senseState.LastPosition.GridZ, out var level))
            {
                if (level.IsOverlapping(senseState.LastIntensity, senseState.LastPosition))
                {
                    AddActiveSenseSource(senseState);
                    v.AssignOrReplace<ObservedSenseSource<TSourceSense>>(k);
                }
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

                lights = new SenseDataLevel(level, cacheView, CreateSensoryResistanceView(senseData), physics);
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
        public void RefreshLocalReceptorState<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                     TGameContext context,
                                                                     TItemId k,
                                                                     in SensoryReceptorData<TReceptorSense> definition,
                                                                     in SensoryReceptorState<TReceptorSense> state,
                                                                     in SenseReceptorDirtyFlag<TReceptorSense> dirtyMarker)
            where TItemId : IEntityKey
        {
            var pos = state.LastPosition;
            if (pos.IsInvalid)
            {
                return;
            }

            if (TryGetResistanceView(pos.GridZ, out var resistanceView))
            {
                state.SenseSource.TryGetValue(out var dataIn);
                var data = RefreshReceptorState(definition, state.LastIntensity, pos, resistanceView, dataIn);
                state.WithDirtyState(SenseSourceDirtyState.Active).WithSenseState(data);
            }
        }

        protected virtual SenseSourceData RefreshReceptorState<TPosition>(SensoryReceptorData<TReceptorSense> definition,
                                                                          float intensity,
                                                                          TPosition pos,
                                                                          IReadOnlyView2D<float> resistanceView,
                                                                          SenseSourceData data)
            where TPosition : IPosition
        {
            var position = new Position2D(pos.GridX, pos.GridY);
            data = sensePropagationAlgorithm.Calculate(definition.SenseDefinition, intensity, position, resistanceView, data);
            return data;
        }


        /// <summary>
        ///   Step 3: Mark sense receptors as clean
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        public void ResetReceptorCacheState<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
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
        ///   Step 4 - Mark sense sources as clean
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="dirty"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        public void ResetSenseSourceObservedState<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                         TGameContext context,
                                                                         TItemId k,
                                                                         in ObservedSenseSource<TSourceSense> dirty)
            where TItemId : IEntityKey
        {
            v.RemoveComponent<ObservedSenseSource<TSourceSense>>(k);
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

        protected abstract IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap);

        protected class SenseDataLevel : ISenseReceptorProcessor
        {
            public readonly IReadOnlyView2D<float> ResistanceView;
            public int LastRecentlyUsedTime { get; private set; }

            readonly ISensePhysics physics;
            readonly List<SenseSourceState<TSourceSense>> sources;
            readonly SenseDataMapServices senseMapServices;
            readonly DirectionalSenseDataMapServices directionalSenseMapServices;
            readonly List<Rectangle> receptorBounds;

            public SenseDataLevel(int z,
                                  Optional<ISenseStateCacheView> senseCache,
                                  IReadOnlyView2D<float> resistanceMap,
                                  ISensePhysics physics)
            {
                this.physics = physics;
                this.senseMapServices = new SenseDataMapServices(z, senseCache);
                this.directionalSenseMapServices = new DirectionalSenseDataMapServices(z, senseCache);
                this.ResistanceView = resistanceMap;
                this.sources = new List<SenseSourceState<TSourceSense>>();
                this.receptorBounds = new List<Rectangle>();
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

            public void ProcessDirectional(IDirectionalSenseBlitter blitter, in Position p, in SenseDataMap brightnessMap, bool flaggedAsDirty)
            {
                var blittableSenses = new List<(Position2D, SenseSourceData)>();
                try
                {
                    bool dirty = flaggedAsDirty;
                    foreach (var collectedSense in Sources)
                    {
                        dirty |= (collectedSense.State == SenseSourceDirtyState.Dirty || collectedSense.State == SenseSourceDirtyState.UnconditionallyDirty);
                        if (collectedSense.SenseSource.TryGetValue(out var sd))
                        {
                            var pos = collectedSense.LastPosition;
                            blittableSenses.Add((new Position2D(pos.GridX, pos.GridY), sd));
                        }
                    }

                    if (!dirty)
                    {
                        return;
                    }

                    directionalSenseMapServices.ProcessSenseSources(brightnessMap, new Position2D(p.GridX, p.GridY), blitter, blittableSenses);
                }
                finally
                {
                    blittableSenses.Clear();
                }
            }

            public void ProcessOmnidirectional(ISenseDataBlitter blitter, in SenseDataMap brightnessMap, bool flaggedAsDirty)
            {
                var blittableSenses = new List<(Position2D, SenseSourceData)>();
                try
                {
                    bool dirty = flaggedAsDirty;
                    foreach (var collectedSense in Sources)
                    {
                        dirty |= (collectedSense.State == SenseSourceDirtyState.Dirty || collectedSense.State == SenseSourceDirtyState.UnconditionallyDirty);
                        if (collectedSense.SenseSource.TryGetValue(out var sd))
                        {
                            var pos = collectedSense.LastPosition;
                            blittableSenses.Add((new Position2D(pos.GridX, pos.GridY), sd));
                        }
                    }

                    if (!dirty)
                    {
                        return;
                    }

                    senseMapServices.ProcessSenseSources(brightnessMap, blitter, blittableSenses);
                }
                finally
                {
                    blittableSenses.Clear();
                }
            }

            public void ClearSources()
            {
                sources.Clear();
                receptorBounds.Clear();
            }

            public void AddReceptor(float intensity, in Position pos)
            {
                var radius = (int)Math.Ceiling(physics.SignalRadiusForIntensity(intensity));
                var bounds = new Rectangle(new Position2D(pos.GridX, pos.GridY), radius, radius);
                receptorBounds.Add(bounds);
            }

            public bool IsOverlapping(float intensity, in Position pos)
            {
                var radius = (int)Math.Ceiling(physics.SignalRadiusForIntensity(intensity));
                var bounds = new Rectangle(new Position2D(pos.GridX, pos.GridY), radius, radius);
                foreach (var b in receptorBounds)
                {
                    if (bounds.Intersects(b))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public interface ISenseReceptorProcessor
    {
        void ProcessDirectional(IDirectionalSenseBlitter blitter, in Position p, in SenseDataMap brightnessMap, bool flaggedAsDirty);
        void ProcessOmnidirectional(ISenseDataBlitter blitter, in SenseDataMap brightnessMap, bool flaggedAsDirty);
    }
}