using SMAS.API.Models;

namespace SMAS.API.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId);
        Task<IEnumerable<Order>> GetByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}