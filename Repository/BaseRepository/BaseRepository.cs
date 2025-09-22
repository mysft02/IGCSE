using DataAccessObject.BaseDAO;
using Repository.IBaseRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.BaseRepository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly BaseDAO<T> _baseDao;
        public BaseRepository(BaseDAO<T> baseDao)
        {
            _baseDao = baseDao;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _baseDao.GetAllAsync();
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await _baseDao.GetByIdAsync(id);
        }
        public async Task<T> AddAsync(T entity)
        {
            return await _baseDao.AddAsync(entity);
        }
        public async Task<T> UpdateAsync(T entity)
        {
            return await _baseDao.UpdateAsync(entity);
        }
        public async Task<T> DeleteAsync(T entity)
        {
            return await _baseDao.DeleteAsync(entity);
        }

        public async Task<T> GetByStringId(string id)
        {
            return await _baseDao.GetByStringIdAsync(id);
        }

        public async Task<List<T>> AddRange(List<T> entities)
        {
            return await _baseDao.AddRange(entities);
        }

        public async Task<List<T>> UpdateRange(List<T> entities)
        {
            return await _baseDao.UpdateRange(entities);
        }

        public async Task<List<T>> DeleteRange(List<T> entities)
        {
            return await _baseDao.DeleteRange(entities);
        }
    }
}
