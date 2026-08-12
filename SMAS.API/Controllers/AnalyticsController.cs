using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly SmasDbContext _context;
        private readonly IGeoAnalyticsService _geoService;

        public AnalyticsController(SmasDbContext context, IGeoAnalyticsService geoService)
        {
            _context = context;
            _geoService = geoService;
        }

        // GET: /api/analytics/sales-heatmap
        [HttpGet("sales-heatmap")]
        public async Task<IActionResult> GetSalesHeatmap()
        {
            var data = await _geoService.GetHeatmapDataAsync();
            return Ok(new ApiResponse<IEnumerable<RegionPerformanceDto>> { Success = true, Data = data.Select(d => new RegionPerformanceDto { Region = d.City, Revenue = d.Revenue, OrderCount = d.OrderCount }) });
        }

        // GET: /api/analytics/top-regions?topN=10
        [HttpGet("top-regions")]
        public async Task<IActionResult> GetTopRegions([FromQuery] int topN = 10)
        {
            var data = await _geoService.GetTopPerformingRegionsAsync(topN);
            return Ok(new ApiResponse<IEnumerable<RegionPerformanceDto>> { Success = true, Data = data });
        }

        // GET: /api/analytics/top-salesmen?topN=10&month=5&year=2026&metric=revenue
        [HttpGet("top-salesmen")]
        public async Task<IActionResult> GetTopSalesmen([FromQuery] int topN = 10, [FromQuery] int? month = null, [FromQuery] int? year = null, [FromQuery] string metric = "revenue")
        {
            var q = _context.SaleRecords.Include(sr => sr.Employee).AsQueryable();
            if (month.HasValue && year.HasValue)
            {
                q = q.Where(sr => sr.SaleDate.Month == month.Value && sr.SaleDate.Year == year.Value);
            }
            else if (month.HasValue)
            {
                q = q.Where(sr => sr.SaleDate.Month == month.Value);
            }
            else if (year.HasValue)
            {
                q = q.Where(sr => sr.SaleDate.Year == year.Value);
            }

            var grouped = await q.GroupBy(sr => new { sr.EmployeeId, Name = sr.Employee != null ? sr.Employee.FullName : string.Empty })
                                 .Select(g => new
                                 {
                                     EmployeeId = g.Key.EmployeeId,
                                     EmployeeName = g.Key.Name,
                                     Revenue = g.Sum(x => x.Revenue),
                                     Quantity = g.Sum(x => x.QuantitySold)
                                 })
                                 .ToListAsync();

            var result = grouped.Select(g => new TopSalesmanDto
            {
                EmployeeId = g.EmployeeId,
                EmployeeName = g.EmployeeName,
                Revenue = g.Revenue,
                QuantitySold = g.Quantity
            });

            result = metric.ToLower() == "quantity" ? result.OrderByDescending(r => r.QuantitySold) : result.OrderByDescending(r => r.Revenue);

            return Ok(new ApiResponse<IEnumerable<TopSalesmanDto>> { Success = true, Data = result.Take(topN) });
        }

        // GET: /api/analytics/top-products?topN=10&month=5&year=2026
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int topN = 10, [FromQuery] int? month = null, [FromQuery] int? year = null, [FromQuery] Guid? category = null)
        {
            var q = _context.SaleRecords.Include(sr => sr.Product!).ThenInclude(p => p.Category!).AsQueryable();
            if (month.HasValue && year.HasValue)
            {
                q = q.Where(sr => sr.SaleDate.Month == month.Value && sr.SaleDate.Year == year.Value);
            }
            else if (month.HasValue)
            {
                q = q.Where(sr => sr.SaleDate.Month == month.Value);
            }
            else if (year.HasValue)
            {
                q = q.Where(sr => sr.SaleDate.Year == year.Value);
            }

            if (category.HasValue)
            {
                q = q.Where(sr => sr.Product != null && sr.Product.CategoryId == category.Value);
            }

            var grouped = await q.GroupBy(sr => new { sr.ProductId, Name = sr.Product != null ? sr.Product.Name : string.Empty, CategoryName = sr.Product != null && sr.Product.Category != null ? sr.Product.Category.Name : string.Empty })
                                 .Select(g => new
                                 {
                                     ProductId = g.Key.ProductId,
                                     ProductName = g.Key.Name,
                                     CategoryName = g.Key.CategoryName,
                                     Revenue = g.Sum(x => x.Revenue),
                                     Quantity = g.Sum(x => x.QuantitySold)
                                 })
                                 .ToListAsync();

            var result = grouped.Select(g => new TopProductDto
            {
                ProductId = g.ProductId,
                ProductName = g.ProductName,
                CategoryName = g.CategoryName,
                Revenue = g.Revenue,
                QuantitySold = g.Quantity
            }).OrderByDescending(r => r.Revenue).Take(topN);

            return Ok(new ApiResponse<IEnumerable<TopProductDto>> { Success = true, Data = result });
        }

        // GET: /api/analytics/order-status-distribution?start=2026-01-01&end=2026-05-01
        [HttpGet("order-status-distribution")]
        public async Task<IActionResult> GetOrderStatusDistribution([FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
        {
            var q = _context.Orders.AsQueryable();
            if (start.HasValue) q = q.Where(o => o.OrderDate >= start.Value);
            if (end.HasValue) q = q.Where(o => o.OrderDate <= end.Value);

            var grouped = await q.GroupBy(o => o.Status)
                                 .Select(g => new { Status = g.Key, Count = g.Count() })
                                 .ToListAsync();

            var result = grouped.Select(g => new StatusDistributionDto { Status = g.Status ?? "Unknown", Count = g.Count });
            return Ok(new ApiResponse<IEnumerable<StatusDistributionDto>> { Success = true, Data = result });
        }

        // GET: /api/analytics/forecast-vs-actual?start=2026-01-01&end=2026-05-01&granularity=day
        [HttpGet("forecast-vs-actual")]
        public async Task<IActionResult> GetForecastVsActual([FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string granularity = "day")
        {
            var fQuery = _context.ForecastRecords.AsQueryable();
            var sQuery = _context.SaleRecords.AsQueryable();
            if (start.HasValue)
            {
                fQuery = fQuery.Where(f => f.ForecastDate >= start.Value.Date);
                sQuery = sQuery.Where(s => s.SaleDate >= start.Value.Date);
            }
            if (end.HasValue)
            {
                fQuery = fQuery.Where(f => f.ForecastDate <= end.Value.Date);
                sQuery = sQuery.Where(s => s.SaleDate <= end.Value.Date);
            }

            var forecasts = await fQuery.ToListAsync();
            var sales = await sQuery.ToListAsync();

            Func<DateTime, string> keyFor = dt => granularity.ToLower() switch
            {
                "month" => dt.ToString("yyyy-MM"),
                "week" => $"{dt.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(dt)}",
                _ => dt.ToString("yyyy-MM-dd")
            };

            var forecastGrouped = forecasts.GroupBy(f => keyFor(f.ForecastDate)).ToDictionary(g => g.Key, g => g.Sum(x => x.PredictedDemand));
            var salesGrouped = sales.GroupBy(s => keyFor(s.SaleDate)).ToDictionary(g => g.Key, g => g.Sum(x => x.QuantitySold));

            var keys = forecastGrouped.Keys.Union(salesGrouped.Keys).OrderBy(k => k);

            var result = keys.Select(k => new ForecastVsActualDto
            {
                Period = k,
                ForecastQuantity = forecastGrouped.ContainsKey(k) ? forecastGrouped[k] : 0,
                ActualQuantity = salesGrouped.ContainsKey(k) ? salesGrouped[k] : 0
            });

            return Ok(new ApiResponse<IEnumerable<ForecastVsActualDto>> { Success = true, Data = result });
        }
    }

    // ----- DTOs specific to analytics controller -----
    public class TopSalesmanDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int QuantitySold { get; set; }
    }

    public class TopProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int QuantitySold { get; set; }
    }

    public class StatusDistributionDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ForecastVsActualDto
    {
        public string Period { get; set; } = string.Empty;
        public int ForecastQuantity { get; set; }
        public int ActualQuantity { get; set; }
    }
}

