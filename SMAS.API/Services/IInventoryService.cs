using SMAS.API.DTOs;
using SMAS.API.Models;

namespace SMAS.API.Services
{
    public interface IInventoryService
    {
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto);
        Task<ProductResponseDto> UpdateProductAsync(Guid id, ProductUpdateDto dto);
        Task DeleteProductAsync(Guid id);
        Task<ProductResponseDto> GetProductWithDetailsAsync(Guid id);
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string query);
        Task AdjustStockAsync(Guid productId, int quantity, string reason);
        Task<IEnumerable<ProductResponseDto>> GetLowStockProductsAsync();
        Task CheckAndCreateAlertsAsync();
    }
}