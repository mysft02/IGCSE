using Microsoft.EntityFrameworkCore;
using Repository.IBaseRepository;
using System.Linq.Expressions;

namespace Repository.BaseRepository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly DbContext _dbContext;
        public BaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<T> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentNullException($"id {id} not found");
            }
            var entity = await _dbContext.Set<T>().FindAsync(id);

            if (entity == null)
            {
                throw new ArgumentNullException($"Entity of type {typeof(T).Name} with id {id} not found");
            }
            return entity;
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<T> DeleteAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();

        }

        public async Task<T> GetByStringId(string id)
        {
            var entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new ArgumentNullException($"Entity of type {typeof(T).Name} with id {id} not found");
            }
            return entity;
        }

        public async Task<List<T>> AddRange(List<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            await _dbContext.Set<T>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task<List<T>> UpdateRange(List<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            _dbContext.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task<List<T>> DeleteRange(List<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            _dbContext.RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.Set<T>().CountAsync();
        }

        public async Task<T> AddOrUpdateAsync(T entity, Func<T, object> keySelector)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var key = keySelector(entity);
            
            // Handle composite keys by extracting individual key values
            T? existing = null;
            if (key is object[] keyArray)
            {
                existing = await _dbContext.Set<T>().FindAsync(keyArray);
            }
            else
            {
                existing = await _dbContext.Set<T>().FindAsync(key);
            }
            
            if (existing != null)
            {
                // Update existing entity
                _dbContext.Entry(existing).CurrentValues.SetValues(entity);
                _dbContext.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                // Add new entity
                await _dbContext.Set<T>().AddAsync(entity);
            }
            
            await _dbContext.SaveChangesAsync();
            return existing ?? entity;
        }

        #region Query Methods

        /// <summary>
        /// Find entities with optional filter
        /// </summary>
        public async Task<List<T>> FindAsync(Expression<Func<T, bool>>? filter = null)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            return await query.ToListAsync();
        }

        /// <summary>
        /// Count entities with optional filter
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            return await query.CountAsync();
        }

        /// <summary>
        /// Find entities with pagination and optional filter
        /// </summary>
        public async Task<List<T>> FindWithPagingAsync(Expression<Func<T, bool>>? filter = null, int page = 0, int size = 10)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            return await query
                .Skip(page * size)
                .Take(size)
                .ToListAsync();
        }

        /// <summary>
        /// Find entities with pagination and count, with optional filter
        /// </summary>
        public async Task<(List<T> Items, int TotalCount)> FindWithPagingAndCountAsync(Expression<Func<T, bool>>? filter = null, int page = 0, int size = 10)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(page * size)
                .Take(size)
                .ToListAsync();
            
            return (items, totalCount);
        }

        public async Task<(List<T> Items, int TotalCount)> FindWithIncludePagingAndCountAsync(Expression<Func<T, bool>>? filter = null, int page = 0, int size = 10, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            // Apply includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Count
            var totalCount = await query.CountAsync();

            // Paging
            var items = await query
                .Skip(page * size)
                .Take(size)
                .ToListAsync();

            return (items, totalCount);
        }


        #endregion

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(filter);
        }

    }
}
