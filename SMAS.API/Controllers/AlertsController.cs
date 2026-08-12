using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetUnresolved()
        {
            var alerts = await _alertService.GetUnresolvedAlertsAsync();
            return Ok(new ApiResponse<IEnumerable<StockAlertResponseDto>> { Success = true, Data = alerts });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var alerts = await _alertService.GetAllAlertsAsync();
            return Ok(new ApiResponse<IEnumerable<StockAlertResponseDto>> { Success = true, Data = alerts });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            await _alertService.ResolveAlertAsync(id);
            return Ok(new ApiResponse<string> { Success = true, Message = "Alert resolved" });
        }
    }
}