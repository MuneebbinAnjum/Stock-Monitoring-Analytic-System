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
    public class CartController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public CartController(SmasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var items = await _context.CartItems
                .Include(ci => ci.Product).ThenInclude(p => p!.ProductImages)
                .Where(ci => ci.CustomerId == customer.Id)
                .ToListAsync();

            var dtos = items.Select(ci => new CartItemResponseDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? "",
                ProductImage = ci.Product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? "",
                UnitPrice = ci.Product?.DiscountPrice ?? ci.Product?.UnitPrice ?? 0,
                OriginalPrice = ci.Product?.UnitPrice ?? 0,
                Quantity = ci.Quantity,
                StockAvailable = ci.Product?.StockQuantity ?? 0,
                Subtotal = (ci.Product?.DiscountPrice ?? ci.Product?.UnitPrice ?? 0) * ci.Quantity,
                TaxPercentage = ci.Product?.TaxPercentage ?? 0
            });

            return Ok(new ApiResponse<IEnumerable<CartItemResponseDto>> { Success = true, Data = dtos });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] CartItemCreateDto dto)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Product not found" });

            if (product.StockQuantity < dto.Quantity)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Insufficient stock" });

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CustomerId == customer.Id && ci.ProductId == dto.ProductId);

            if (existing != null)
            {
                existing.Quantity += dto.Quantity;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CustomerId = customer.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { Success = true, Message = "Item added to cart" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] CartItemUpdateDto dto)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var item = await _context.CartItems.Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.CustomerId == customer.Id);

            if (item == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Cart item not found" });

            if (item.Product != null && item.Product.StockQuantity < dto.Quantity)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Insufficient stock" });

            item.Quantity = dto.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Cart updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.CustomerId == customer.Id);

            if (item == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Cart item not found" });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Item removed from cart" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var items = await _context.CartItems.Where(ci => ci.CustomerId == customer.Id).ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Cart cleared" });
        }

        private async Task<Customer?> GetCurrentCustomer()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }
    }
}
