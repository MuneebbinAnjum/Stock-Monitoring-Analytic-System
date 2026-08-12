using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Models;
using SMAS.API.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace SMAS.API.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IProductRepository _productRepository;
        private readonly SmasDbContext _context;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<SMAS.API.Hubs.NotificationHub> _hubContext;
        private readonly ICacheService _cache;

        public InventoryService(IProductRepository productRepository, SmasDbContext context, Microsoft.AspNetCore.SignalR.IHubContext<SMAS.API.Hubs.NotificationHub> hubContext, ICacheService cache)
        {
            _productRepository = productRepository;
            _context = context;
            _hubContext = hubContext;
            _cache = cache;
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto)
        {
            if (dto.CategoryId == Guid.Empty)
                throw new InvalidOperationException("Category must be selected.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                // Ensure SKU uniqueness
                var existsSku = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
                if (existsSku)
                    throw new InvalidOperationException("Another product with the same SKU already exists.");

                // Use a transaction to ensure operations are atomic
                using var tx = await BeginTransactionIfSupportedAsync();
                var product = new Product
                {
                    Name = dto.Name,
                    SKU = dto.SKU,
                    CategoryId = dto.CategoryId,
                    UnitPrice = dto.UnitPrice,
                    PurchasePrice = dto.PurchasePrice,
                    DiscountPrice = dto.DiscountPrice,
                    StockQuantity = dto.StockQuantity,
                    ReorderLevel = dto.ReorderLevel,
                    SupplierId = dto.SupplierId,
                    Description = dto.Description,
                    BrandName = dto.BrandName,
                    CompanyName = dto.CompanyName,
                    Model = dto.Model,
                    DeliveryPeriod = dto.DeliveryPeriod,
                    WarrantyInfo = dto.WarrantyInfo,
                    Weight = dto.Weight,
                    Dimensions = dto.Dimensions,
                    Tags = dto.Tags,
                    TaxPercentage = dto.TaxPercentage
                };

                if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
                {
                    product.ProductImages = dto.ImageUrls.Select((url, index) => new ProductImage
                    {
                        ImageUrl = url,
                        DisplayOrder = index,
                        AltText = dto.Name
                    }).ToList();
                }

                var created = await _productRepository.CreateAsync(product, saveChanges: false);

                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = "Product",
                    EntityId = created.Id,
                    Action = "Create",
                    PerformedBy = "Admin",
                    PerformedAt = DateTime.UtcNow,
                    Details = $"Product '{created.Name}' (SKU: {created.SKU}) created with stock {created.StockQuantity}"
                });
                await _context.SaveChangesAsync();
                if (tx != null) await tx.CommitAsync();

                // After commit, recalculate alerts and clear cache
                await CheckAndCreateAlertsAsync();

                if (_cache != null) await _cache.RemoveAsync("products:all");

                return await MapToResponseDtoAsync(created);
            });
        }

        public async Task<ProductResponseDto> UpdateProductAsync(Guid id, ProductUpdateDto dto)
        {
            // Basic server-side validation to avoid common DB errors
            if (dto.CategoryId == Guid.Empty)
                throw new InvalidOperationException("Category must be selected.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (product == null) throw new KeyNotFoundException("Product not found");

                // Ensure SKU uniqueness (do not allow another product to use same SKU)
                var skuConflict = await _context.Products.AnyAsync(p => p.SKU == dto.SKU && p.Id != id);
                if (skuConflict)
                    throw new InvalidOperationException("Another product with the same SKU already exists.");

                // Use a transaction to ensure atomic update
                using var tx = await BeginTransactionIfSupportedAsync();

                product.Name = dto.Name;
                product.SKU = dto.SKU;
                product.CategoryId = dto.CategoryId;
                product.UnitPrice = dto.UnitPrice;
                product.PurchasePrice = dto.PurchasePrice;
                product.DiscountPrice = dto.DiscountPrice;
                product.StockQuantity = dto.StockQuantity;
                product.ReorderLevel = dto.ReorderLevel;
                product.SupplierId = dto.SupplierId;
                product.Description = dto.Description;
                product.BrandName = dto.BrandName;
                product.CompanyName = dto.CompanyName;
                product.Model = dto.Model;
                product.DeliveryPeriod = dto.DeliveryPeriod;
                product.WarrantyInfo = dto.WarrantyInfo;
                product.Weight = dto.Weight;
                product.Dimensions = dto.Dimensions;
                product.Tags = dto.Tags;
                product.TaxPercentage = dto.TaxPercentage;

                // Update images
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    _context.ProductImages.RemoveRange(product.ProductImages);
                }

                if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
                {
                    product.ProductImages = dto.ImageUrls.Select((url, index) => new ProductImage
                    {
                        ImageUrl = url,
                        DisplayOrder = index,
                        AltText = dto.Name
                    }).ToList();
                }

                var updated = await _productRepository.UpdateAsync(product, saveChanges: false);

                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = "Product",
                    EntityId = updated.Id,
                    Action = "Update",
                    PerformedBy = "Admin",
                    PerformedAt = DateTime.UtcNow,
                    Details = $"Product '{updated.Name}' (SKU: {updated.SKU}) updated, stock: {updated.StockQuantity}"
                });

                try
                {
                    await _context.SaveChangesAsync();
                    if (tx != null) await tx.CommitAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    if (tx != null) await tx.RollbackAsync();
                    throw new InvalidOperationException("The product was modified by another user. Please reload and retry.");
                }

                // Recompute alerts and clear cache after commit
                await CheckAndCreateAlertsAsync();

                if (_cache != null) await _cache.RemoveAsync("products:all");

                return await MapToResponseDtoAsync(updated);
            });
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            await _productRepository.DeleteAsync(id, saveChanges: false);

            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Product",
                EntityId = id,
                Action = "Delete",
                PerformedBy = "Admin",
                PerformedAt = DateTime.UtcNow,
                Details = $"Product '{product?.Name ?? id.ToString()}' soft-deleted"
            });
            await _context.SaveChangesAsync();

            if (_cache != null) await _cache.RemoveAsync("products:all");
        }

        public async Task<ProductResponseDto> GetProductWithDetailsAsync(Guid id)
        {
            var product = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) throw new KeyNotFoundException("Product not found");
            return await MapToResponseDtoAsync(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
        {
            // Try cache first
            var cacheKey = "products:all";
            List<ProductResponseDto>? cached = null;
            try
            {
                if (_cache != null)
                {
                    cached = await _cache.GetAsync<List<ProductResponseDto>>(cacheKey);
                }
            }
            catch { cached = null; }

            if (cached != null)
            {
                return cached;
            }

            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .ToListAsync();

            var dtos = new List<ProductResponseDto>();
            foreach (var product in products)
            {
                dtos.Add(await MapToResponseDtoAsync(product));
            }

            try
            {
                if (_cache != null)
                {
                    await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromSeconds(60));
                }
            }
            catch { }

            return dtos;
        }

        public async Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string query)
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted && ((p.Name != null && p.Name.Contains(query)) || (p.Description != null && p.Description.Contains(query)) || (p.BrandName != null && p.BrandName.Contains(query)) || (p.CompanyName != null && p.CompanyName.Contains(query)) || (p.SKU != null && p.SKU.Contains(query))))
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .ToListAsync();

            var dtos = new List<ProductResponseDto>();
            foreach (var product in products)
            {
                dtos.Add(await MapToResponseDtoAsync(product));
            }

            return dtos;
        }

        public async Task AdjustStockAsync(Guid productId, int quantity, string reason)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new KeyNotFoundException("Product not found");

            product.StockQuantity += quantity;
            if (product.StockQuantity < 0) product.StockQuantity = 0;

            await _context.SaveChangesAsync();
            await CheckAndCreateAlertsAsync();
            // create inventory transaction
            var tx = new InventoryTransaction
            {
                ProductId = productId,
                QuantityChange = quantity,
                Reason = reason,
                CreatedBy = "system"
            };
            _context.InventoryTransactions.Add(tx);
            await _context.SaveChangesAsync();

            if (_cache != null) await _cache.RemoveAsync("products:all");

            // broadcast update
            if (_hubContext != null)
            {
                try { await _hubContext.Clients.All.SendAsync("InventoryUpdated", new { ProductId = productId, NewQuantity = product.StockQuantity }); } catch { }
            }
        }

        public async Task<IEnumerable<ProductResponseDto>> GetLowStockProductsAsync()
        {
            var products = await _productRepository.GetLowStockAsync(0);
            var dtos = new List<ProductResponseDto>();
            foreach (var product in products)
            {
                dtos.Add(await MapToResponseDtoAsync(product));
            }
            return dtos;
        }

        public async Task CheckAndCreateAlertsAsync()
        {
            var lowStockProducts = await _productRepository.GetLowStockAsync(0);
            foreach (var product in lowStockProducts)
            {
                var existingAlert = await _context.StockAlerts
                    .FirstOrDefaultAsync(sa => sa.ProductId == product.Id && !sa.IsResolved);

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        ProductId = product.Id,
                        CurrentStock = product.StockQuantity,
                        ReorderLevel = product.ReorderLevel
                    };
                    _context.StockAlerts.Add(alert);
                    // broadcast new alert
                    if (_hubContext != null)
                    {
                        try { await _hubContext.Clients.All.SendAsync("StockAlertCreated", new { ProductId = product.Id, CurrentStock = product.StockQuantity }); } catch { }
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<IDbContextTransaction?> BeginTransactionIfSupportedAsync()
        {
            if (_context.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            {
                return null;
            }

            return await _context.Database.BeginTransactionAsync();
        }

        private async Task<ProductResponseDto> MapToResponseDtoAsync(Product product)
        {
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            await _context.Entry(product).Reference(p => p.Supplier).LoadAsync();

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                UnitPrice = product.UnitPrice,
                PurchasePrice = product.PurchasePrice,
                StockQuantity = product.StockQuantity,
                ReorderLevel = product.ReorderLevel,
                SupplierId = product.SupplierId ?? Guid.Empty,
                SupplierName = product.Supplier?.CompanyName ?? "",
                Description = product.Description,
                BrandName = product.BrandName,
                CompanyName = product.CompanyName,
                Model = product.Model,
                DeliveryPeriod = product.DeliveryPeriod,
                DiscountPrice = product.DiscountPrice,
                WarrantyInfo = product.WarrantyInfo,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                Tags = product.Tags,
                TaxPercentage = product.TaxPercentage,
                ViewCount = product.ViewCount,
                ProductImages = product.ProductImages?.OrderBy(pi => pi.DisplayOrder).Select(pi => new ProductImageResponseDto
                {
                    Id = pi.Id,
                    ProductId = pi.ProductId,
                    ImageUrl = pi.ImageUrl,
                    AltText = pi.AltText,
                    DisplayOrder = pi.DisplayOrder
                }).ToList() ?? new List<ProductImageResponseDto>(),
                CreatedAt = product.CreatedAt,
                RowVersion = product.RowVersion
            };
        }
    }
}