using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using Common.Utils;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Request.Courses;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Repository.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly IGCSEContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseRepository(IGCSEContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Course?> GetByCourseIdAsync(long courseId)
        {
            return await _context.Set<Course>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course?> GetByCourseIdWithCategoryAsync(long courseId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Module)
                .Include(c => c.FinalQuiz)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(long categoryId)
        {
            return await _context.Set<Course>()
                .Where(c => c.ModuleId == categoryId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, string? searchByName, long? couseId, string? status)
        {
            var query = _context.Set<Course>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchByName))
            {
                var keyword = searchByName.Trim();
                query = query.Where(c => c.Name.Contains(keyword));
            }

            if (couseId.HasValue)
            {
                query = query.Where(c => c.CourseId == couseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            var total = await query.CountAsync();

            var skip = (page <= 1 ? 0 : (page - 1) * pageSize);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
        public async Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(long courseId, decimal score)
        {
            var targetCourse = await GetByCourseIdAsync(courseId);

            var targetEmbeddingData = CommonUtils.StringToObject<List<float>>(targetCourse.EmbeddingData);

            var courses = _context.Courses
                .Where(c => c.CourseId != courseId)
                .AsEnumerable()
                .Select(c => new
                {
                    Course = c,
                    SimilarScore = score * (CommonUtils.CosineSimilarity(targetEmbeddingData, CommonUtils.StringToObject<List<float>>(c.EmbeddingData)))
                })
                .OrderByDescending(c => c.SimilarScore)
                .Take(5)
                .Select(c => c.Course)
                .ToList();
            return courses;
        }

        public async Task<Dictionary<string, int>> GetCoursesSortedByStatus()
        {
            var result = await _context.Courses
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
            
            return result;
        }

        public async Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string creatorAccountId)
        {
            return await _context.Set<Course>()
                .Where(c => c.CreatedBy == creatorAccountId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<CourseAnalyticsResponse> GetCourseAnalyticsAsync(int courseId)
        {
            var result = await _context.Courses
                .Include(x => x.FinalQuiz).ThenInclude(xc => xc.FinalQuizResult)
                .Where(x => x.CourseId == courseId)
                .Select(x => new CourseAnalyticsResponse
                {
                    CourseId = x.CourseId,
                    CourseName = x.Name,
                    CourseDescription = x.Description,
                    Status = x.Status,
                    Price = x.Price,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = (DateTime)x.CreatedAt,
                    UpdatedAt = (DateTime)x.UpdatedAt,
                    CustomerCount = _context.Processes.Where(xc => xc.CourseId == x.CourseId).Count(),
                    AverageFinalScore = x.FinalQuiz.FinalQuizResult.Average(xc => xc.Score),
                    TotalIncome = _context.Transactionhistories.Where(xc => xc.ItemId == x.CourseId).Sum(n => n.Amount),
                })
                .FirstOrDefaultAsync();

            if(result == null)
            {
                return null;
            }

            return result;
        }

        public async Task<CourseDetailWithoutProgressResponse> GetCourseDetailAsync(int courseId)
        {
            var detail = await _context.Courses
                .Include(x => x.CourseSections)
                .ThenInclude(cs => cs.Lessons)
                .ThenInclude(l => l.Lessonitems)
                .Include(x => x.CourseSections)
                .ThenInclude(cs => cs.Lessons)
                .ThenInclude(l => l.Quiz)
                .Where(x => x.CourseId == courseId)
                .Select(c => new CourseDetailWithoutProgressResponse
                {
                    CourseId = c.CourseId,
                    Name = c.Name,
                    Description = c.Description,
                    Status = c.Status,
                    Price = c.Price,
                    ImageUrl = CommonUtils.GetMediaUrl(c.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Sections = c.CourseSections
                    .Select(n => new CourseSectionDetailWithoutProgressResponse
                    {
                        CourseSectionId = n.CourseSectionId,
                        Name = n.Name,
                        Description = n.Description,
                        Order = n.Order,
                        IsActive = n.IsActive == 1,
                        Lessons = n.Lessons
                        .Select(s => new LessonDetailWithoutProgressResponse
                        {
                            LessonId = s.LessonId,
                            Name = s.Name,
                            Description = s.Description,
                            Order = s.Order,
                            IsActive = s.IsActive == 1,
                            IsUnlocked = true,
                            LessonItems = s.Lessonitems
                            .Select(p => new LessonItemDetailWithoutProgressResponse
                            {
                                LessonItemId = p.LessonItemId,
                                Name = p.Name,
                                Order = p.Order,
                            })
                            .ToList(),
                            Quiz = new LessonQuizResponse
                            {
                                QuizId = s.Quiz.QuizId,
                                QuizTitle = s.Quiz.QuizTitle,
                                QuizDescription = s.Quiz.QuizDescription,
                            }
                        })
                        .ToList()
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            if(detail == null)
            {
                return null;
            }

            return detail;
        }
    }
}
