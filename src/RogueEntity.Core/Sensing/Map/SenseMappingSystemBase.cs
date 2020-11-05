using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
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
        readonly ISenseMapDataBlitter blitterFactory;
        readonly List<int> zLevelBuffer;

        protected SenseMappingSystemBase([NotNull] Lazy<ITimeSource> timeSource,
                                         ISenseMapDataBlitter blitterFactory)
        {
            this.timeSource = timeSource;
            this.blitterFactory = blitterFactory ?? new DefaultSenseMapDataBlitter();
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
        ///  Step 3: Finally clear the collected sense sources
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

        public bool TryGetSenseData(int z, out IDynamicSenseDataView2D data)
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
            readonly SenseMapBlitterService senseMapBlitterService;
            readonly ISenseMapDataBlitter blitter;
            readonly List<(Position2D pos, SenseSourceState<TSourceSense> senseState)> collectedSenses;
            readonly List<(Position2D pos, SenseSourceData senseData)> blittableSenses;
            public int LastRecentlyUsedTime { get; private set; }

            public SenseDataLevel(ISenseMapDataBlitter blitter)
            {
                this.blitter = blitter;
                this.senseMapBlitterService = new SenseMapBlitterService();
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
                        if (!collectedSense.senseState.SenseSource.TryGetValue(out var sd))
                        {
                            continue;
                        }

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

                    senseMapBlitterService.ProcessSenseSources(SenseMap, targetBounds, blitter, blittableSenses);
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