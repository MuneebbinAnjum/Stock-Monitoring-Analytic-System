using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SalaryController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public SalaryController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpPost("set/{employeeId}")]
        public async Task<IActionResult> SetSalary(Guid employeeId, [FromBody] SetSalaryDto dto)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.Role != "Salesman")
                return NotFound(new ApiResponse<string> { Success = false, Message = "Salesman not found" });

            if (dto.MonthlySalary < 0)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Salary cannot be negative" });

            employee.MonthlySalary = dto.MonthlySalary;
            employee.UpdatedAt = DateTime.UtcNow;

            _context.Employees.Update(employee);

            // Audit log
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Employee",
                EntityId = employee.Id,
                Action = "SetSalary",
                PerformedBy = "Admin",
                PerformedAt = DateTime.UtcNow,
                Details = $"Salary set to {dto.MonthlySalary:C} for {employee.FullName}"
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Salary updated successfully" });
        }

        [HttpGet("summary/{employeeId}")]
        public async Task<IActionResult> GetSalarySummary(Guid employeeId, [FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || employee.Role != "Salesman")
                return NotFound(new ApiResponse<string> { Success = false, Message = "Salesman not found" });

            var currentDate = DateTime.UtcNow;
            var queryMonth = month ?? currentDate.Month;
            var queryYear = year ?? currentDate.Year;

            var startDate = new DateTime(queryYear, queryMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get all sales for this salesman in this month
            var saleRecords = await _context.SaleRecords
                .Where(sr => sr.EmployeeId == employeeId && 
                            sr.SaleDate >= startDate && 
                            sr.SaleDate <= endDate)
                .Include(sr => sr.Product)
                .ToListAsync();

            decimal totalCommission = 0;

            foreach (var sale in saleRecords)
            {
                var commission = await _context.Commissions
                    .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.ProductId == sale.ProductId);

                if (commission != null)
                {
                    var saleAmount = sale.Revenue; // Use stored Revenue on SaleRecord
                    totalCommission += (saleAmount * commission.CommissionPercentage) / 100;
                }
            }

            var summary = new SalarySummaryDto
            {
                SalesmanId = employee.Id,
                SalesmanName = employee.FullName,
                SalesmanEmail = employee.Email,
                MonthlySalary = employee.MonthlySalary,
                TotalCommissionEarned = totalCommission,
                TotalAmountDue = employee.MonthlySalary + totalCommission,
                Month = queryMonth,
                Year = queryYear,
                SalesRecordsCount = saleRecords.Count
            };

            return Ok(new ApiResponse<SalarySummaryDto> { Success = true, Data = summary });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSalesmanSalaries()
        {
            var salesmen = await _context.Employees
                .Where(e => e.Role == "Salesman")
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var summaries = new List<SalarySummaryDto>();

            foreach (var salesman in salesmen)
            {
                var saleRecords = await _context.SaleRecords
                    .Where(sr => sr.EmployeeId == salesman.Id && 
                                sr.SaleDate >= startDate && 
                                sr.SaleDate <= endDate)
                    .Include(sr => sr.Product)
                    .ToListAsync();

                decimal totalCommission = 0;

                foreach (var sale in saleRecords)
                {
                    var commission = await _context.Commissions
                        .FirstOrDefaultAsync(c => c.EmployeeId == salesman.Id && c.ProductId == sale.ProductId);

                    if (commission != null)
                    {
                        var saleAmount = sale.Revenue;
                        totalCommission += (saleAmount * commission.CommissionPercentage) / 100;
                    }
                }

                summaries.Add(new SalarySummaryDto
                {
                    SalesmanId = salesman.Id,
                    SalesmanName = salesman.FullName,
                    SalesmanEmail = salesman.Email,
                    MonthlySalary = salesman.MonthlySalary,
                    TotalCommissionEarned = totalCommission,
                    TotalAmountDue = salesman.MonthlySalary + totalCommission,
                    Month = currentMonth,
                    Year = currentYear,
                    SalesRecordsCount = saleRecords.Count
                });
            }

            return Ok(new ApiResponse<List<SalarySummaryDto>> { Success = true, Data = summaries });
        }
    }
}
