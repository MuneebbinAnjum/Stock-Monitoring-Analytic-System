using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public interface IAlertService
    {
        Task<IEnumerable<StockAlertResponseDto>> GetUnresolvedAlertsAsync();
        Task<IEnumerable<StockAlertResponseDto>> GetAllAlertsAsync();
        Task ResolveAlertAsync(Guid alertId);
        Task TriggerAlertsCheckAsync();
    }

    public class StockAlertResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public DateTime TriggeredAt { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsResolved { get; set; }
    }
}