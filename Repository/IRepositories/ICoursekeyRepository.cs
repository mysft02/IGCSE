using BusinessObject.Model;
using Repository.IBaseRepository;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface ICoursekeyRepository : IBaseRepository<Coursekey>
    {
        Task<Coursekey?> GetByCourseAndStudentAsync(long courseId, string studentId);
        Task<Coursekey?> GetByCourseKeyAsync(long courseKeyId);
        Task<IEnumerable<Coursekey>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Coursekey>> GetByCourseIdAsync(long courseId);
        Task<IEnumerable<Coursekey>> GetByParentIdAsync(string parentId);
        Task<IEnumerable<Coursekey>> GetAvailableCourseKeysAsync();
        Task<IEnumerable<Coursekey>> GetAllCourseKeysAsync(string? status = null, string? parentId = null, long? courseId = null);
        Task<IEnumerable<Coursekey>> GetAllCourseKeysWithNullHandlingAsync();
        Task<string> GenerateUniqueCourseKeyAsync(long courseId, string studentId);
    }
}
