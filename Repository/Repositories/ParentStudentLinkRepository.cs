using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Account>> GetByParentId(string id)
        {
            var linkList = _context.Parentstudentlinks
                .Where(x => x.ParentId == id)
                .Include(x => x.Student)
                .Select(x => x.Student)
                .ToList();

            return linkList;
        }

        public async Task<bool> CheckDuplicateParentStudentLink(string parentId, string studentId)
        {
            return await _context.Parentstudentlinks.AnyAsync(x => x.ParentId == parentId && x.StudentId == studentId);
        }
    }
}
