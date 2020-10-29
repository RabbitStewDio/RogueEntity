using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Sensing.Map
{
    public class SenseMappingSystemBase<TTargetSense, TSourceSense, TSenseSourceDefinition>
        where TSenseSourceDefinition : ISenseDefinition
        where TTargetSense : ISense
        where TSourceSense : ISense
    {
        static readonly ILogger Logger = SLog.ForContext<SenseMappingSystemBase<TTargetSense, TSourceSense, TSenseSourceDefinition>>();
        static readonly Action<SenseDataLevel> ProcessSenseMapInstance = v => v.ProcessSenseSources();
        const int ZLayerTimeToLive = 50;

        readonly Lazy<ITimeSource> timeSource;
        readonly Dictionary<int, SenseDataLevel> activeLightsPerLevel;
        readonly ISenseDataBlitter blitterFactory;
        readonly List<int> zLevelBuffer;

        protected SenseMappingSystemBase([NotNull] Lazy<ITimeSource> timeSource,
                                         ISenseDataBlitter blitterFactory)
        {
            this.timeSource = timeSource;
            this.blitterFactory = blitterFactory ?? new DefaultSenseDataBlitter();
            this.activeLightsPerLevel = new Dictionary<int, SenseDataLevel>();
            this.zLevelBuffer = new List<int>();
        }

        /// <summary>
        ///   Cleanup method used during system shutdown.
        /// </summary>
        /// <param name="ctx"></param>
        /// <typeparam name="TGameContext"></typeparam>
        public void ShutDown<TGameContext>(TGameContext ctx)
        {
            activeLightsPerLevel.Clear();
        }

        /// <summary>
        ///  Step 1: Collect all active sense sources. Also marks all lights as observed.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="definition"></param>
        /// <param name="state"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        public void CollectSenseSources<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                               TGameContext context,
                                                               TItemId k,
                                                               in TSenseSourceDefinition definition,
                                                               in SenseSourceState<TSourceSense> state)
            where TItemId : IEntityKey
        {
            if (!state.LastPosition.IsInvalid)
            {
                v.AssignOrReplace<ObservedSenseSource<TSourceSense>>(k);
                AddActiveSense(state);
            }
        }

        /// <summary>
        ///   Step 2: Copy all processed senses into the global sense map.
        /// </summary>
        /// <param name="v"></param>
        /// <typeparam name="TItemId"></typeparam>
        public void ProcessSenseMap<TItemId>(EntityRegistry<TItemId> v)
            where TItemId : IEntityKey
        {
            Parallel.ForEach(activeLightsPerLevel.Values, ProcessSenseMapInstance);

            v.ResetComponent<SenseDirtyFlag<TSourceSense>>();
        }

        
        /// <summary>
        ///   Step 3: Apply sense map data to each sense-receptor in need of updates. This action replaces the
        ///   <see cref="OmnidirectionalSenseReceptorSystem{TTargetSense,TSourceSense}.CopySenseSourcesToVisionField{TItemId,TGameContext}"/>
        ///   action of the SenseReceptorModule. Instead of populating the receptor from each sense source
        ///   individually, we can reuse the already computed global sense map as data source.
        /// </summary>
        /// <remarks>
        ///   This only works well for omni-directional sources, like light or infra-vision receptors. Directional
        ///   sensing is highly location specific and cannot be replaced this way.
        ///
        ///   This action must be executed after the sense-receptor had a chance to compute its own field of view.
        /// </remarks>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="receptorState"></param>
        /// <param name="receptorDirtyFlag"></param>
        /// <param name="brightnessMap"></param>
        /// <typeparam name="TItemId"></typeparam>
        /// <typeparam name="TGameContext"></typeparam>
        public void ApplyReceptorFieldOfView<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                    TGameContext context,
                                                                    TItemId k,
                                                                    in SensoryReceptorState<TTargetSense> receptorState,
                                                                    in SenseReceptorDirtyFlag<TTargetSense> receptorDirtyFlag,
                                                                    in SingleLevelSenseDirectionMapData<TTargetSense, TSourceSense> brightnessMap)
            where TItemId : IEntityKey
        {
            if (!receptorState.SenseSource.TryGetValue(out var sourceData))
            {
                v.WriteBack(k, brightnessMap.WithDisabledState());
                return;
            }

            var lastPosition = receptorState.LastPosition;
            var z = lastPosition.GridZ;
            if (!TryGetSenseData(z, out var lights))
            {
                v.WriteBack(k, brightnessMap.WithDisabledState());
                return;
            }

            var dest = brightnessMap.SenseMap;
            dest.Clear();
            
            SenseReceptors.CopyReceptorFieldOfView(dest, lastPosition, receptorState.LastIntensity, sourceData, lights);

            v.WriteBack(k, brightnessMap.WithLevel(z));
        }

        
        /// <summary>
        ///  Step 4: Finally clear the collected sense sources
        /// </summary>
        public void EndSenseCalculation<TGameContext>(TGameContext ctx)
        {
            var currentTime = timeSource.Value.FixedStepTime;
            zLevelBuffer.Clear();
            foreach (var l in activeLightsPerLevel)
            {
                var level = l.Value;
                level.ClearCollectedSenses();
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

        void AddActiveSense(SenseSourceState<TSourceSense> s)
        {
            var p = s.LastPosition;
            var level = p.GridZ;
            if (!activeLightsPerLevel.TryGetValue(level, out var lights))
            {
                lights = new SenseDataLevel(blitterFactory);
                activeLightsPerLevel[level] = lights;
            }

            var currentTime = timeSource.Value.FixedStepTime;
            lights.MarkUsed(currentTime);
            lights.Add(p, s);
        }

        public bool TryGetSenseData(int z, out ISenseDataView data)
        {
            if (activeLightsPerLevel.TryGetValue(z, out var level))
            {
                var currentTime = timeSource.Value.FixedStepTime;
                level.MarkUsed(currentTime);
                data = level.SenseMap;
                return true;
            }

            data = default;
            return false;
        }

        class SenseDataLevel
        {
            public readonly SenseDataMap SenseMap;
            readonly OmniDirectionalSenseDataMapServices senseMapServices;
            readonly ISenseDataBlitter blitter;
            readonly List<(Position2D pos, SenseSourceState<TSourceSense> senseState)> collectedSenses;
            readonly List<(Position2D pos, SenseSourceData senseData)> blittableSenses;
            public int LastRecentlyUsedTime { get; private set; }

            public SenseDataLevel(ISenseDataBlitter blitter = null)
            {
                this.blitter = blitter ?? new DefaultSenseDataBlitter();
                this.senseMapServices = new OmniDirectionalSenseDataMapServices();
                this.SenseMap = new SenseDataMap();
                this.blittableSenses = new List<(Position2D, SenseSourceData)>();
                this.collectedSenses = new List<(Position2D, SenseSourceState<TSourceSense>)>();
            }

            public void Add(Position p, SenseSourceState<TSourceSense> sense)
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
                    Rectangle targetBounds = default;
                    foreach (var collectedSense in collectedSenses)
                    {
                        if (collectedSense.senseState.SenseSource.TryGetValue(out var sd))
                        {
                            var senseBounds = sd.Bounds.WithCenter(collectedSense.senseState.LastPosition.ToGridXY());
                            if (blittableSenses.Count == 0)
                            {
                                targetBounds = senseBounds;
                            }
                            else
                            {
                                targetBounds = targetBounds.GetUnion(senseBounds);
                            }
                            
                            blittableSenses.Add((collectedSense.pos, sd));
                        }
                    }

                    senseMapServices.ProcessSenseSources(SenseMap, targetBounds, blitter, blittableSenses);
                }
                finally
                {
                    blittableSenses.Clear();
                }
            }

            public void MarkUsed(int currentTime)
            {
                LastRecentlyUsedTime = currentTime;
            }
        }
    }
}