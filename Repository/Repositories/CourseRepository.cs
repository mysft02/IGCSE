using BusinessObject.Model;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.BaseRepository;
using Repository.IRepositories;
using Common.Utils;
using BusinessObject.DTOs.Response.Courses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using BusinessObject.DTOs.Request.Courses;

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

        public async Task<Course?> GetByCourseIdAsync(int courseId)
        {
            return await _context.Set<Course>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course?> GetByCourseIdWithCategoryAsync(int courseId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Module)
                .Include(c => c.FinalQuiz)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Set<Course>()
                .Where(c => c.ModuleId == categoryId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, CourseListQuery query)
        {
            var courseQuery = _context.Set<Course>()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchByName))
            {
                var keyword = query.SearchByName.Trim();
                courseQuery = courseQuery.Where(c => c.Name.Contains(keyword));
            }

            if (query.CouseId.HasValue)
            {
                courseQuery = courseQuery.Where(c => c.CourseId == query.CouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status.ToString()))
            {
                courseQuery = courseQuery.Where(c => c.Status == query.Status.ToString());
            }

            var total = await courseQuery.CountAsync();

            var skip = (page <= 1 ? 0 : (page - 1) * pageSize);
            var items = await courseQuery
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<Course>> GetAllSimilarCoursesAsync(int courseId, decimal score)
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
                    IsEnrolled = false, // Sẽ được set trong service dựa trên enrollment status
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy,
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
                                Description = p.Description,
                                Content = p.Content,
                                ItemType = p.ItemType,
                                Order = p.Order,
                            })
                            .ToList(),
                            Quiz = s.Quiz != null ? new LessonQuizResponse
                            {
                                QuizId = s.Quiz.QuizId,
                                QuizTitle = s.Quiz.QuizTitle,
                                QuizDescription = s.Quiz.QuizDescription,
                            } : new LessonQuizResponse
                            {
                                QuizId = 0,
                                QuizTitle = string.Empty,
                                QuizDescription = string.Empty,
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

        public async Task<bool> CheckOwnedByLessonItemId(int lessonItemId, string userId)
        {
            var check = await _context.Courses
                .Include(x => x.CourseSections).ThenInclude(xc => xc.Lessons).ThenInclude(c => c.Lessonitems)
                .AnyAsync(c => c.CreatedBy == userId && c.CourseSections.Any(cs => cs.Lessons.Any(ls => ls.Lessonitems.Any(li => li.LessonItemId == lessonItemId))));

            return check;
        }

        public async Task<SimilarCourseForStudentRequest> GetCourseAndFinalQuizResult(string userId)
        {
            var enrollments = await _context.Studentenrollments
                .Include(x => x.Student)
                .Include(x => x.Course)
                .ThenInclude(xc => xc.FinalQuiz)
                .ThenInclude(c => c.FinalQuizResult)
                .Where(x => x.StudentId == userId)
                .ToListAsync();

            var courseResponse = enrollments
                .AsEnumerable()
                .Select(p => new CourseWithFinalQuizResultResponse
                {
                    Course = new CourseWithStudyingTimeResponse
                    {
                        CourseId = p.CourseId,
                        Name = p.Course.Name,
                        Description = p.Course.Description,
                        Status = p.Course.Status,
                        Price = p.Course.Price,
                        ImageUrl = p.Course.ImageUrl,
                        CreatedAt = p.Course.CreatedAt,
                        UpdatedAt = p.Course.UpdatedAt,
                        AverageLearningTime = GetAverageStudyingTime(p.CourseId, userId),
                    },
                    Finalquizresults = p.Course?.FinalQuiz?.FinalQuizResult?
                    .Where(x => x.UserId == userId)
                    .Select(n => new FinalQuizResultResponse
                    {
                        FinalQuizResultId = n.FinalQuizResultId,
                        Score = n.Score,
                        IsPassed = n.IsPassed,
                        CreatedAt = n.CreatedAt
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(3)
                    .ToList()
                    ?? new List<FinalQuizResultResponse>() 
                    })
                .ToList();


            var result = new SimilarCourseForStudentRequest
            {
                User = enrollments.FirstOrDefault()?.Student,
                Courses = courseResponse
            };

            return result;

        }

        public Task<IEnumerable<Course>> GetAllSimilarCoursesForStudentAsync(List<float> embeddingData)
        {
            var courses = _context.Courses
                .AsEnumerable()
                .Select(c => new
                {
                    Course = c,
                    SimilarScore = CommonUtils.CosineSimilarity(embeddingData, CommonUtils.StringToObject<List<float>>(c.EmbeddingData))
                })
                .OrderByDescending(c => c.SimilarScore)
                .Take(5)
                .Select(c => c.Course)
                .ToList();

            return Task.FromResult(courses.AsEnumerable());
        }

        private TimeSpan GetAverageStudyingTime(int courseId, string studentId)
        {
            var data = _context.Processitems
                .Include(c => c.Process).ThenInclude(c => c.Course).ThenInclude(c => c.CourseSections).ThenInclude(c => c.Lessons).ThenInclude(c => c.Lessonitems)
                .Include(c => c.Process).ThenInclude(c => c.Course).ThenInclude(c => c.StudentEnrollments)
                .Where(x => x.Process.CourseId == courseId && x.Process.Course.StudentEnrollments.Any(c => c.StudentId == studentId))
                .OrderBy(x => x.UpdatedAt)
                .Select(x => x.UpdatedAt)
                .ToList();


            var durations = new List<TimeSpan>();
            
            for (int i = 1; i < data.Count; i++)
            {
                // chắc chắn cả 2 đều có giá trị
                var d1 = data[i];
                var d2 = data[i - 1];

                durations.Add(d1 - d2);
            }

            if (!durations.Any())
            {
                return TimeSpan.Zero;
            }

            var average = TimeSpan.FromTicks((long)durations.Average(x => x.Ticks));

            return average;
        }

        public async Task<List<ActivityCountResponse>> GetActivityForYear(string userId, int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year + 1, 1, 1);

            var courseQuery = _context.Processitems
                .Where(x => x.Process.StudentId == userId
                    && x.UpdatedAt >= start
                    && x.UpdatedAt < end)
                .Select(x => new { Date = x.UpdatedAt });

            var quizQuery = _context.Quizresults
                .Where(x => x.UserId == userId
                    && x.CreatedAt >= start
                    && x.CreatedAt < end)
                .Select(x => new { Date = x.CreatedAt });

            var finalQuizQuery = _context.Finalquizresults
                .Where(x => x.UserId == userId
                    && x.CreatedAt >= start
                    && x.CreatedAt < end)
                .Select(x => new { Date = x.CreatedAt });

            // Gộp 3 bảng
            var merged = courseQuery
                .Concat(quizQuery)
                .Concat(finalQuizQuery);

            // Lấy dữ liệu raw
            var raw = await merged
                .GroupBy(x => x.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Tạo full 365 ngày
            var days = Enumerable.Range(0, 365)
                .Select(i => start.AddDays(i))
                .Select(day =>
                {
                    var match = raw.FirstOrDefault(x => x.Date == day.Date);

                    return new ActivityCountResponse
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Count = match?.Count ?? 0   // 👈 ngày không có thì = 0
                    };
                })
                .ToList();

            return days;
        }
    }
}
