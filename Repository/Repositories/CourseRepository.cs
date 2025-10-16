using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using System.Linq;
using System.Threading.Tasks;
using Common.Utils;

namespace Repository.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly IGCSEContext _context;

        public CourseRepository(IGCSEContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Course?> GetByCourseIdAsync(long courseId)
        {
            return await _context.Set<Course>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course?> GetByCourseIdWithCategoryAsync(long courseId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(long categoryId)
        {
            return await _context.Set<Course>()
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(long courseId, decimal score)
        {
            var targetCourse = await GetByCourseIdAsync(courseId);

            var targetEmbeddingData = CommonUtils.StringToObject<List<float>>(targetCourse.EmbeddingData);

            var courses = await _context.Set<Course>()
                .Where(c => c.CourseId != courseId)
                .Select(c => new
                {
                    c,
                    SimilarScore = score * (CommonUtils.CosineSimilarity(targetEmbeddingData, CommonUtils.StringToObject<List<float>>(c.EmbeddingData)))
                })
                .OrderByDescending(c => c.SimilarScore)
                .Take(5)
                .Select(c => c.c)
                .ToListAsync();
            return courses;
        }
    }
}
