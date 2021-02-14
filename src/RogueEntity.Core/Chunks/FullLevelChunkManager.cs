using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Chunks
{
    public class FullLevelChunkManager : IDisposable
    {
        class LevelTrackingData
        {
            public bool Dirty;
            public bool Unloaded;
            public bool CreateRequested;
            public bool WriteBackRequested;

            public LevelTrackingData(int lastActiveTime)
            {
                LastActiveTime = lastActiveTime;
            }

            public int LastActiveTime { get; private set; }

            public void MarkActiveAtTime(int frameTime)
            {
                LastActiveTime = Math.Max(LastActiveTime, frameTime);
            }
        }

        readonly List<IChunkManagerData> mapData;
        readonly BufferList<int> levelsToRemove;
        readonly Dictionary<int, LevelTrackingData> levelData;
        readonly Lazy<ITimeSource> timeSource;
        readonly int inactiveTimeOut;
        readonly int adjacentKeptAliveRange;
        readonly Lazy<IMapLevelDataSource<int>> levelDataSource;

        int frameTime;

        public FullLevelChunkManager([NotNull] Lazy<ITimeSource> timeSource,
                                     [NotNull] Lazy<IMapLevelDataSource<int>> levelDataSource,
                                     int adjacentKeptAliveRange = 0,
                                     int inactiveTimeOut = 1)
        {
            if (inactiveTimeOut < 0)
            {
                throw new ArgumentException(nameof(inactiveTimeOut));
            }

            if (adjacentKeptAliveRange < 0)
            {
                throw new ArgumentException(nameof(adjacentKeptAliveRange));
            }

            this.timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            this.levelDataSource = levelDataSource ?? throw new ArgumentNullException(nameof(levelDataSource));
            this.inactiveTimeOut = inactiveTimeOut;
            this.adjacentKeptAliveRange = adjacentKeptAliveRange.Clamp(0, int.MaxValue);
            this.levelsToRemove = new BufferList<int>();
            this.levelData = new Dictionary<int, LevelTrackingData>();
            this.mapData = new List<IChunkManagerData>();
        }

        public bool CanCreateLevel(int level)
        {
            return levelDataSource.Value.CanCreateLevel(level);
        }

        public void Activate()
        {
            levelDataSource.Value.LevelCreateComplete += OnLevelCreated;
            levelDataSource.Value.LevelWriteBackComplete += OnLevelWrittenBack;
            levelDataSource.Value.LevelUnloadComplete += OnLevelUnloaded;

            foreach (var m in mapData)
            {
                m.ViewCreated += OnViewCreated;
                m.ViewExpired += OnViewExpired;
                m.ViewMarkedDirty += OnViewDirty;
            }
        }

        /// <summary>
        ///   Called during module initialization.
        /// </summary>
        /// <param name="d"></param>
        public void AddMap(IChunkManagerData d)
        {
            mapData.Add(d);
        }

        public void Dispose()
        {
            Deactivate();
            mapData.Clear();
        }

        public void Deactivate()
        {
            foreach (var m in mapData)
            {
                m.ViewCreated -= OnViewCreated;
                m.ViewExpired -= OnViewExpired;
                m.ViewMarkedDirty -= OnViewDirty;
                m.Dispose();
            }

            levelDataSource.Value.LevelCreateComplete -= OnLevelCreated;
            levelDataSource.Value.LevelWriteBackComplete -= OnLevelWrittenBack;
            levelDataSource.Value.LevelUnloadComplete -= OnLevelUnloaded;
        }

        void OnLevelUnloaded(object sender, int e)
        {
            if (levelData.TryGetValue(e, out var ld))
            {
                ld.Unloaded = false;
            }
        }

        void OnLevelCreated(object sender, int levelNumber)
        {
            if (levelData.TryGetValue(levelNumber, out var ld))
            {
                ld.CreateRequested = false;
            }
            else
            {
                ld = new LevelTrackingData(frameTime);
                levelData[levelNumber] = ld;
            }
        }

        void OnLevelWrittenBack(object sender, int levelNumber)
        {
            if (levelData.TryGetValue(levelNumber, out var ld))
            {
                ld.Dirty = false;
            }
        }

        void OnViewDirty(object sender, int pos)
        {
            if (levelData.TryGetValue(pos, out var ld))
            {
                ld.Dirty |= !ld.CreateRequested;
                ld.MarkActiveAtTime(frameTime);
            }
        }

        void OnViewCreated(object sender, int z)
        {
            if (!levelData.TryGetValue(z, out var ld))
            {
                levelData[z] = new LevelTrackingData(frameTime);
            }
        }

        void OnViewExpired(object sender, int z)
        {
            if (levelData.TryGetValue(z, out var ld))
            {
                levelData.Remove(z);
            }
        }

        /// <summary>
        ///   System Action Step 1: Initialize marking data
        /// </summary>
        public void BeginMarkPhase()
        {
            frameTime = timeSource.Value.FixedStepTime;
            foreach (var l in levelData)
            {
                if (l.Value.CreateRequested)
                {
                    l.Value.MarkActiveAtTime(frameTime);
                }
            }
        }

        /// <summary>
        ///   Action Step 2: Process all active maps. Find all observers and mark the observed position active.
        /// </summary>
        public void ProcessObservers<TEntity, TPosition>(IEntityViewControl<TEntity> v, TEntity k, in PlayerObserver obs, in TPosition pos)
            where TEntity : IEntityKey
            where TPosition : IPosition<TPosition>
        {
            if (pos.IsInvalid)
            {
                return;
            }

            MarkLayerActive(pos);
        }

        /// <summary>
        ///   Indirect Action Step 2: Process all active maps. The observer manager deals with this.
        /// </summary>
        void MarkLayerActive<TPosition>(TPosition pos)
            where TPosition : IPosition<TPosition>
        {
            if (pos.IsInvalid)
            {
                return;
            }

            for (var levelNum = -adjacentKeptAliveRange; levelNum <= adjacentKeptAliveRange; levelNum += 1)
            {
                var levelId = pos.GridZ + levelNum;
                if (this.levelData.TryGetValue(levelId, out var level))
                {
                    level.MarkActiveAtTime(frameTime);
                }
                else if (levelDataSource.Value.TryCreateMapLevel(levelId))
                {
                    this.levelData[levelId] = new LevelTrackingData(frameTime)
                    {
                        CreateRequested = true
                    };
                }
            }
        }

        /// <summary>
        ///   Prepares to remove all inactive layers. 
        /// </summary>
        public void FinalizeMarkPhase()
        {
            levelsToRemove.Clear();

            foreach (var l in levelData)
            {
                var z = l.Key;
                var data = l.Value;

                if (data.CreateRequested)
                {
                    continue;
                }

                if ((data.LastActiveTime + inactiveTimeOut) >= frameTime)
                {
                    continue;
                }

                if (data.Dirty)
                {
                    // try write back
                    if (levelDataSource.Value.TryWriteBackMapLevel(z))
                    {
                        continue;
                    }
                }

                if (data.Dirty == false)
                {
                    levelsToRemove.Add(z);
                }
            }

            foreach (var l in levelsToRemove)
            {
                if (!levelDataSource.Value.TryPrepareRemoveMapLevel(l))
                {
                    continue;
                }

                foreach (var m in mapData)
                {
                    m.RemoveView(l);
                }

                levelData.Remove(l);
            }
        }

        public MapChunkLoadingResult TryLoadMap(int levelId)
        {
            if (this.levelData.TryGetValue(levelId, out var level))
            {
                if (level.CreateRequested)
                {
                    return MapChunkLoadingResult.LevelNotLoaded;
                }

                level.MarkActiveAtTime(frameTime);
                return MapChunkLoadingResult.Success;
            }

            if (levelDataSource.Value.TryCreateMapLevel(levelId))
            {
                this.levelData[levelId] = new LevelTrackingData(frameTime)
                {
                    CreateRequested = true
                };
                return MapChunkLoadingResult.LevelNotLoaded;
            }

            return MapChunkLoadingResult.NoSuchLevel;
        }
    }
}
