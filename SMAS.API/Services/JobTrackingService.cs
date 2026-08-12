using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    /// <summary>
    /// Service for tracking long-running background jobs
    /// </summary>
    public interface IJobTrackingService
    {
        /// <summary>
        /// Creates a new job tracking entry
        /// </summary>
        string CreateJob(string jobType);

        /// <summary>
        /// Gets the status of a job
        /// </summary>
        JobStatusDto? GetJobStatus(string jobId);

        /// <summary>
        /// Updates the progress of a job
        /// </summary>
        void UpdateJobProgress(string jobId, int progressPercentage);

        /// <summary>
        /// Marks a job as completed with an optional result
        /// </summary>
        void CompleteJob(string jobId, string? result = null);

        /// <summary>
        /// Marks a job as failed with an error message
        /// </summary>
        void FailJob(string jobId, string errorMessage);

        /// <summary>
        /// Cleans up old completed jobs (older than specified days)
        /// </summary>
        void CleanupOldJobs(int daysToKeep = 7);
    }

    public class JobTrackingService : IJobTrackingService
    {
        private readonly Dictionary<string, JobStatusDto> _jobs = new();
        private readonly object _lockObject = new();

        public string CreateJob(string jobType)
        {
            var jobId = Guid.NewGuid().ToString();
            var job = new JobStatusDto
            {
                JobId = jobId,
                JobType = jobType,
                Status = JobStatus.Pending,
                ProgressPercentage = 0,
                CreatedAt = DateTime.UtcNow
            };

            lock (_lockObject)
            {
                _jobs[jobId] = job;
            }

            return jobId;
        }

        public JobStatusDto? GetJobStatus(string jobId)
        {
            lock (_lockObject)
            {
                return _jobs.TryGetValue(jobId, out var job) ? job : null;
            }
        }

        public void UpdateJobProgress(string jobId, int progressPercentage)
        {
            lock (_lockObject)
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.ProgressPercentage = Math.Min(progressPercentage, 100);
                    job.Status = JobStatus.Processing;
                }
            }
        }

        public void CompleteJob(string jobId, string? result = null)
        {
            lock (_lockObject)
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.Status = JobStatus.Completed;
                    job.ProgressPercentage = 100;
                    job.Result = result;
                    job.CompletedAt = DateTime.UtcNow;
                }
            }
        }

        public void FailJob(string jobId, string errorMessage)
        {
            lock (_lockObject)
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.Status = JobStatus.Failed;
                    job.ErrorMessage = errorMessage;
                    job.CompletedAt = DateTime.UtcNow;
                }
            }
        }

        public void CleanupOldJobs(int daysToKeep = 7)
        {
            lock (_lockObject)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldJobs = _jobs
                    .Where(kvp => kvp.Value.CompletedAt.HasValue && kvp.Value.CompletedAt < cutoffDate)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var jobId in oldJobs)
                {
                    _jobs.Remove(jobId);
                }
            }
        }
    }
}
