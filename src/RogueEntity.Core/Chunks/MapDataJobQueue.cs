using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Chunks
{
    public enum JobType
    {
        LoadOrCreate,
        Write,
        Unload
    }

    public class MapDataJobQueue<TJobKey, TJobState>
    {
        static readonly ILogger Logger = SLog.ForContext<MapDataJobQueue<TJobKey, TJobState>>();
        static readonly EqualityComparer<TJobKey> keyEqualityComparer = EqualityComparer<TJobKey>.Default;

        class JobEntry
        {
            public JobType ScheduledJobType;
            public Optional<TJobState> JobProgress;
            public bool JobStarted => JobProgress.HasValue;
        }
        
        readonly Dictionary<TJobKey, JobEntry> jobs;
        readonly CircularBuffer<TJobKey> loadQueue;
        readonly CircularBuffer<TJobKey> unloadQueue;
        readonly CircularBuffer<TJobKey> writeQueue;

        public MapDataJobQueue()
        {
            this.jobs = new Dictionary<TJobKey, JobEntry>();
            this.loadQueue = new CircularBuffer<TJobKey>(50, 5);
            this.unloadQueue = new CircularBuffer<TJobKey>(50, 5);
            this.writeQueue = new CircularBuffer<TJobKey>(50, 5);
        }

        public bool SubmitLoad(TJobKey job)
        {
            if (jobs.TryGetValue(job, out var existingData))
            {
                switch (existingData.ScheduledJobType)
                {
                    case JobType.LoadOrCreate:
                    case JobType.Write:
                        return true;
                    case JobType.Unload:
                        if (!existingData.JobStarted)
                        {
                            unloadQueue.RemoveAll(job);
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
            }

            jobs[job] = new JobEntry()
            {
                ScheduledJobType = JobType.LoadOrCreate
            };
            loadQueue.PushBack(job);
            return true;
        }

        public bool SubmitUnload(TJobKey job)
        {
            if (jobs.TryGetValue(job, out var existingData))
            {
                switch (existingData.ScheduledJobType)
                {
                    case JobType.LoadOrCreate:
                        if (existingData.JobStarted)
                        {
                            return false;
                        }

                        loadQueue.RemoveAll(job);
                        break;
                    case JobType.Write:
                        if (existingData.JobStarted)
                        {
                            return false;
                        }
                        
                        writeQueue.RemoveAll(job);
                        break;
                    case JobType.Unload:
                        return true;
                }
            }

            jobs[job] = new JobEntry()
            {
                ScheduledJobType = JobType.Unload
            };
            unloadQueue.PushBack(job);
            return true;
        }

        public bool SubmitWrite(TJobKey job)
        {
            if (jobs.TryGetValue(job, out var existingData))
            {
                switch (existingData.ScheduledJobType)
                {
                    case JobType.LoadOrCreate:
                        return false;
                    case JobType.Write:
                        return true;
                    case JobType.Unload:
                    {
                        if (existingData.JobStarted)
                        {
                            return false;
                        }

                        unloadQueue.RemoveAll(job);
                        break;
                    }
                }
            }

            jobs[job] = new JobEntry()
            {
                ScheduledJobType = JobType.Write
            };
            writeQueue.PushBack(job);
            return true;
        }

        public bool TryPeek(JobType t, out TJobKey job, out Optional<TJobState> state)
        {
            var buffer = GetBufferForJobType(t);

            if (buffer.IsEmpty)
            {
                job = default;
                state = default;
                return false;
            }

            job = buffer.Front();
            if (jobs.TryGetValue(job, out var jobEntry))
            {
                state = jobEntry.JobProgress;
                return true;
            }
            
            // Correct invalid job state after complaining to the logger
            jobs[job] = new JobEntry()
            {
                ScheduledJobType = t
            };
            state = default;
            return true;
        }

        public void RecordJobDone(JobType t, TJobKey k)
        {
            var buffer = GetBufferForJobType(t);

            if (buffer.IsEmpty)
            {
                Logger.Warning("Empty job queue when recording finished job for {JobType}:{Job}", t, k);
                jobs.Remove(k);
                return;
            }

            var job = buffer.Front();
            if (!keyEqualityComparer.Equals(job, k))
            {
                Logger.Warning("Job queue head entry mismatch for {JobType}:{Job}", t, job);
                jobs.Remove(k);
                return;
            }

            if (!jobs.TryGetValue(job, out var jobData))
            {
                Logger.Warning("Unable to fetch job data for job {JobType}:{Job}", t, job);
                buffer.PopFront();
            }
            else
            {
                jobData.JobProgress = Optional.Empty();
                jobs.Remove(k);
                buffer.PopFront();
                return;
            }
        }

        CircularBuffer<TJobKey> GetBufferForJobType(JobType t)
        {
            CircularBuffer<TJobKey> buffer = t switch
            {
                JobType.LoadOrCreate => loadQueue,
                JobType.Write => writeQueue,
                JobType.Unload => unloadQueue,
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
            };
            return buffer;
        }

        public void RecordProgress(JobType t, TJobKey k, TJobState stateInProgress)
        {
            if (!jobs.TryGetValue(k, out var jobData))
            {
                Logger.Warning("While recording progress, unable to fetch job data for job {JobType}:{Job}", t, k);
                jobs[k] = new JobEntry()
                {
                    JobProgress = stateInProgress
                };
                var buffer = GetBufferForJobType(t);
                buffer.PushFront(k);
            }
            else
            {
                jobData.JobProgress = stateInProgress;
            }
            
        }
    }
}
