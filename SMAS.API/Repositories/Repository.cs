using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.Models;
using System.Linq.Expressions;

namespace SMAS.API.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        protected readonly SmasDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(SmasDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> CreateAsync(T entity, bool saveChanges = true)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Add(entity);
            if (saveChanges)
                await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity, bool saveChanges = true)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
            if (saveChanges)
                await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await UpdateAsync(entity, saveChanges);
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}