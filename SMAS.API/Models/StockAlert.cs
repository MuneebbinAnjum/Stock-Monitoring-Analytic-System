namespace SMAS.API.Models
{
    public class StockAlert : Entity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsResolved { get; set; } = false;
    }
}