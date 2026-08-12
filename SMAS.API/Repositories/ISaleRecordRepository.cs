using SMAS.API.Models;

namespace SMAS.API.Repositories
{
    public interface ISaleRecordRepository : IRepository<SaleRecord>
    {
        Task<IEnumerable<SaleRecord>> GetByProductAsync(Guid productId);
        Task<IEnumerable<SaleRecord>> GetByEmployeeAsync(Guid employeeId);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    }
}