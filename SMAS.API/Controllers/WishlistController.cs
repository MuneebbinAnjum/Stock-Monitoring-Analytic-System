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
    [Authorize(Roles = "Buyer")]
    public class WishlistController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public WishlistController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var items = await _context.WishlistItems
                .Include(wi => wi.Product).ThenInclude(p => p!.ProductImages)
                .Where(wi => wi.CustomerId == customer.Id)
                .OrderByDescending(wi => wi.CreatedAt)
                .ToListAsync();

            var dtos = items.Select(wi => new WishlistItemResponseDto
            {
                Id = wi.Id,
                ProductId = wi.ProductId,
                ProductName = wi.Product?.Name ?? "",
                ProductImage = wi.Product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? "",
                UnitPrice = wi.Product?.DiscountPrice ?? wi.Product?.UnitPrice ?? 0,
                OriginalPrice = wi.Product?.UnitPrice ?? 0,
                InStock = (wi.Product?.StockQuantity ?? 0) > 0,
                CreatedAt = wi.CreatedAt
            });

            return Ok(new ApiResponse<IEnumerable<WishlistItemResponseDto>> { Success = true, Data = dtos });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] WishlistItemCreateDto dto)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var exists = await _context.WishlistItems
                .AnyAsync(wi => wi.CustomerId == customer.Id && wi.ProductId == dto.ProductId);

            if (exists)
                return Ok(new ApiResponse<string> { Success = true, Message = "Already in wishlist" });

            _context.WishlistItems.Add(new WishlistItem
            {
                CustomerId = customer.Id,
                ProductId = dto.ProductId
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Added to wishlist" });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveItem(Guid productId)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(wi => wi.CustomerId == customer.Id && wi.ProductId == productId);

            if (item == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Not in wishlist" });

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Removed from wishlist" });
        }

        private async Task<Customer?> GetCurrentCustomer()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }
    }
}
