namespace SMAS.API.DTOs
{
    /// <summary>
    /// Represents the status of a long-running background job
    /// </summary>
    public class JobStatusDto
    {
        public string JobId { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public JobStatus Status { get; set; }
        public int ProgressPercentage { get; set; }
        public string? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Enum representing possible job states
    /// </summary>
    public enum JobStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}
