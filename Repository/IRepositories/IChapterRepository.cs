using BusinessObject.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IChapterRepository
    {
        Task<IEnumerable<Chapter>> GetByModuleIdAsync(int moduleId);
        Task<Chapter?> GetByIdAsync(int chapterId);
        Task<Chapter> AddAsync(Chapter chapter);
        Task<Chapter> UpdateAsync(Chapter chapter);
        Task DeleteAsync(int chapterId);
    }
}
