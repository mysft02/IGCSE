using BusinessObject.Model;
using Repository.IBaseRepository;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface ICoursekeyRepository : IBaseRepository<Coursekey>
    {
        Task<Coursekey?> GetByCourseAndStudentAsync(long courseId, long studentId);
        Task<Coursekey?> GetByCourseKeyAsync(long courseKeyId);
        Task<IEnumerable<Coursekey>> GetByStudentIdAsync(long studentId);
        Task<IEnumerable<Coursekey>> GetByCourseIdAsync(long courseId);
        Task<string> GenerateUniqueCourseKeyAsync(long courseId, long studentId);
    }
}
