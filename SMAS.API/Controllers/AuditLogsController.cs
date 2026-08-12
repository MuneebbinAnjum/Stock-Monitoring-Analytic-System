using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMAS.API.DTOs;
using SMAS.API.Data;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public AuditLogsController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? entityName = null,
            [FromQuery] string? action = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(al => al.EntityName == entityName);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(al => al.Action == action);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(al => al.PerformedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = logs.Select(al => new AuditLogResponseDto
            {
                Id = al.Id,
                EntityName = al.EntityName,
                EntityId = al.EntityId,
                Action = al.Action,
                PerformedBy = al.PerformedBy,
                PerformedAt = al.PerformedAt,
                Details = al.Details,
                OldValues = al.OldValues,
                NewValues = al.NewValues,
                IpAddress = al.IpAddress
            });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { items = dtos, total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) }
            });
        }
    }
}
