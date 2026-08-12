using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Data;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly SmasDbContext _context;

        public EmployeesController(IEmployeeService employeeService, SmasDbContext context)
        {
            _employeeService = employeeService;
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees.ToListAsync();
            var dtos = employees.Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                Name = e.FullName ?? string.Empty,
                Role = e.Role ?? string.Empty,
                Email = e.Email ?? string.Empty,
                HireDate = e.HireDate,
                MonthlySalesTarget = e.MonthlySalesTarget,
                ApprovalStatus = e.ApprovalStatus ?? "Pending",
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            });
            return Ok(new ApiResponse<IEnumerable<EmployeeResponseDto>> { Success = true, Data = dtos });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var pending = await _context.Employees
                .Where(e => e.ApprovalStatus == "Pending")
                .ToListAsync();
            var dtos = pending.Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                Name = e.FullName ?? string.Empty,
                Role = e.Role ?? string.Empty,
                Email = e.Email ?? string.Empty,
                HireDate = e.HireDate,
                MonthlySalesTarget = e.MonthlySalesTarget,
                ApprovalStatus = e.ApprovalStatus ?? "Pending",
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            });
            return Ok(new ApiResponse<IEnumerable<EmployeeResponseDto>> { Success = true, Data = dtos });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Employee not found" });

            employee.ApprovalStatus = "Approved";

            // Notify the salesman
            _context.Notifications.Add(new SMAS.API.Models.Notification
            {
                EmployeeId = employee.Id,
                Title = "Account Approved!",
                Message = $"Congratulations {employee.FullName}! Your salesman account has been approved. You can now log in.",
                NotificationType = "AccountApproved",
                RelatedId = employee.Id
            });

            // Audit log
            _context.AuditLogs.Add(new SMAS.API.Models.AuditLog
            {
                EntityName = "Employee",
                EntityId = employee.Id,
                Action = "Approve",
                PerformedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Admin",
                PerformedAt = DateTime.UtcNow,
                Details = $"Salesman '{employee.FullName}' ({employee.Email}) approved"
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Employee approved" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Employee not found" });

            employee.ApprovalStatus = "Rejected";

            // Audit log
            _context.AuditLogs.Add(new SMAS.API.Models.AuditLog
            {
                EntityName = "Employee",
                EntityId = employee.Id,
                Action = "Reject",
                PerformedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Admin",
                PerformedAt = DateTime.UtcNow,
                Details = $"Salesman '{employee.FullName}' ({employee.Email}) rejected"
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Employee rejected" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
        {
            var employee = await _employeeService.RegisterEmployeeAsync(dto);
            return Ok(new ApiResponse<EmployeeResponseDto> { Success = true, Data = employee, Message = "Employee created" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeUpdateDto dto)
        {
            var employee = await _employeeService.UpdateEmployeeAsync(id, dto);
            return Ok(new ApiResponse<EmployeeResponseDto> { Success = true, Data = employee, Message = "Employee updated" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}/kpi")]
        public async Task<IActionResult> GetKpi(Guid id, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _employeeService.GetKPIDashboardAsync(id, month, year);
            return Ok(new ApiResponse<EmployeeKPIDto> { Success = true, Data = result });
        }
    }
}