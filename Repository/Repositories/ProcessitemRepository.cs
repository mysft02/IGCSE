using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class ProcessitemRepository : BaseRepository<Processitem>, IProcessitemRepository
    {
        private readonly IGCSEContext _context;

        public ProcessitemRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Processitem>> GetByProcessIdAsync(int processId)
        {
            return await _context.Set<Processitem>()
                .Include(pi => pi.LessonItem)
                .Include(pi => pi.Process)
                .Where(pi => pi.ProcessId == processId)
                .ToListAsync();
        }

        public async Task<Processitem?> GetByProcessAndLessonItemAsync(int processId, int lessonItemId)
        {
            return await _context.Set<Processitem>()
                .Include(pi => pi.LessonItem)
                .Include(pi => pi.Process)
                .FirstOrDefaultAsync(pi => pi.ProcessId == processId && pi.LessonItemId == lessonItemId);
        }

        public async Task<bool> IsLessonItemCompletedAsync(int processId, int lessonItemId)
        {
            var processItem = await GetByProcessAndLessonItemAsync(processId, lessonItemId);
            return processItem != null;
        }
    }
}
