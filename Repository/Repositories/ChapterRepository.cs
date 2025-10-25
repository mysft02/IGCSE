using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly IGCSEContext _context;
        public ChapterRepository(IGCSEContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Chapter>> GetByModuleIdAsync(int moduleId)
        {
            return await _context.Set<Chapter>()
                .Where(c => c.ModuleID == moduleId)
                .ToListAsync();
        }
        public async Task<Chapter?> GetByIdAsync(int chapterId)
        {
            return await _context.Set<Chapter>()
                .FirstOrDefaultAsync(c => c.ChapterID == chapterId);
        }
        public async Task<Chapter> AddAsync(Chapter chapter)
        {
            _context.Set<Chapter>().Add(chapter);
            await _context.SaveChangesAsync();
            return chapter;
        }
        public async Task<Chapter> UpdateAsync(Chapter chapter)
        {
            _context.Set<Chapter>().Update(chapter);
            await _context.SaveChangesAsync();
            return chapter;
        }
        public async Task DeleteAsync(int chapterId)
        {
            var chapter = await GetByIdAsync(chapterId);
            if (chapter != null)
            {
                _context.Set<Chapter>().Remove(chapter);
                await _context.SaveChangesAsync();
            }
        }
    }
}
