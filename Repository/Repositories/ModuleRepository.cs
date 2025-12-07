using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;

namespace Repository.Repositories
{
    public class ModuleRepository : BaseRepository<Module>, IModuleRepository
    {
        private readonly IGCSEContext _context;
        public ModuleRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Module>> GetAllAsync()
        {
            return await _context.Modules
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(IEnumerable<Module> items, int total)> SearchAsync(
            int page, 
            int pageSize, 
            string? searchByName, 
            CourseSubject? courseSubject, 
            bool? isActive)
        {
            var query = _context.Modules.AsQueryable();

            // Filter by name
            if (!string.IsNullOrWhiteSpace(searchByName))
            {
                query = query.Where(m => m.ModuleName.Contains(searchByName));
            }

            // Filter by course subject
            if (courseSubject.HasValue)
            {
                var subjectString = courseSubject.Value.ToString();
                query = query.Where(m => m.EmbeddingDataSubject == subjectString);
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderBy(m => m.ModuleName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, total);
        }
        public async Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Module)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course?.Module != null)
            {
                return new List<Module> { course.Module };
            }
            
            return new List<Module>();
        }

        public async Task<IEnumerable<Module>> GetByCourseSubjectAsync(CourseSubject courseSubject)
        {
            var subjectString = courseSubject.ToString();
            return await _context.Set<Module>()
                .Where(m => m.EmbeddingDataSubject == subjectString)
                .ToListAsync();
        }
        public async Task<Module?> GetByIdAsync(int moduleId)
        {
            return await _context.Set<Module>()
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
