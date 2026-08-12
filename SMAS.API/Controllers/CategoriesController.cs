using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Models;
using SMAS.API.Data;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public CategoriesController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories.Include(c => c.SubCategories).ToListAsync();
            return Ok(new ApiResponse<IEnumerable<CategoryResponseDto>>
            {
                Success = true,
                Data = categories.Select(c => MapToDto(c))
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var category = await _context.Categories.Include(c => c.SubCategories).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Category not found" });

            return Ok(new ApiResponse<CategoryResponseDto> { Success = true, Data = MapToDto(category) });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ParentCategoryId = dto.ParentCategoryId
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<CategoryResponseDto> { Success = true, Data = MapToDto(category), Message = "Category created" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Category not found" });

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;
            category.ParentCategoryId = dto.ParentCategoryId;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<CategoryResponseDto> { Success = true, Data = MapToDto(category), Message = "Category updated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Category not found" });

            category.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Category deleted" });
        }

        private static CategoryResponseDto MapToDto(Category c)
        {
            return new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? "",
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}