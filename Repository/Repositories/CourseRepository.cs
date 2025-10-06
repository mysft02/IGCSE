using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        public CourseRepository(IGCSEContext context) : base(context)
        {
        }
    }
}
