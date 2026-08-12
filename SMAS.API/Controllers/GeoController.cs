using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeoController : ControllerBase
    {
        private readonly IGeoAnalyticsService _geoService;

        public GeoController(IGeoAnalyticsService geoService)
        {
            _geoService = geoService;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("heatmap")]
        public async Task<IActionResult> Heatmap()
        {
            var data = await _geoService.GetHeatmapDataAsync();
            return Ok(new ApiResponse<IEnumerable<HeatmapDataDto>> { Success = true, Data = data });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("cities")]
        public async Task<IActionResult> Cities()
        {
            var data = await _geoService.GetSalesByCityAsync();
            return Ok(new ApiResponse<Dictionary<string, decimal>> { Success = true, Data = data });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("provinces")]
        public async Task<IActionResult> Provinces()
        {
            var data = await _geoService.GetSalesByProvinceAsync();
            return Ok(new ApiResponse<Dictionary<string, decimal>> { Success = true, Data = data });
        }
    }
}