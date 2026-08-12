using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Exceptions;
using ApiValidationException = SMAS.API.Exceptions.ValidationException;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportStatusController : ControllerBase
    {
        private readonly IJobTrackingService _jobTrackingService;
        private readonly IReportService _reportService;
        private readonly ILogger<ReportStatusController> _logger;

        public ReportStatusController(
            IJobTrackingService jobTrackingService,
            IReportService reportService,
            ILogger<ReportStatusController> logger)
        {
            _jobTrackingService = jobTrackingService;
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Get the status of a report export job
        /// </summary>
        /// <param name="jobId">The job ID returned from the export endpoint</param>
        /// <returns>
        /// 200 OK with job status if found
        /// 404 Not Found if job ID does not exist
        /// </returns>
        [HttpGet("{jobId}")]
        public IActionResult GetStatus(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ApiValidationException("Job ID cannot be empty");
            }

            var jobStatus = _jobTrackingService.GetJobStatus(jobId);
            if (jobStatus == null)
            {
                throw new NotFoundException($"Job with ID '{jobId}' not found. It may have expired.");
            }

            var response = new ApiResponse<JobStatusDto>
            {
                Success = true,
                Data = jobStatus,
                Message = $"Job status: {jobStatus.Status}"
            };

            return Ok(response);
        }

        /// <summary>
        /// Export a report asynchronously
        /// </summary>
        /// <returns>
        /// 202 Accepted with job ID for polling
        /// Use the returned jobId to poll /api/reportstatus/{jobId} for completion
        /// </returns>
        [HttpPost("export")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ExportReport([FromQuery] string reportType, [FromQuery] string format, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportType) || string.IsNullOrWhiteSpace(format))
                {
                    throw new ApiValidationException(new Dictionary<string, string[]>
                    {
                        { "reportType", new[] { "Report type is required" } },
                        { "format", new[] { "Format is required" } }
                    });
                }

                // Create job tracking entry
                var jobId = _jobTrackingService.CreateJob($"export-{reportType}");
                _logger.LogInformation("Report export job created: {JobId} for type {ReportType}", jobId, reportType);

                // Fire and forget - start the export in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _jobTrackingService.UpdateJobProgress(jobId, 10);
                        var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
                        var result = await _reportService.ExportReportAsync(reportType, format, dateRange);
                        _jobTrackingService.CompleteJob(jobId, result);
                        _logger.LogInformation("Report export job completed: {JobId}", jobId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Report export job failed: {JobId}", jobId);
                        _jobTrackingService.FailJob(jobId, ex.Message);
                    }
                });

                // Return 202 Accepted with job tracking info
                var jobStatusDto = new JobStatusDto
                {
                    JobId = jobId,
                    JobType = $"export-{reportType}",
                    Status = JobStatus.Pending,
                    ProgressPercentage = 0,
                    CreatedAt = DateTime.UtcNow
                };

                return Accepted(new ApiResponse<JobStatusDto>
                {
                    Success = true,
                    Data = jobStatusDto,
                    Message = "Export job accepted. Poll /api/reportstatus/{jobId} to check progress."
                });
            }
            catch (ApiValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting report export");
                throw new ServiceUnavailableException("Could not start report export. Please try again.");
            }
        }
    }
}
