using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForecastController : ControllerBase
    {
        private readonly IForecastService _forecastService;

        public ForecastController(IForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetForecast(Guid productId)
        {
            var forecast = await _forecastService.GetForecastForProductAsync(productId);
            return Ok(new ApiResponse<ForecastDto> { Success = true, Data = forecast });
        }
    }
}