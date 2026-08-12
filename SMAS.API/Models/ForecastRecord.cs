namespace SMAS.API.Models
{
    public class ForecastRecord : Entity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime ForecastDate { get; set; }

        public int PredictedDemand { get; set; }
        public decimal TrendScore { get; set; } = 0;
    }
}