using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.Models;

namespace SMAS.API.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(SmasDbContext context) : base(context) { }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.StockQuantity <= p.ReorderLevel + threshold)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string query)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && EF.Functions.ILike(p.Name, $"%{query}%"))
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
        }
    }
}