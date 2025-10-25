using BusinessObject;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IGCSEContext _context;
        public ModuleRepository(IGCSEContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Set<Module>()
                .Include(m => m.Chapters)
                .Where(m => m.CourseId == courseId)
                .ToListAsync();
        }
        public async Task<Module?> GetByIdAsync(int moduleId)
        {
            return await _context.Set<Module>()
                .Include(m => m.Chapters)
                .FirstOrDefaultAsync(m => m.ModuleID == moduleId);
        }
        public async Task<Module> AddAsync(Module module)
        {
            _context.Set<Module>().Add(module);
            await _context.SaveChangesAsync();
            return module;
        }
        public async Task<Module> UpdateAsync(Module module)
        {
            _context.Set<Module>().Update(module);
            await _context.SaveChangesAsync();
            return module;
        }
        public async Task DeleteAsync(int moduleId)
        {
            var module = await GetByIdAsync(moduleId);
            if (module != null)
            {
                _context.Set<Module>().Remove(module);
                await _context.SaveChangesAsync();
            }
        }
    }
}
