using System.Linq.Expressions;
using SMAS.API.Models;

namespace SMAS.API.Repositories
{
    public interface IRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity, bool saveChanges = true);
        Task<T> UpdateAsync(T entity, bool saveChanges = true);
        Task DeleteAsync(Guid id, bool saveChanges = true);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}