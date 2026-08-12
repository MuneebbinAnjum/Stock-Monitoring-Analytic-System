using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public interface IGeoAnalyticsService
    {
        Task<Dictionary<string, decimal>> GetSalesByCityAsync();
        Task<Dictionary<string, decimal>> GetSalesByProvinceAsync();
        Task<IEnumerable<RegionPerformanceDto>> GetTopPerformingRegionsAsync(int topN);
        Task<IEnumerable<HeatmapDataDto>> GetHeatmapDataAsync();
    }

    public class RegionPerformanceDto
    {
        public string Region { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class HeatmapDataDto
    {
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Intensity { get; set; }
        public int OrderCount { get; set; }
    }
}