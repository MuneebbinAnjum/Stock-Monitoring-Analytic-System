using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("sales")]
        public async Task<IActionResult> GetSales([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportService.GenerateSalesSummaryAsync(new DateRangeDto { StartDate = startDate, EndDate = endDate });
            return Ok(new ApiResponse<SalesSummaryDto> { Success = true, Data = report });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var report = await _reportService.GenerateInventoryTurnoverReportAsync();
            return Ok(new ApiResponse<InventoryTurnoverDto> { Success = true, Data = report });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] string groupBy)
        {
            var report = await _reportService.GenerateRevenueBreakdownAsync(groupBy);
            return Ok(new ApiResponse<IEnumerable<RevenueBreakdownDto>> { Success = true, Data = report });
        }

        // Sales by location endpoint implemented above

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("sales-by-location")]
        public async Task<IActionResult> GetSalesByLocation()
        {
            var report = await _reportService.GenerateSalesByLocationAsync();
            return Ok(new ApiResponse<IEnumerable<RevenueBreakdownDto>> { Success = true, Data = report });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("export/{reportType}")]
        public async Task<IActionResult> Export(string reportType, [FromQuery] string format = "csv", [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
        {
            var dateRange = new DateRangeDto { StartDate = start ?? DateTime.UtcNow.AddDays(-30), EndDate = end ?? DateTime.UtcNow };
            var content = await _reportService.ExportReportAsync(reportType, format, dateRange);

            if (format.ToLower() == "csv")
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                return File(bytes, "text/csv", $"{reportType}-{DateTime.UtcNow:yyyyMMddHHmm}.csv");
            }

            if (format.Equals("excel", StringComparison.OrdinalIgnoreCase) || format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var bytes = Convert.FromBase64String(content);
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportType}-{DateTime.UtcNow:yyyyMMddHHmm}.xlsx");
                }
                catch
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Failed to generate Excel file" });
                }
            }

            return Ok(new ApiResponse<string> { Success = true, Data = content });
        }
    }
}