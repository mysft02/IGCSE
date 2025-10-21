using BusinessObject;
using BusinessObject.Model;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class ParentStudentLinkRepository : BaseRepository<Parentstudentlink>, IParentStudentLinkRepository
    {
        private readonly IGCSEContext _context;

        public ParentStudentLinkRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }
    }
}
