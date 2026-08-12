using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Data;
using System.Linq;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly SmasDbContext _context;
        private readonly IReportService _reportService;

        public SalesController(SmasDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _context.SaleRecords
                .Include(sr => sr.Product)
                .Include(sr => sr.Employee)
                .ToListAsync();

            var response = sales.Select(sr => new
            {
                sr.Id,
                ProductName = sr.Product?.Name,
                EmployeeName = sr.Employee?.FullName,
                sr.SaleDate,
                sr.QuantitySold,
                sr.Revenue
            });

            return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = response });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("report")]
        public async Task<IActionResult> GetReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportService.GenerateSalesSummaryAsync(new DateRangeDto { StartDate = startDate, EndDate = endDate });
            return Ok(new ApiResponse<SalesSummaryDto> { Success = true, Data = report });
        }
    }
}