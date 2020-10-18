using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using GoRogue;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

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
    public class SenseReceptorSystemBase<TReceptorSense, TSourceSense>
        where TReceptorSense : ISense
        where TSourceSense : ISense
    {
        static readonly ILogger Logger = SLog.ForContext<SenseReceptorSystemBase<TReceptorSense, TSourceSense>>();
        const int ZLayerTimeToLive = 50;

        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<ISenseStateCacheProvider> senseCacheProvider;
        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly ISensePropagationAlgorithm sensePropagationAlgorithm;
        readonly List<int> zLevelBuffer;
        readonly ISensePhysics physics;
        readonly ISenseDataBlitter blitter;

        Optional<ISenseStateCacheView> cacheView;
        int currentTime;

        public SenseReceptorSystemBase([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                       [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                       [NotNull] ISensePhysics physics,
                                       [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm)
        {
            this.sensePropagationAlgorithm = sensePropagationAlgorithm ?? throw new ArgumentNullException(nameof(sensePropagationAlgorithm));
            this.blitter = blitter ?? throw new ArgumentNullException(nameof(blitter));
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
                if (state.LastPosition != position)
                {
                    var nstate = state.WithPosition(position).WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.AssignOrReplace(k, new SenseReceptorDirtyFlag<VisionSense>());
                }
                else if (state.State != SenseSourceDirtyState.Active ||
                         (cacheView.TryGetValue(out var cache) &&
                          cache.IsDirty(pos, physics.SignalRadiusForIntensity(definition.SenseDefinition.Intensity))))
                {
                    var nstate = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.AssignOrReplace(k, new SenseReceptorDirtyFlag<VisionSense>());
                }

                AddActiveSenseReceptor(definition.SenseDefinition.Intensity, position);
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

        void AddActiveSenseReceptor(float intensity, in Position pos)
        {
            var level = pos.GridZ;
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new SenseDataLevel(level, cacheView,  senseData, physics);
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
                if (level.IsOverlapping(senseDefinition.SenseDefinition.Intensity, senseState.LastPosition))
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

                lights = new SenseDataLevel(level, cacheView, senseData, physics);
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
        public void RefreshLocalReceptorState<TItemId, TGameContext, TPosition>(IEntityViewControl<TItemId> v,
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
                var data = RefreshReceptorState(definition, pos, resistanceView, dataIn);
                state.WithDirtyState(SenseSourceDirtyState.Active).WithSenseState(data);
            }
        }

        protected virtual SenseSourceData RefreshReceptorState<TPosition>(SensoryReceptorData<TReceptorSense> definition,
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

            readonly ISensePhysics physics;
            readonly List<SenseSourceState<TSourceSense>> sources;
            readonly SenseDataMapServices senseMapServices;
            readonly DirectionalSenseDataMapServices directionalSenseMapServices;
            readonly List<Rectangle> receptorBounds;

            public SenseDataLevel(int z,
                                  Optional<ISenseStateCacheView> senseCache,
                                  IReadOnlyMapData<SenseProperties> resistanceMap,
                                  ISensePhysics physics)
            {
                this.physics = physics;
                this.senseMapServices = new SenseDataMapServices(z, senseCache);
                this.directionalSenseMapServices = new DirectionalSenseDataMapServices(z, senseCache);
                this.ResistanceView = new BlockVisionMap(resistanceMap);
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
                var radius = (int) Math.Ceiling(physics.SignalRadiusForIntensity(intensity));
                var bounds = new Rectangle(new Coord(pos.GridX, pos.GridY), radius, radius);
                receptorBounds.Add(bounds);
            }

            public bool IsOverlapping(float intensity, in Position pos)
            {
                var radius = (int) Math.Ceiling(physics.SignalRadiusForIntensity(intensity));
                var bounds = new Rectangle(new Coord(pos.GridX, pos.GridY), radius, radius);
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
}