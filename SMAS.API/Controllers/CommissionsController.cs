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
    public class CommissionsController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public CommissionsController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommission([FromBody] CreateCommissionDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) 
                return NotFound(new ApiResponse<string> { Success = false, Message = "Employee not found" });

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Product not found" });

            // Check if commission already exists for this employee-product combination
            var existing = await _context.Commissions.FirstOrDefaultAsync(c => 
                c.EmployeeId == dto.EmployeeId && c.ProductId == dto.ProductId);

            if (existing != null && !existing.IsDeleted)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Commission already exists for this employee-product combination" });

            var commission = new Commission
            {
                EmployeeId = dto.EmployeeId,
                ProductId = dto.ProductId,
                CommissionPercentage = dto.CommissionPercentage,
                CreatedAt = DateTime.UtcNow
            };

            _context.Commissions.Add(commission);
            await _context.SaveChangesAsync();

            var result = new CommissionDto
            {
                Id = commission.Id,
                EmployeeId = commission.EmployeeId,
                ProductId = commission.ProductId,
                CommissionPercentage = commission.CommissionPercentage,
                CreatedAt = commission.CreatedAt
            };

            return CreatedAtAction(nameof(GetCommission), new { id = commission.Id }, 
                new ApiResponse<CommissionDto> { Success = true, Data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommission(Guid id)
        {
            var commission = await _context.Commissions
                .Include(c => c.Employee)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commission == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Commission not found" });

            var result = new CommissionDto
            {
                Id = commission.Id,
                EmployeeId = commission.EmployeeId,
                ProductId = commission.ProductId,
                CommissionPercentage = commission.CommissionPercentage,
                CreatedAt = commission.CreatedAt
            };

            return Ok(new ApiResponse<CommissionDto> { Success = true, Data = result });
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeCommissions(Guid employeeId)
        {
            var commissions = await _context.Commissions
                .Include(c => c.Product)
                .Where(c => c.EmployeeId == employeeId)
                .ToListAsync();

            var results = commissions.Select(c => new CommissionDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                ProductId = c.ProductId,
                CommissionPercentage = c.CommissionPercentage,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(new ApiResponse<List<CommissionDto>> { Success = true, Data = results });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommission(Guid id, [FromBody] UpdateCommissionDto dto)
        {
            var commission = await _context.Commissions.FindAsync(id);
            if (commission == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Commission not found" });

            commission.CommissionPercentage = dto.CommissionPercentage;
            commission.UpdatedAt = DateTime.UtcNow;

            _context.Commissions.Update(commission);
            await _context.SaveChangesAsync();

            var result = new CommissionDto
            {
                Id = commission.Id,
                EmployeeId = commission.EmployeeId,
                ProductId = commission.ProductId,
                CommissionPercentage = commission.CommissionPercentage,
                CreatedAt = commission.CreatedAt
            };

            return Ok(new ApiResponse<CommissionDto> { Success = true, Data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommission(Guid id)
        {
            var commission = await _context.Commissions.FindAsync(id);
            if (commission == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Commission not found" });

            commission.IsDeleted = true;
            commission.UpdatedAt = DateTime.UtcNow;

            _context.Commissions.Update(commission);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Commission deleted successfully" });
        }

        [HttpGet("salesman/{salesmanId}/monthly/{year}/{month}")]
        public async Task<IActionResult> GetSalesmanCommissionSummary(Guid salesmanId, int year, int month)
        {
            var salesman = await _context.Employees.FindAsync(salesmanId);
            if (salesman == null || salesman.Role != "Salesman")
                return NotFound(new ApiResponse<string> { Success = false, Message = "Salesman not found" });

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get all sales for this salesman in this month
            var saleRecords = await _context.SaleRecords
                .Where(sr => sr.EmployeeId == salesmanId && 
                            sr.SaleDate >= startDate && 
                            sr.SaleDate <= endDate)
                .Include(sr => sr.Product)
                .ToListAsync();

            decimal totalCommission = 0;

            foreach (var sale in saleRecords)
            {
                var commission = await _context.Commissions
                    .FirstOrDefaultAsync(c => c.EmployeeId == salesmanId && c.ProductId == sale.ProductId);

                if (commission != null)
                {
                    var saleAmount = sale.Revenue; // Use SaleRecord.Revenue (already stored)
                    totalCommission += (saleAmount * commission.CommissionPercentage) / 100;
                }
            }

            var result = new SalesmanCommissionSummaryDto
            {
                SalesmanId = salesman.Id,
                SalesmanName = salesman.FullName,
                SalesmanEmail = salesman.Email,
                MonthlySalary = salesman.MonthlySalary,
                TotalCommissionEarned = totalCommission,
                TotalAmountDue = salesman.MonthlySalary + totalCommission,
                Month = month,
                Year = year
            };

            return Ok(new ApiResponse<SalesmanCommissionSummaryDto> { Success = true, Data = result });
        }
        [HttpGet("salesman/{salesmanId}/earnings")]
        public async Task<IActionResult> GetSalesmanEarnings(Guid salesmanId, [FromQuery] int days = 7)
        {
            var salesman = await _context.Employees.FindAsync(salesmanId);
            if (salesman == null || salesman.Role != "Salesman")
                return NotFound(new ApiResponse<string> { Success = false, Message = "Salesman not found" });

            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1); // +1 to include today as the last day
            var endDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1); // End of today

            var saleRecords = await _context.SaleRecords
                .Where(sr => sr.EmployeeId == salesmanId && 
                            sr.SaleDate >= startDate && 
                            sr.SaleDate <= endDate)
                .Include(sr => sr.Product)
                .ToListAsync();

            var dailyBreakdownDict = new Dictionary<string, decimal>();
            for (int i = 0; i < days; i++)
            {
                dailyBreakdownDict[startDate.AddDays(i).ToString("yyyy-MM-dd")] = 0;
            }

            decimal totalEarnings = 0;

            foreach (var sale in saleRecords)
            {
                var commission = await _context.Commissions
                    .FirstOrDefaultAsync(c => c.EmployeeId == salesmanId && c.ProductId == sale.ProductId);

                if (commission != null)
                {
                    var earned = (sale.Revenue * commission.CommissionPercentage) / 100;
                    totalEarnings += earned;
                    
                    var dateKey = sale.SaleDate.ToString("yyyy-MM-dd");
                    if (dailyBreakdownDict.ContainsKey(dateKey))
                    {
                        dailyBreakdownDict[dateKey] += earned;
                    }
                }
            }

            var result = new AgentEarningsDto
            {
                SalesmanId = salesman.Id,
                SalesmanName = salesman.FullName ?? string.Empty,
                TotalEarnings = totalEarnings,
                Days = days,
                DailyBreakdown = dailyBreakdownDict.Select(kvp => new DailyEarningDto
                {
                    Date = kvp.Key,
                    Earnings = kvp.Value
                }).OrderBy(d => d.Date).ToList()
            };

            return Ok(new ApiResponse<AgentEarningsDto> { Success = true, Data = result });
        }
    }
}
