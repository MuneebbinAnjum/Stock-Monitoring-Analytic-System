using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeKPIDto> GetKPIDashboardAsync(Guid employeeId, int month, int year);
        Task<IEnumerable<EmployeeLeaderboardDto>> GetSalesLeaderboardAsync(int month, int year);
        Task<EmployeeResponseDto> RegisterEmployeeAsync(EmployeeCreateDto dto);
        Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid id, EmployeeUpdateDto dto);
    }

    public class EmployeeKPIDto
    {
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public int Rank { get; set; }
        public decimal TargetPercentage { get; set; }
        public List<DailySalesDto> DailyBreakdown { get; set; } = new();
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class EmployeeLeaderboardDto
    {
        public Guid EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int Rank { get; set; }
    }
}