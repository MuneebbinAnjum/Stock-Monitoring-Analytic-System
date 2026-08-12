using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.Models;

namespace SMAS.API.Repositories
{
    public class SaleRecordRepository : Repository<SaleRecord>, ISaleRecordRepository
    {
        public SaleRecordRepository(SmasDbContext context) : base(context) { }

        public async Task<IEnumerable<SaleRecord>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(sr => sr.ProductId == productId)
                .Include(sr => sr.Product)
                .Include(sr => sr.Employee)
                .ToListAsync();
        }

        public async Task<IEnumerable<SaleRecord>> GetByEmployeeAsync(Guid employeeId)
        {
            return await _dbSet
                .Where(sr => sr.EmployeeId == employeeId)
                .Include(sr => sr.Product)
                .Include(sr => sr.Employee)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(sr => sr.SaleDate >= startDate && sr.SaleDate <= endDate)
                .SumAsync(sr => sr.Revenue);
        }
    }
}