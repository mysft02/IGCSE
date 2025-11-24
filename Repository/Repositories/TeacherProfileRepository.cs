using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class TeacherProfileRepository : BaseRepository<Teacherprofile>, ITeacherProfileRepository
    {
        private readonly IGCSEContext _context;

        public TeacherProfileRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
