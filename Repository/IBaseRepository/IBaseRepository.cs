using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        Task<List<T>> AddRange(List<T> entities);
        Task<List<T>> UpdateRange(List<T> entities);
        Task<List<T>> DeleteRange(List<T> entities);
    }
}
