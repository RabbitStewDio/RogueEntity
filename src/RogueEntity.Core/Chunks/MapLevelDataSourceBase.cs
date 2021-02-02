using RogueEntity.Api.Utils;
using System;
using System.Diagnostics;

namespace RogueEntity.Core.Chunks
{
    public abstract class MapLevelDataSourceBase<TJobKey, TJobState>: IMapLevelDataSource<TJobKey>, IMapLevelDataSourceSystem
    {
        public event EventHandler<TJobKey> LevelCreateComplete;
        public event EventHandler<TJobKey> LevelWriteBackComplete;
        public event EventHandler<TJobKey> LevelUnloadComplete;
        
        readonly Stopwatch timer;
        readonly TimeSpan maximumProcessingTime;
        readonly MapDataJobQueue<TJobKey, TJobState> jobQueue;

        protected MapLevelDataSourceBase(TimeSpan maximumProcessingTime): this()
        {
            this.maximumProcessingTime = maximumProcessingTime;
        }

        protected MapLevelDataSourceBase()
        {
            timer = new Stopwatch();
            jobQueue = new MapDataJobQueue<TJobKey, TJobState>();
            maximumProcessingTime = TimeSpan.FromMilliseconds(1);
        }

        public bool TryPrepareRemoveMapLevel(TJobKey z)
        {
            return jobQueue.SubmitUnload(z);
        }

        public bool TryCreateMapLevel(TJobKey z)
        {
            if (CanCreateLevel(z))
            {
                return jobQueue.SubmitLoad(z);
            }

            return false;
        }

        public bool TryWriteBackMapLevel(TJobKey z)
        {
            return jobQueue.SubmitWrite(z);
        }

        public abstract bool CanCreateLevel(TJobKey z);
        
        public void UnloadChunks()
        {
            timer.Restart();
            while (jobQueue.TryPeek(JobType.Unload, out var zLevel, out var progress))
            {
                // do work
                if (PerformUnloadChunks(zLevel, progress).TryGetValue(out var stateInProgress))
                {
                    jobQueue.RecordProgress(JobType.Write, zLevel, stateInProgress);
                }
                else
                {
                    jobQueue.RecordJobDone(JobType.Write, zLevel);
                    LevelUnloadComplete?.Invoke(this, zLevel);
                }
                
                if (timer.Elapsed > maximumProcessingTime)
                {
                    return;
                }
            }
        }

        public void LoadChunks()
        {
            timer.Restart();
            while (jobQueue.TryPeek(JobType.LoadOrCreate, out var zLevel, out var progress))
            {
                // do work
                if (PerformLoadChunks(zLevel, progress).TryGetValue(out var stateInProgress))
                {
                    jobQueue.RecordProgress(JobType.Write, zLevel, stateInProgress);
                }
                else
                {
                    jobQueue.RecordJobDone(JobType.Write, zLevel);
                    LevelCreateComplete?.Invoke(this, zLevel);
                }
                
                if (timer.Elapsed > maximumProcessingTime)
                {
                    return;
                }
            }
        }


        public void WriteChunks()
        {
            timer.Restart();
            while (jobQueue.TryPeek(JobType.Write, out var zLevel, out var progress))
            {
                // do work
                if (PerformWriteChunks(zLevel, progress).TryGetValue(out var stateInProgress))
                {
                    jobQueue.RecordProgress(JobType.Write, zLevel, stateInProgress);
                }
                else
                {
                    jobQueue.RecordJobDone(JobType.Write, zLevel);
                    LevelWriteBackComplete?.Invoke(this, zLevel);
                }
                
                if (timer.Elapsed > maximumProcessingTime)
                {
                    return;
                }
            }
        }

        public void Deactivate()
        {
            while (jobQueue.TryPeek(JobType.Write, out var zLevel, out var progress))
            {
                // do work
                if (PerformWriteChunks(zLevel, progress).TryGetValue(out var stateInProgress))
                {
                    jobQueue.RecordProgress(JobType.Write, zLevel, stateInProgress);
                }
                else
                {
                    jobQueue.RecordJobDone(JobType.Write, zLevel);
                }
            }
        }
        
        protected abstract Optional<TJobState> PerformLoadChunks(in TJobKey key, in Optional<TJobState> progressSoFar);

        protected virtual Optional<TJobState> PerformWriteChunks(in TJobKey key, Optional<TJobState> progressSoFar)
        {
            return Optional.Empty();
        }
        
        protected virtual Optional<TJobState> PerformUnloadChunks(in TJobKey key, Optional<TJobState> progressSoFar)
        {
            return Optional.Empty();
        }
        
    }
}
