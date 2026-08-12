using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.Models;

namespace SMAS.API.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(SmasDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByEmployeeAsync(Guid employeeId)
        {
            return await _dbSet
                .Where(o => o.EmployeeId == employeeId)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(o => o.Status == status)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .ToListAsync();
        }
    }
}