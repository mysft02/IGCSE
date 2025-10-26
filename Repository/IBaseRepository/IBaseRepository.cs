﻿using System.Linq.Expressions;

namespace Repository.IBaseRepository
{
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
        Task<T> GetByStringId(string id);
        Task<int> CountAsync();
        Task<List<T>> AddRange(List<T> entities);
        Task<List<T>> UpdateRange(List<T> entities);
        Task<List<T>> DeleteRange(List<T> entities);
        Task<T> AddOrUpdateAsync(T entity, Func<T, object> keySelector);
        
        // Query methods
        Task<List<T>> FindAsync(Expression<Func<T, bool>>? filter = null);
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
        Task<List<T>> FindWithPagingAsync(Expression<Func<T, bool>>? filter = null, int page = 0, int size = 10);
        Task<(List<T> Items, int TotalCount)> FindWithPagingAndCountAsync(Expression<Func<T, bool>>? filter = null, int page = 0, int size = 10);
    }
}
