using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public interface IForecastService
    {
        Task<decimal> PredictDemandAsync(Guid productId, int daysAhead);
        Task UpdateTrendScoresAsync();
        Task<ForecastDto> GetForecastForProductAsync(Guid productId);
    }

    public class ForecastDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal PredictedDemand { get; set; }
        public decimal TrendScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }
}