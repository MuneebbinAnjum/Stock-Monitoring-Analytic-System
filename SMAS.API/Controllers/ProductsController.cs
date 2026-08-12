using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IInventoryService inventoryService, ILogger<ProductsController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _inventoryService.GetAllProductsAsync();
                return Ok(new ApiResponse<IEnumerable<ProductResponseDto>> 
                { 
                    Success = true, 
                    Data = products,
                    Message = $"Retrieved {products?.Count()} products"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while retrieving products" 
                });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return await GetAll();
                }

                var products = await _inventoryService.SearchProductsAsync(q);
                return Ok(new ApiResponse<IEnumerable<ProductResponseDto>> 
                { 
                    Success = true, 
                    Data = products,
                    Message = $"Found {products?.Count()} product(s) matching '{q}'"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with query: {Query}", q);
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while searching products" 
                });
            }
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            try
            {
                var products = await _inventoryService.GetLowStockProductsAsync();
                return Ok(new ApiResponse<IEnumerable<ProductResponseDto>> 
                { 
                    Success = true, 
                    Data = products,
                    Message = $"Found {products?.Count()} product(s) with low stock"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock products");
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while retrieving low stock products" 
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Product ID is invalid" 
                    });

                var product = await _inventoryService.GetProductWithDetailsAsync(id);
                
                if (product == null)
                    return NotFound(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = $"Product with ID {id} not found" 
                    });

                return Ok(new ApiResponse<ProductResponseDto> 
                { 
                    Success = true, 
                    Data = product 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", id);
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while retrieving the product" 
                });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Product data is required" 
                    });

                var product = await _inventoryService.CreateProductAsync(dto);
                return StatusCode(201, new ApiResponse<ProductResponseDto> 
                { 
                    Success = true, 
                    Data = product, 
                    Message = "Product created successfully" 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while creating the product" 
                });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateDto dto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Product ID is invalid" 
                    });

                if (dto == null)
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Product data is required" 
                    });

                var product = await _inventoryService.UpdateProductAsync(id, dto);
                
                if (product == null)
                    return NotFound(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = $"Product with ID {id} not found" 
                    });

                return Ok(new ApiResponse<ProductResponseDto> 
                { 
                    Success = true, 
                    Data = product, 
                    Message = "Product updated successfully" 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while updating the product" 
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Product ID is invalid" 
                    });

                await _inventoryService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while deleting the product" 
                });
            }
        }
    }
}