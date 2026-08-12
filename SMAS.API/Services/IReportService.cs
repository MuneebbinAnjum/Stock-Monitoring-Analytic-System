using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public interface IReportService
    {
        Task<SalesSummaryDto> GenerateSalesSummaryAsync(DateRangeDto dateRange);
        Task<InventoryTurnoverDto> GenerateInventoryTurnoverReportAsync();
        Task<IEnumerable<RevenueBreakdownDto>> GenerateRevenueBreakdownAsync(string groupBy);
        Task<IEnumerable<RevenueBreakdownDto>> GenerateSalesByLocationAsync();
        Task<string> ExportReportAsync(string reportType, string format, DateRangeDto dateRange);
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SalesSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUnitsSold { get; set; }
        public Dictionary<string, decimal> DailyRevenue { get; set; } = new();
        public Dictionary<string, decimal> WeeklyRevenue { get; set; } = new();
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();
    }

    public class InventoryTurnoverDto
    {
        public decimal TurnoverRatio { get; set; }
        public List<ProductTurnoverDto> ProductTurnovers { get; set; } = new();
    }

    public class ProductTurnoverDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal AverageInventory { get; set; }
        public decimal TurnoverRatio { get; set; }
    }

    public class RevenueBreakdownDto
    {
        public string GroupName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }
}