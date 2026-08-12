using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.Services;

namespace SMAS.API.Services
{
    public class GeoAnalyticsService : IGeoAnalyticsService
    {
        private readonly SmasDbContext _context;

        public GeoAnalyticsService(SmasDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, decimal>> GetSalesByCityAsync()
        {
            return await _context.Orders
                .GroupBy(o => o.DeliveryCity)
                .Select(g => new { City = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
                .ToDictionaryAsync(x => x.City ?? string.Empty, x => x.Revenue);
        }

        public async Task<Dictionary<string, decimal>> GetSalesByProvinceAsync()
        {
            return await _context.Customers
                .GroupBy(c => c.Province)
                .Select(g => new { Province = g.Key, Revenue = g.Sum(c => 0m) }) // Placeholder since orders track city only
                .ToDictionaryAsync(x => x.Province ?? string.Empty, x => x.Revenue);
        }

        public async Task<IEnumerable<RegionPerformanceDto>> GetTopPerformingRegionsAsync(int topN)
        {
            var byCity = await _context.Orders
                .GroupBy(o => o.DeliveryCity)
                .Select(g => new RegionPerformanceDto
                {
                    Region = g.Key ?? string.Empty,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(topN)
                .ToListAsync();

            return byCity;
        }

        public async Task<IEnumerable<HeatmapDataDto>> GetHeatmapDataAsync()
        {
            var data = await _context.Orders
                .GroupBy(o => o.DeliveryCity)
                .Select(g => new
                {
                    City = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Count = g.Count()
                })
                .ToListAsync();

            var maxRevenue = data.Max(d => d.Revenue);
            return data.Select(d => new HeatmapDataDto
            {
                City = d.City ?? string.Empty,
                Province = string.Empty,
                Revenue = d.Revenue,
                OrderCount = d.Count,
                Intensity = maxRevenue > 0 ? (int)Math.Round((d.Revenue / maxRevenue) * 100) : 0
            });
        }
    }
}