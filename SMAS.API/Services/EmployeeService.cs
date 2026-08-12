using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Models;
using SMAS.API.Repositories;

namespace SMAS.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ISaleRecordRepository _saleRepository;
        private readonly SmasDbContext _context;

        public EmployeeService(ISaleRecordRepository saleRepository, SmasDbContext context)
        {
            _saleRepository = saleRepository;
            _context = context;
        }

        public async Task<EmployeeKPIDto> GetKPIDashboardAsync(Guid employeeId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var sales = await _saleRepository.GetByEmployeeAsync(employeeId);
            var monthlySales = sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate);

            var unitsSold = monthlySales.Sum(s => s.QuantitySold);
            var revenue = monthlySales.Sum(s => s.Revenue);

            var employee = await _context.Employees.FindAsync(employeeId);
            var targetPercentage = employee != null && employee.MonthlySalesTarget > 0
                ? (revenue / employee.MonthlySalesTarget) * 100
                : 0;

            var allEmployeesRevenue = await _context.SaleRecords
                .Where(sr => sr.SaleDate >= startDate && sr.SaleDate <= endDate)
                .GroupBy(sr => sr.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, Revenue = g.Sum(sr => sr.Revenue) })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            var rank = allEmployeesRevenue.FindIndex(x => x.EmployeeId == employeeId) + 1;

            var dailyBreakdown = monthlySales
                .GroupBy(s => s.SaleDate)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    UnitsSold = g.Sum(s => s.QuantitySold),
                    Revenue = g.Sum(s => s.Revenue)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new EmployeeKPIDto
            {
                UnitsSold = unitsSold,
                Revenue = revenue,
                Rank = rank,
                TargetPercentage = targetPercentage,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<IEnumerable<EmployeeLeaderboardDto>> GetSalesLeaderboardAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var leaderboard = await _context.SaleRecords
                .Where(sr => sr.SaleDate >= startDate && sr.SaleDate <= endDate)
                .Include(sr => sr.Employee)
                .GroupBy(sr => sr.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    Name = g.First().Employee!.FullName,
                    TotalRevenue = g.Sum(sr => sr.Revenue)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToListAsync();

            return leaderboard.Select((x, index) => new EmployeeLeaderboardDto
            {
                EmployeeId = x.EmployeeId,
                Name = x.Name,
                TotalRevenue = x.TotalRevenue,
                Rank = index + 1
            });
        }

        public async Task<EmployeeResponseDto> RegisterEmployeeAsync(EmployeeCreateDto dto)
        {
            var employee = new Employee
            {
                FullName = dto.Name,
                Role = dto.Role,
                Email = dto.Email,
                HireDate = dto.HireDate,
                MonthlySalesTarget = dto.MonthlySalesTarget,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return new EmployeeResponseDto
            {
                Id = employee.Id,
                Name = employee.FullName,
                Role = employee.Role,
                Email = employee.Email,
                HireDate = employee.HireDate,
                MonthlySalesTarget = employee.MonthlySalesTarget,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }

        public async Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid id, EmployeeUpdateDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) throw new KeyNotFoundException("Employee not found");

            employee.FullName = dto.Name;
            employee.Role = dto.Role;
            employee.Email = dto.Email;
            employee.HireDate = dto.HireDate;
            employee.MonthlySalesTarget = dto.MonthlySalesTarget;

            // Explicitly mark entity as modified to ensure EF Core tracks changes
            _context.Update(employee);
            await _context.SaveChangesAsync();

            return new EmployeeResponseDto
            {
                Id = employee.Id,
                Name = employee.FullName,
                Role = employee.Role,
                Email = employee.Email,
                HireDate = employee.HireDate,
                MonthlySalesTarget = employee.MonthlySalesTarget,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }
    }
}