using BusinessObject.Model;
using Repository.IBaseRepository;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface ILessonitemRepository : IBaseRepository<Lessonitem>
    {
        Task<IEnumerable<Lessonitem>> GetByLessonIdAsync(long lessonId);
        Task<Lessonitem?> GetByLessonItemIdAsync(long lessonItemId);
    }
}
