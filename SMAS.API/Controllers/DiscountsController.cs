using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DiscountsController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public DiscountsController(SmasDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDiscounts()
        {
            var now = DateTime.UtcNow;
            var activeDiscounts = await _context.Discounts
                .Include(d => d.Product)
                .Where(d => d.StartDate <= now && d.EndDate >= now && !d.IsDeleted)
                .ToListAsync();

            var results = activeDiscounts.Select(d => new DiscountDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                DiscountPercentage = d.DiscountPercentage,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                IsActive = true,
                CreatedByAdmin = d.CreatedByAdmin,
                CreatedAt = d.CreatedAt
            }).ToList();

            return Ok(new ApiResponse<List<DiscountDto>> { Success = true, Data = results });
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductDiscounts(Guid productId)
        {
            var now = DateTime.UtcNow;
            var discount = await _context.Discounts
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.ProductId == productId && 
                                         d.StartDate <= now && 
                                         d.EndDate >= now && 
                                         !d.IsDeleted);

            if (discount == null)
                return Ok(new ApiResponse<DiscountDto?> { Success = true, Data = null });

            var result = new DiscountDto
            {
                Id = discount.Id,
                ProductId = discount.ProductId,
                ProductName = discount.Product?.Name ?? string.Empty,
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = true,
                CreatedByAdmin = discount.CreatedByAdmin,
                CreatedAt = discount.CreatedAt
            };

            return Ok(new ApiResponse<DiscountDto?> { Success = true, Data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "End date must be after start date" });

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Product not found" });

            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";

            var discount = new Discount
            {
                ProductId = dto.ProductId,
                DiscountPercentage = dto.DiscountPercentage,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedByAdmin = adminEmail,
                CreatedAt = DateTime.UtcNow
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            var result = new DiscountDto
            {
                Id = discount.Id,
                ProductId = discount.ProductId,
                ProductName = product.Name,
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = DateTime.UtcNow >= discount.StartDate && DateTime.UtcNow <= discount.EndDate,
                CreatedByAdmin = discount.CreatedByAdmin,
                CreatedAt = discount.CreatedAt
            };

            return CreatedAtAction(nameof(GetDiscount), new { id = discount.Id }, 
                new ApiResponse<DiscountDto> { Success = true, Data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscount(Guid id)
        {
            var discount = await _context.Discounts
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discount == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Discount not found" });

            var result = new DiscountDto
            {
                Id = discount.Id,
                ProductId = discount.ProductId,
                ProductName = discount.Product?.Name ?? string.Empty,
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = DateTime.UtcNow >= discount.StartDate && DateTime.UtcNow <= discount.EndDate,
                CreatedByAdmin = discount.CreatedByAdmin,
                CreatedAt = discount.CreatedAt
            };

            return Ok(new ApiResponse<DiscountDto> { Success = true, Data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDiscounts()
        {
            var discounts = await _context.Discounts
                .Include(d => d.Product)
                .ToListAsync();

            var results = discounts.Select(d => new DiscountDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                DiscountPercentage = d.DiscountPercentage,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                IsActive = DateTime.UtcNow >= d.StartDate && DateTime.UtcNow <= d.EndDate,
                CreatedByAdmin = d.CreatedByAdmin,
                CreatedAt = d.CreatedAt
            }).ToList();

            return Ok(new ApiResponse<List<DiscountDto>> { Success = true, Data = results });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscount(Guid id, [FromBody] UpdateDiscountDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "End date must be after start date" });

            var discount = await _context.Discounts
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (discount == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Discount not found" });

            discount.DiscountPercentage = dto.DiscountPercentage;
            discount.StartDate = dto.StartDate;
            discount.EndDate = dto.EndDate;
            discount.UpdatedAt = DateTime.UtcNow;

            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();

            var result = new DiscountDto
            {
                Id = discount.Id,
                ProductId = discount.ProductId,
                ProductName = discount.Product?.Name ?? string.Empty,
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = DateTime.UtcNow >= discount.StartDate && DateTime.UtcNow <= discount.EndDate,
                CreatedByAdmin = discount.CreatedByAdmin,
                CreatedAt = discount.CreatedAt
            };

            return Ok(new ApiResponse<DiscountDto> { Success = true, Data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Discount not found" });

            discount.IsDeleted = true;
            discount.UpdatedAt = DateTime.UtcNow;

            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Discount deleted successfully" });
        }
    }
}
