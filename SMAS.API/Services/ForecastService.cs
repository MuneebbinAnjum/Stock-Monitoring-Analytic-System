using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public class ForecastService : IForecastService
    {
        private readonly SmasDbContext _context;

        public ForecastService(SmasDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> PredictDemandAsync(Guid productId, int daysAhead)
        {
            var sales = await _context.SaleRecords
                .Where(sr => sr.ProductId == productId)
                .OrderByDescending(sr => sr.SaleDate)
                .Take(30)
                .ToListAsync();

            if (!sales.Any()) return 0;

            var averageDaily = sales.Sum(s => (decimal)s.QuantitySold) / 30m;
            var trend = sales.Zip(sales.Skip(1), (a, b) => (decimal)(a.QuantitySold - b.QuantitySold)).Average();

            return Math.Max(0m, averageDaily + (trend * daysAhead / 30m));
        }

        public async Task UpdateTrendScoresAsync()
        {
            var products = await _context.Products.ToListAsync();

            foreach (var product in products)
            {
                var sales = await _context.SaleRecords
                    .Where(sr => sr.ProductId == product.Id)
                    .OrderBy(sr => sr.SaleDate)
                    .ToListAsync();

                if (sales.Count < 2) continue;

                var trend = sales.Zip(sales.Skip(1), (a, b) => (decimal)(b.QuantitySold - a.QuantitySold)).Average();
                var forecast = await _context.ForecastRecords
                    .FirstOrDefaultAsync(fr => fr.ProductId == product.Id);

                if (forecast == null)
                {
                    forecast = new Models.ForecastRecord
                    {
                        ProductId = product.Id,
                        ForecastDate = DateTime.UtcNow.Date,
                        PredictedDemand = (int)Math.Round(await PredictDemandAsync(product.Id, 7)),
                        TrendScore = trend
                    };
                    _context.ForecastRecords.Add(forecast);
                }
                else
                {
                    forecast.PredictedDemand = (int)Math.Round(await PredictDemandAsync(product.Id, 7));
                    forecast.TrendScore = trend;
                    forecast.UpdatedAt = DateTime.UtcNow;
                    
                    // Explicitly mark entity as modified to ensure EF Core tracks changes
                    _context.Update(forecast);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ForecastDto> GetForecastForProductAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new KeyNotFoundException("Product not found");

            var forecast = await _context.ForecastRecords
                .FirstOrDefaultAsync(fr => fr.ProductId == productId);

            var predicted = forecast?.PredictedDemand ?? (int)Math.Round(await PredictDemandAsync(productId, 7));
            var trend = forecast?.TrendScore ?? 0;

            string recommendation;
            if (predicted > product.StockQuantity)
                recommendation = "Increase stock - high demand expected";
            else if (predicted < product.ReorderLevel)
                recommendation = "Low demand - consider reducing stock";
            else
                recommendation = "Demand stable - maintain current levels";

            return new ForecastDto
            {
                ProductId = productId,
                ProductName = product.Name,
                PredictedDemand = predicted,
                TrendScore = trend,
                Recommendation = recommendation
            };
        }
    }
}