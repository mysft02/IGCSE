using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class LessonitemRepository : BaseRepository<Lessonitem>, ILessonitemRepository
    {
        private readonly IGCSEContext _context;

        public LessonitemRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lessonitem>> GetByLessonIdAsync(long lessonId)
        {
            return await _context.Set<Lessonitem>()
                .Where(li => li.LessonId == lessonId)
                .OrderBy(li => li.Order)
                .ToListAsync();
        }

        public async Task<Lessonitem?> GetByLessonItemIdAsync(long lessonItemId)
        {
            return await _context.Set<Lessonitem>()
                .FirstOrDefaultAsync(li => li.LessonItemId == lessonItemId);
        }
    }
}
