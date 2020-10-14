using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Sensing.Sources
{
    public class SenseSystemBase<TSense, TSenseSourceDefinition>
        where TSenseSourceDefinition : ISenseDefinition
        where TSense : ISense
    {
        static readonly ILogger Logger = SLog.ForContext<SenseSystemBase<TSense, TSenseSourceDefinition>>();

        readonly Lazy<ISensePropertiesSource> senseProperties;
        readonly Lazy<ISenseStateCacheProvider> senseCacheProvider;
        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly ISenseDataBlitter blitterFactory;
        readonly ISensePropagationAlgorithm sensePropagationAlgorithm;
        Optional<ISenseStateCacheView> cacheView;

        public SenseSystemBase([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                               [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                               [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                               ISenseDataBlitter blitterFactory = null)
        {
            this.sensePropagationAlgorithm = sensePropagationAlgorithm ?? throw new ArgumentNullException(nameof(sensePropagationAlgorithm));
            this.senseProperties = senseProperties ?? throw new ArgumentNullException(nameof(senseProperties));
            this.senseCacheProvider = senseCacheProvider ?? throw new ArgumentNullException(nameof(senseCacheProvider));

            this.blitterFactory = blitterFactory ?? new DefaultSenseDataBlitter();
            this.activeLightsPerLevel = new Dictionary<int, SenseDataLevel>();
        }

        /// <summary>
        ///   To be called once during initialization.
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TGameContext"></typeparam>
        public void EnsureSenseCacheAvailable<TGameContext>(TGameContext context)
        {
            var provider = senseCacheProvider.Value;
            if (!provider.TryGetSenseCache<TSense>(out var cache))
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
        {
            foreach (var l in activeLightsPerLevel.Values)
            {
                l.ClearCollectedSenses();
            }
        }

        /// <summary>
        ///  Step 1: Collect all active sense sources.
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
        public void CollectLights<TItemId, TGameContext, TPosition>(IEntityViewControl<TItemId> v,
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
                if (state.LastPosition != position)
                {
                    var nstate = state.WithPosition(position).WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.WriteBack(k, new SenseDirtyFlag<VisionSense>());
                }
                else if (state.State != SenseSourceDirtyState.Active ||
                         (cacheView.TryGetValue(out var cache) && cache.IsDirty(pos, definition.SenseDefinition.Radius)))
                {
                    var nstate = state.WithDirtyState(SenseSourceDirtyState.Dirty);
                    v.WriteBack(k, in nstate);
                    v.WriteBack(k, new SenseDirtyFlag<VisionSense>());
                }

                AddActiveLight(pos.GridZ, position, state);
                return;
            }

            if (state.State != SenseSourceDirtyState.Inactive)
            {
                // Light has been disabled since the last calculation.
                var nstate = state.WithPosition(Position.Invalid).WithDirtyState(SenseSourceDirtyState.Dirty);
                v.WriteBack(k, in nstate);
                AddInactiveLight(pos.GridZ);
            }

            // lights that are not enabled and have not been enabled in the last
            // turn can be safely ignored.
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
                                                                             in TSenseSourceDefinition definition,
                                                                             in SenseSourceState<TSense> state,
                                                                             in SenseDirtyFlag<TSense> dirtyMarker,
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

        protected virtual SenseSourceData RefreshSenseState<TPosition>(TSenseSourceDefinition definition,
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
        ///   Step 3: Copy all processed senses into the global sense map.
        /// </summary>
        /// <param name="v"></param>
        /// <typeparam name="TItemId"></typeparam>
        public void ProcessSenseMap<TItemId>(EntityRegistry<TItemId> v)
            where TItemId : IEntityKey
        {
            Parallel.ForEach(activeLightsPerLevel.Values, ProcessSenseMapInstance);

            v.ResetComponent<SenseDirtyFlag<VisionSense>>();
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
                                                                      in SenseSourceState<TSense> state)
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
            foreach (var l in activeLightsPerLevel.Values)
            {
                l.ClearCollectedSenses();
            }
        }


        protected bool TryGetSenseData(int z, out ISenseDataView brightnessMap)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var data))
            {
                brightnessMap = data.SenseMap;
                return true;
            }

            brightnessMap = default;
            return false;
        }

        protected void AddActiveLight(int level, Position p, SenseSourceState<TSense> s)
        {
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new SenseDataLevel(senseData, blitterFactory);
                activeLightsPerLevel[level] = lights;
            }

            lights.Add(p, s);
        }

        protected void AddInactiveLight(int level)
        {
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                if (!senseProperties.Value.TryGet(level, out var senseData))
                {
                    return;
                }

                lights = new SenseDataLevel(senseData, blitterFactory);
                activeLightsPerLevel[level] = lights;
            }

            lights.ForceDirty = true;
        }

        static readonly Action<SenseDataLevel> ProcessSenseMapInstance = v => v.ProcessSenseSources();

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

        class SenseDataLevel
        {
            public readonly SenseDataMap SenseMap;
            readonly SenseDataMapServices senseMapServices;
            readonly ISenseDataBlitter blitter;
            readonly List<(Position2D, SenseSourceState<TSense>)> collectedSenses;
            readonly List<(Position2D, SenseSourceData)> blittableSenses;
            public readonly IReadOnlyView2D<float> ResistanceView;

            public SenseDataLevel(IReadOnlyMapData<SenseProperties> resistanceMap,
                                  ISenseDataBlitter blitter = null)
            {
                this.blitter = blitter ?? new DefaultSenseDataBlitter();
                this.ResistanceView = new BlockVisionMap(resistanceMap);
                this.senseMapServices = new SenseDataMapServices();
                this.SenseMap = new SenseDataMap();
                this.blittableSenses = new List<(Position2D, SenseSourceData)>();
                this.collectedSenses = new List<(Position2D, SenseSourceState<TSense>)>();
            }

            public bool ForceDirty { get; set; }

            public void Add(Position p, SenseSourceState<TSense> sense)
            {
                collectedSenses.Add((new Position2D(p.GridX, p.GridY), sense));
            }

            public void ClearCollectedSenses()
            {
                collectedSenses.Clear();
            }

            public void ProcessSenseSources()
            {
                blittableSenses.Clear();
                try
                {
                    foreach (var collectedSense in collectedSenses)
                    {
                        if (collectedSense.Item2.SenseSource.TryGetValue(out var sd))
                        {
                            blittableSenses.Add((collectedSense.Item1, sd));
                        }
                    }

                    senseMapServices.ProcessSenseSources(SenseMap, blitter, blittableSenses);
                }
                finally
                {
                    blittableSenses.Clear();
                }
            }
        }
    }
}