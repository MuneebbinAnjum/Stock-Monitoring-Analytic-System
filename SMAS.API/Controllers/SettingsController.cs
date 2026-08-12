using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMAS.API.DTOs;
using SMAS.API.Data;
using SMAS.API.Models;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public SettingsController(SmasDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var settings = await _context.SystemSettings.OrderBy(s => s.Category).ThenBy(s => s.Key).ToListAsync();
            var dtos = settings.Select(s => new SystemSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                Category = s.Category
            });
            return Ok(new ApiResponse<IEnumerable<SystemSettingDto>> { Success = true, Data = dtos });
        }

        [HttpGet("public/{key}")]
        public async Task<IActionResult> GetPublic(string key)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Setting not found" });

            return Ok(new ApiResponse<SystemSettingDto>
            {
                Success = true,
                Data = new SystemSettingDto { Id = setting.Id, Key = setting.Key, Value = setting.Value, Category = setting.Category }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{key}")]
        public async Task<IActionResult> Update(string key, [FromBody] SystemSettingUpdateDto dto)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Setting not found" });

            setting.Value = dto.Value;
            setting.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<SystemSettingDto>
            {
                Success = true,
                Data = new SystemSettingDto { Id = setting.Id, Key = setting.Key, Value = setting.Value, Description = setting.Description, Category = setting.Category },
                Message = "Setting updated"
            });
        }
    }
}
