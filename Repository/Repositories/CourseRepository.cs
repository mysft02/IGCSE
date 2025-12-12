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
using Common.Constants;
using BusinessObject.DTOs.Response.Dashboards;
using System.Text.RegularExpressions;
using MySqlConnector;
using System.Data;
using System.Globalization;

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
                .Include(c => c.Module)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        // Override to always include Module when fetching all
        public new async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Set<Course>()
                .Include(c => c.Module)
                .ToListAsync();
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

        public async Task<(IEnumerable<Course> items, int total)> SearchAsync(int page, int pageSize, CourseListQuery query, string userRole)
        {
            var courseQuery = _context.Set<Course>()
                .Include(c => c.Module)
                .AsQueryable();

            if (userRole == null)
            {
                courseQuery = courseQuery.Where(x => x.Status == "Open");
            }
            else
            {
                if(userRole == "Parent" || userRole == "Student")
                {
                    courseQuery = courseQuery.Where(x => x.Status == "Open");
                }
            }

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
                    ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? "" : CommonUtils.GetMediaUrl(x.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
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
                    ImageUrl = string.IsNullOrEmpty(c.ImageUrl) ? "" : CommonUtils.GetMediaUrl(c.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor),
                    IsEnrolled = true,
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
                                Content = string.IsNullOrEmpty(p.Content) ? "" : CommonUtils.GetMediaUrl(p.Content, _webHostEnvironment.WebRootPath, _httpContextAccessor),
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

        public async Task<TeacherAnalyticsResponse> GetAnalytics(string userId, BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            var grouped = await _context.Courses
                .Where(x => x.CreatedBy == userId)
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                })
                .ToListAsync();

            var courseCount = new CourseCountMetadata
            {
                TotalCourses = grouped.Sum(x => x.Count),
                Pending = grouped.FirstOrDefault(x => x.Status == "Pending")?.Count ?? 0,
                Open = grouped.FirstOrDefault(x => x.Status == "Open")?.Count ?? 0,
                Rejected = grouped.FirstOrDefault(x => x.Status == "Rejected")?.Count ?? 0,
                Draft = grouped.FirstOrDefault(x => x.Status == "Draft")?.Count ?? 0
            };

            // Xác định startDate và endDate (mặc định: đầu năm - cuối năm)
            var startDate = dateRange?.StartDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);

            // Tự động xác định time unit dựa trên khoảng thời gian
            var timeSpan = endDate - startDate;
            Common.Constants.TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                // Nếu <= 24 giờ -> theo giờ
                timeUnit = Common.Constants.TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                // Nếu <= 31 ngày -> theo ngày
                timeUnit = Common.Constants.TimeUnitEnum.Day;
            }
            else
            {
                // Nếu > 31 ngày -> theo tháng
                timeUnit = Common.Constants.TimeUnitEnum.Month;
            }

            // Build SQL query với time unit tương ứng
            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case Common.Constants.TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(th.TransactionDate) AS Year,
                        MONTH(th.TransactionDate) AS Month,
                        DAY(th.TransactionDate) AS Day,
                        HOUR(th.TransactionDate) AS Hour";
                    groupByClause = "YEAR(th.TransactionDate), MONTH(th.TransactionDate), DAY(th.TransactionDate), HOUR(th.TransactionDate)";
                    break;
                case Common.Constants.TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(th.TransactionDate) AS Year,
                        MONTH(th.TransactionDate) AS Month,
                        DAY(th.TransactionDate) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(th.TransactionDate), MONTH(th.TransactionDate), DAY(th.TransactionDate)";
                    break;
                default: // Month
                    selectClause = @"
                        YEAR(th.TransactionDate) AS Year,
                        MONTH(th.TransactionDate) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(th.TransactionDate), MONTH(th.TransactionDate)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    SUM(th.Amount) AS TotalIncome
                FROM transactionhistory th
                INNER JOIN course c ON th.ItemID = c.CourseID
                WHERE th.TransactionDate IS NOT NULL
                AND th.TransactionDate >= @StartDate
                AND th.TransactionDate <= @EndDate
                AND c.CreatedBy = @UserId";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));
            sqlParams.Add(new MySqlParameter("@UserId", userId));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            // Execute raw SQL query
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            // Read income data from database
            var incomeDict = new Dictionary<string, decimal>();

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var totalIncome = reader.GetDecimal("TotalIncome");

                string key = timeUnit switch
                {
                    Common.Constants.TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    Common.Constants.TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                incomeDict[key] = totalIncome;
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            // Build metadata
            var metadata = new TeacherAnalyticsMetadata
            {
                TotalCourses = courseCount.TotalCourses,
                Pending = courseCount.Pending,
                Open = courseCount.Open,
                Rejected = courseCount.Rejected,
                Draft = courseCount.Draft
            };

            // Build chart data - fill đầy đủ các time slots
            var chart = new List<IncomeChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;
                int month;

                switch (timeUnit)
                {
                    case Common.Constants.TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Hour}:00";
                        month = current.Month;
                        current = current.AddHours(1);
                        break;

                    case Common.Constants.TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        month = current.Month;
                        current = current.AddDays(1);
                        break;

                    default: // Month
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        month = current.Month;
                        current = current.AddMonths(1);
                        break;
                }

                var totalIncome = incomeDict.GetValueOrDefault(key, 0);
                chart.Add(new IncomeChartDataPoint
                {
                    Label = label,
                    Month = month,
                    TotalIncome = totalIncome,
                    ReceivedIncome = totalIncome * 0.7m
                });
            }

            var result = new TeacherAnalyticsResponse
            {
                Metadata = metadata,
                Chart = chart
            };

            return result;
        }

        public async Task<AdminAnalyticsResponse> GetAdminAnalytics()
        {
            var courseGrouped = await _context.Courses
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                })
                .ToListAsync();

            var courseCount = new CourseCountMetadata
            {
                TotalCourses = courseGrouped.Sum(x => x.Count),
                Pending = courseGrouped.FirstOrDefault(x => x.Status == "Pending")?.Count ?? 0,
                Open = courseGrouped.FirstOrDefault(x => x.Status == "Open")?.Count ?? 0,
                Rejected = courseGrouped.FirstOrDefault(x => x.Status == "Rejected")?.Count ?? 0,
                Draft = courseGrouped.FirstOrDefault(x => x.Status == "Draft")?.Count ?? 0
            };

            var incomeRaw = await _context.Transactionhistories
                .GroupBy(x => x.TransactionDate.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalIncome = g.Sum(c => c.Amount),
                })
                .ToListAsync();

            var roleCounts = await (
                from role in _context.Roles
                join userRole in _context.UserRoles
                on role.Id equals userRole.RoleId into roleGroup
                select new
                {
                    RoleName = role.Name,
                    Count = roleGroup.Count()
                })
                .ToListAsync();

            var accountCount = new AccountCountMetadata
            {
                TotalAccounts = await _context.Users.CountAsync(),
                Teacher = roleCounts.FirstOrDefault(x => x.RoleName == "Teacher")?.Count ?? 0,
                Parent = roleCounts.FirstOrDefault(x => x.RoleName == "Parent")?.Count ?? 0,
                Student = roleCounts.FirstOrDefault(x => x.RoleName == "Student")?.Count ?? 0
            };

            // Build metadata
            var metadata = new AdminAnalyticsMetadata
            {
                Course = courseCount,
                Accounts = accountCount
            };

            // Build chart data
            var chart = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var data = incomeRaw.FirstOrDefault(x => x.Month == month);
                    return new IncomeChartDataPoint
                    {
                        Label = $"Tháng {month}",
                        Month = month,
                        TotalIncome = data?.TotalIncome ?? 0,
                        ReceivedIncome = (data?.TotalIncome ?? 0) * 0.3m
                    };
                })
                .ToList();

            var result = new AdminAnalyticsResponse
            {
                Metadata = metadata,
                Chart = chart
            };

            return result;
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.StudentLearningAbilityResponse> GetStudentLearningAbilityAsync(
            BusinessObject.Payload.Request.Filter.StudentLearningAbilityQueryRequest query)
        {
            // Build SQL query with parameters
            var sqlParams = new List<MySqlParameter>();

            // Base SQL query to get student learning statistics
            // Using subquery to calculate completion percentage correctly
            var sql = @"
                SELECT 
                    p.StudentID AS StudentId,
                    p.CourseID AS CourseId,
                    c.Name AS CourseName,
                    u.UserName AS StudentName,
                    u.Email AS StudentEmail,
                    COUNT(DISTINCT pi.ProcessItemID) AS CompletedLessonItems,
                    COUNT(DISTINCT li.LessonItemID) AS TotalLessonItems,
                    MIN(p.CreatedAt) AS StartedAt,
                    CASE 
                        WHEN COUNT(DISTINCT pi.ProcessItemID) = COUNT(DISTINCT li.LessonItemID) 
                        THEN MAX(pi.CreatedAt)
                        ELSE NULL
                    END AS CompletedAt,
                    CASE 
                        WHEN COUNT(DISTINCT li.LessonItemID) > 0 
                        THEN ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        ELSE 0 
                    END AS CompletionPercentage
                FROM process p
                INNER JOIN course c ON p.CourseID = c.CourseID
                INNER JOIN coursesection cs ON cs.CourseID = c.CourseID
                INNER JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                INNER JOIN lessonitem li ON li.LessonID = l.LessonID
                LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                LEFT JOIN AspNetUsers u ON u.Id = p.StudentID
                WHERE 1=1";

            // Apply filters
            if (query.CourseId.HasValue)
            {
                sql += " AND p.CourseID = @CourseId";
                sqlParams.Add(new MySqlParameter("@CourseId", query.CourseId.Value));
            }

            if (!CommonUtils.IsEmptyString(query.StudentId))
            {
                sql += " AND p.StudentID = @StudentId";
                sqlParams.Add(new MySqlParameter("@StudentId", query.StudentId));
            }

            if (!CommonUtils.IsEmptyString(query.TeacherId))
            {
                sql += " AND c.CreatedBy = @TeacherId";
                sqlParams.Add(new MySqlParameter("@TeacherId", query.TeacherId));
            }

            if (query.StartDate.HasValue)
            {
                sql += " AND (pi.CreatedAt >= @StartDate OR pi.CreatedAt IS NULL)";
                sqlParams.Add(new MySqlParameter("@StartDate", query.StartDate.Value));
            }

            if (query.EndDate.HasValue)
            {
                sql += " AND (pi.CreatedAt <= @EndDate OR pi.CreatedAt IS NULL)";
                sqlParams.Add(new MySqlParameter("@EndDate", query.EndDate.Value));
            }

            sql += @"
                GROUP BY p.StudentID, p.CourseID, c.Name, u.UserName, u.Email";

            // Apply MinCompletionPercentage filter using HAVING
            var havingConditions = new List<string>();
            if (query.MinCompletionPercentage.HasValue)
            {
                havingConditions.Add("CompletionPercentage >= @MinCompletionPercentage");
                sqlParams.Add(new MySqlParameter("@MinCompletionPercentage", query.MinCompletionPercentage.Value));
            }

            // Apply OnlyCompletedCourses filter
            if (query.OnlyCompletedCourses == true)
            {
                havingConditions.Add("CompletionPercentage >= 100");
            }

            if (havingConditions.Any())
            {
                sql += " HAVING " + string.Join(" AND ", havingConditions);
            }

            // Execute raw SQL query
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var students = new Dictionary<string, BusinessObject.DTOs.Response.Dashboards.StudentLearningStatistic>();
            var allCourseDetails = new List<BusinessObject.DTOs.Response.Dashboards.CourseCompletionDetail>();

            // Read results
            while (await reader.ReadAsync())
            {
                var studentId = reader.GetString("StudentId");
                var courseId = reader.GetInt32("CourseId");
                var courseName = reader.IsDBNull("CourseName") ? "" : reader.GetString("CourseName");
                var studentName = reader.IsDBNull("StudentName") ? null : reader.GetString("StudentName");
                var studentEmail = reader.IsDBNull("StudentEmail") ? null : reader.GetString("StudentEmail");
                var completedLessonItems = reader.GetInt32("CompletedLessonItems");
                var totalLessonItems = reader.GetInt32("TotalLessonItems");
                var completionPercentage = reader.GetDecimal("CompletionPercentage");
                var startedAt = reader.IsDBNull("StartedAt") ? (DateTime?)null : reader.GetDateTime("StartedAt");
                var completedAt = reader.IsDBNull("CompletedAt") ? (DateTime?)null : reader.GetDateTime("CompletedAt");

                // Get or create student statistic
                if (!students.ContainsKey(studentId))
                {
                    students[studentId] = new BusinessObject.DTOs.Response.Dashboards.StudentLearningStatistic
                    {
                        StudentId = studentId,
                        StudentName = studentName,
                        StudentEmail = studentEmail,
                        CourseDetails = new List<BusinessObject.DTOs.Response.Dashboards.CourseCompletionDetail>()
                    };
                }

                var studentStat = students[studentId];

                // Add course detail
                var courseDetail = new BusinessObject.DTOs.Response.Dashboards.CourseCompletionDetail
                {
                    CourseId = courseId,
                    CourseName = courseName,
                    TotalLessonItems = totalLessonItems,
                    CompletedLessonItems = completedLessonItems,
                    CompletionPercentage = completionPercentage,
                    IsCompleted = completionPercentage >= 100,
                    StartedAt = startedAt,
                    CompletedAt = completedAt
                };

                studentStat.CourseDetails.Add(courseDetail);
                allCourseDetails.Add(courseDetail);
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            // Calculate student-level statistics
            var studentStatistics = students.Values.ToList();
            foreach (var studentStat in studentStatistics)
            {
                studentStat.TotalEnrolledCourses = studentStat.CourseDetails.Count;
                studentStat.CompletedCourses = studentStat.CourseDetails.Count(cd => cd.IsCompleted);
                studentStat.InProgressCourses = studentStat.CourseDetails.Count(cd => !cd.IsCompleted);
                studentStat.AverageCompletionPercentage = studentStat.CourseDetails.Any()
                    ? studentStat.CourseDetails.Average(cd => cd.CompletionPercentage)
                    : 0;
            }

            // Apply pagination if needed
            var totalStudents = studentStatistics.Count;
            if (query.Page > 0 || query.Size > 0)
            {
                studentStatistics = studentStatistics
                    .Skip(query.Page * query.GetPageSize())
                    .Take(query.GetPageSize())
                    .ToList();
            }

            // Calculate overall statistics (based on all students, not just paginated)
            var overallStats = new BusinessObject.DTOs.Response.Dashboards.OverallLearningStatistic();
            if (students.Values.Any())
            {
                var allStudentStats = students.Values.ToList();
                overallStats.AverageCompletionPercentage = allStudentStats
                    .Average(s => s.AverageCompletionPercentage);
                overallStats.TotalCompletedCourses = allStudentStats
                    .Sum(s => s.CompletedCourses);
                overallStats.TotalInProgressCourses = allStudentStats
                    .Sum(s => s.InProgressCourses);
                overallStats.StudentsWithCompletedCourses = allStudentStats
                    .Count(s => s.CompletedCourses > 0);
                overallStats.StudentsWithNoCompletedCourses = allStudentStats
                    .Count(s => s.CompletedCourses == 0);
            }

            return new BusinessObject.DTOs.Response.Dashboards.StudentLearningAbilityResponse
            {
                TotalStudents = totalStudents,
                Students = studentStatistics,
                Overall = overallStats
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.LessonCompletionResponse> GetLessonCompletionAsync(
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest query,
            string teacherId)
        {
            // Set default dates: đầu năm và cuối năm hiện tại
            var now = DateTime.Now;
            var startDate = query.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = query.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            // Tự động xác định time unit dựa trên khoảng thời gian
            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                // Nếu <= 24 giờ -> theo giờ
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                // Nếu <= 31 ngày -> theo ngày
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                // Nếu > 31 ngày -> theo tháng
                timeUnit = TimeUnitEnum.Month;
            }

            // Build SQL query với time unit tương ứng
            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        DAY(pi.CreatedAt) AS Day,
                        HOUR(pi.CreatedAt) AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt), DAY(pi.CreatedAt), HOUR(pi.CreatedAt)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        DAY(pi.CreatedAt) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt), DAY(pi.CreatedAt)";
                    break;
                default: // Month
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    COUNT(DISTINCT p.StudentID) AS CompletedUsersCount,
                    COUNT(DISTINCT pi.ProcessItemID) AS CompletedLessonItemsCount
                FROM processitem pi
                INNER JOIN process p ON pi.ProcessID = p.ProcessID
                INNER JOIN course c ON p.CourseID = c.CourseID
                WHERE pi.CreatedAt IS NOT NULL
                AND pi.CreatedAt >= @StartDate
                AND pi.CreatedAt <= @EndDate";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));

            // Apply filters - chỉ filter theo teacher
            if (!CommonUtils.IsEmptyString(teacherId))
            {
                sql += " AND c.CreatedBy = @TeacherId";
                sqlParams.Add(new MySqlParameter("@TeacherId", teacherId));
            }

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            // Execute raw SQL query
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            // Internal class để lưu data từ database
            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, string Label, int CompletedUsersCount, int CompletedLessonItemsCount)>();
            var totalCompletedUsers = new HashSet<string>();
            var totalCompletedLessonItems = 0;

            // Read results from database
            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var completedUsersCount = reader.GetInt32("CompletedUsersCount");
                var completedLessonItemsCount = reader.GetInt32("CompletedLessonItemsCount");

                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{year}-{month:D2}-{day:D2}-{hour:D2}";
                        label = $"{day}/{month} {hour}:00";
                        break;
                    case TimeUnitEnum.Day:
                        key = $"{year}-{month:D2}-{day:D2}";
                        label = $"{day}/{month}";
                        break;
                    default: // Month
                        key = $"{year}-{month:D2}";
                        label = $"T{month}";
                        break;
                }

                statsDict[key] = (year, month, day, hour, label, completedUsersCount, completedLessonItemsCount);
                totalCompletedLessonItems += completedLessonItemsCount;
            }

            await reader.CloseAsync();

            // Get unique users
            var usersSql = @"
                SELECT DISTINCT p.StudentID
                FROM processitem pi
                INNER JOIN process p ON pi.ProcessID = p.ProcessID
                INNER JOIN course c ON p.CourseID = c.CourseID
                WHERE pi.CreatedAt IS NOT NULL
                AND pi.CreatedAt >= @StartDate
                AND pi.CreatedAt <= @EndDate";

            var usersParams = new List<MySqlParameter>
            {
                new MySqlParameter("@StartDate", startDate),
                new MySqlParameter("@EndDate", endDate)
            };

            if (!CommonUtils.IsEmptyString(teacherId))
            {
                usersSql += " AND c.CreatedBy = @TeacherId";
                usersParams.Add(new MySqlParameter("@TeacherId", teacherId));
            }

            var usersCommand = connection.CreateCommand();
            usersCommand.CommandText = usersSql;
            usersCommand.Parameters.AddRange(usersParams.ToArray());

            var usersReader = await usersCommand.ExecuteReaderAsync();
            while (await usersReader.ReadAsync())
            {
                totalCompletedUsers.Add(usersReader.GetString("StudentID"));
            }

            await usersReader.CloseAsync();

            // Get unique users per time unit
            var usersByTimeUnitSql = usersSql.Replace("SELECT DISTINCT p.StudentID", 
                timeUnit == TimeUnitEnum.Hour 
                    ? "SELECT YEAR(pi.CreatedAt) AS Year, MONTH(pi.CreatedAt) AS Month, DAY(pi.CreatedAt) AS Day, HOUR(pi.CreatedAt) AS Hour, p.StudentID"
                    : timeUnit == TimeUnitEnum.Day
                    ? "SELECT YEAR(pi.CreatedAt) AS Year, MONTH(pi.CreatedAt) AS Month, DAY(pi.CreatedAt) AS Day, p.StudentID"
                    : "SELECT YEAR(pi.CreatedAt) AS Year, MONTH(pi.CreatedAt) AS Month, p.StudentID");

            var usersByTimeUnitCommand = connection.CreateCommand();
            usersByTimeUnitCommand.CommandText = usersByTimeUnitSql;
            usersByTimeUnitCommand.Parameters.AddRange(usersParams.ToArray());

            var usersByTimeUnitReader = await usersByTimeUnitCommand.ExecuteReaderAsync();
            var usersByTimeUnitDict = new Dictionary<string, HashSet<string>>();

            while (await usersByTimeUnitReader.ReadAsync())
            {
                var year = usersByTimeUnitReader.GetInt32("Year");
                var month = usersByTimeUnitReader.GetInt32("Month");
                var userId = usersByTimeUnitReader.GetString("StudentID");
                
                string key;
                if (timeUnit == TimeUnitEnum.Hour)
                {
                    var day = usersByTimeUnitReader.GetInt32("Day");
                    var hour = usersByTimeUnitReader.GetInt32("Hour");
                    key = $"{year}-{month:D2}-{day:D2}-{hour:D2}";
                }
                else if (timeUnit == TimeUnitEnum.Day)
                {
                    var day = usersByTimeUnitReader.GetInt32("Day");
                    key = $"{year}-{month:D2}-{day:D2}";
                }
                else
                {
                    key = $"{year}-{month:D2}";
                }

                if (!usersByTimeUnitDict.ContainsKey(key))
                {
                    usersByTimeUnitDict[key] = new HashSet<string>();
                }

                usersByTimeUnitDict[key].Add(userId);
            }

            await usersByTimeUnitReader.CloseAsync();
            await connection.CloseAsync();

            // Update user counts per time unit
            var updatedStatsDict = new Dictionary<string, (string Label, int CompletedUsersCount, int CompletedLessonItemsCount)>();
            foreach (var kvp in statsDict)
            {
                var userCount = usersByTimeUnitDict.ContainsKey(kvp.Key) ? usersByTimeUnitDict[kvp.Key].Count : 0;
                updatedStatsDict[kvp.Key] = (kvp.Value.Label, userCount, kvp.Value.CompletedLessonItemsCount);
            }

            // Fill all time slots and build chart data
            var chart = new List<BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint>();
            
            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    var currentHour = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
                    while (currentHour <= endDate)
                    {
                        var key = $"{currentHour.Year}-{currentHour.Month:D2}-{currentHour.Day:D2}-{currentHour.Hour:D2}";
                        var label = $"{currentHour.Hour}:00";
                        
                        if (updatedStatsDict.ContainsKey(key))
                        {
                            var stat = updatedStatsDict[key];
                            chart.Add(new BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint
                            {
                                Label = stat.Label,
                                CompletedUsersCount = stat.CompletedUsersCount,
                                CompletedLessonItemsCount = stat.CompletedLessonItemsCount
                            });
                        }
                        else
                        {
                            chart.Add(new BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint
                            {
                                Label = label,
                                CompletedUsersCount = 0,
                                CompletedLessonItemsCount = 0
                            });
                        }
                        currentHour = currentHour.AddHours(1);
                    }
                    break;
                case TimeUnitEnum.Day:
                    var currentDay = new DateTime(startDate.Year, startDate.Month, startDate.Day);
                    while (currentDay <= endDate)
                    {
                        var key = $"{currentDay.Year}-{currentDay.Month:D2}-{currentDay.Day:D2}";
                        var label = $"{currentDay.Day}/{currentDay.Month}";
                        
                        if (updatedStatsDict.ContainsKey(key))
                        {
                            var stat = updatedStatsDict[key];
                            chart.Add(new BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint
                            {
                                Label = stat.Label,
                                CompletedUsersCount = stat.CompletedUsersCount,
                                CompletedLessonItemsCount = stat.CompletedLessonItemsCount
                            });
                        }
                        else
                        {
                            chart.Add(new BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint
                            {
                                Label = label,
                                CompletedUsersCount = 0,
                                CompletedLessonItemsCount = 0
                            });
                        }
                        currentDay = currentDay.AddDays(1);
                    }
                    break;
                default: // Month
                    var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
                    var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                    while (currentMonth <= endMonth)
                    {
                        var key = $"{currentMonth.Year}-{currentMonth.Month:D2}";
                        var label = $"T{currentMonth.Month}";
                        
                        if (updatedStatsDict.ContainsKey(key))
                        {
                            var stat = updatedStatsDict[key];
                            chart.Add(new BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint
                            {
                                Label = stat.Label,
                                CompletedUsersCount = stat.CompletedUsersCount,
                                CompletedLessonItemsCount = stat.CompletedLessonItemsCount
                            });
                        }
                        else
                        {
                            chart.Add(new BusinessObject.DTOs.Response.Dashboards.LessonCompletionChartDataPoint
                            {
                                Label = label,
                                CompletedUsersCount = 0,
                                CompletedLessonItemsCount = 0
                            });
                        }
                        currentMonth = currentMonth.AddMonths(1);
                    }
                    break;
            }

            // Build metadata
            var metadata = new LessonCompletionMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalCompletedUsers = totalCompletedUsers.Count,
                TotalCompletedLessonItems = totalCompletedLessonItems
            };

            return new BusinessObject.DTOs.Response.Dashboards.LessonCompletionResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.TopStudentsResponse> GetTopStudentsAsync(string? teacherId = null, int limit = 10)
        {
            var sqlParams = new List<MySqlParameter>();
            
            var sql = @"
                SELECT 
                    se.StudentID AS StudentId,
                    u.UserName AS StudentName,
                    u.Email AS StudentEmail,
                    COUNT(DISTINCT se.CourseID) AS TotalEnrolledCourses,
                    COUNT(DISTINCT CASE WHEN completion.CompletionPercentage >= 100 THEN se.CourseID END) AS CompletedCourses,
                    COALESCE(AVG(completion.CompletionPercentage), 0) AS AverageCompletionPercentage,
                    COUNT(DISTINCT pi.ProcessItemID) AS TotalCompletedLessonItems,
                    COALESCE(AVG(score.AverageScore), 0) AS AverageScore
                FROM studentenrollment se
                INNER JOIN AspNetUsers u ON se.StudentID = u.Id";

            if (!CommonUtils.IsEmptyString(teacherId))
            {
                sql += " INNER JOIN course c ON se.CourseID = c.CourseID";
                sqlParams.Add(new MySqlParameter("@TeacherId", teacherId));
            }

            sql += @"
                LEFT JOIN (
                    SELECT 
                        p.StudentID,
                        p.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p
                    INNER JOIN course c ON p.CourseID = c.CourseID";

            if (!CommonUtils.IsEmptyString(teacherId))
            {
                sql += " AND c.CreatedBy = @TeacherId2";
                sqlParams.Add(new MySqlParameter("@TeacherId2", teacherId));
            }

            sql += @"
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p.StudentID, p.CourseID
                ) completion ON completion.StudentID = se.StudentID AND completion.CourseID = se.CourseID
                LEFT JOIN process p ON p.StudentID = se.StudentID AND p.CourseID = se.CourseID
                LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID
                LEFT JOIN (
                    SELECT 
                        p.StudentID,
                        AVG(COALESCE(fqr.Score, qr.Score)) AS AverageScore
                    FROM process p";

            if (!CommonUtils.IsEmptyString(teacherId))
            {
                sql += " INNER JOIN course c ON p.CourseID = c.CourseID AND c.CreatedBy = @TeacherId3";
                sqlParams.Add(new MySqlParameter("@TeacherId3", teacherId));
            }

            sql += @"
                    LEFT JOIN finalquizresult fqr ON fqr.UserID = p.StudentID
                    LEFT JOIN quizresult qr ON qr.UserID = p.StudentID
                    WHERE (fqr.Score IS NOT NULL OR qr.Score IS NOT NULL)
                    GROUP BY p.StudentID
                ) score ON score.StudentID = se.StudentID";

            if (!CommonUtils.IsEmptyString(teacherId))
            {
                sql += " WHERE c.CreatedBy = @TeacherId";
            }

            sql += @"
                GROUP BY se.StudentID, u.UserName, u.Email
                ORDER BY 
                    CompletedCourses DESC,
                    AverageCompletionPercentage DESC,
                    TotalCompletedLessonItems DESC,
                    AverageScore DESC
                LIMIT @Limit";

            sqlParams.Add(new MySqlParameter("@Limit", limit));

            // Get total count
            var countSql = @"SELECT COUNT(DISTINCT se.StudentID) FROM studentenrollment se";
            if (!CommonUtils.IsEmptyString(teacherId))
            {
                countSql += " INNER JOIN course c ON se.CourseID = c.CourseID WHERE c.CreatedBy = @TeacherId";
            }

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Get total count
            var countCommand = connection.CreateCommand();
            countCommand.CommandText = countSql;
            if (!CommonUtils.IsEmptyString(teacherId))
            {
                countCommand.Parameters.Add(new MySqlParameter("@TeacherId", teacherId));
            }
            var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            // Get top students
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();
            var students = new List<BusinessObject.DTOs.Response.Dashboards.TopStudentItem>();

            while (await reader.ReadAsync())
            {
                students.Add(new BusinessObject.DTOs.Response.Dashboards.TopStudentItem
                {
                    StudentId = reader.GetString("StudentId"),
                    StudentName = reader.IsDBNull("StudentName") ? null : reader.GetString("StudentName"),
                    StudentEmail = reader.IsDBNull("StudentEmail") ? null : reader.GetString("StudentEmail"),
                    TotalEnrolledCourses = reader.GetInt32("TotalEnrolledCourses"),
                    CompletedCourses = reader.GetInt32("CompletedCourses"),
                    AverageCompletionPercentage = reader.GetDecimal("AverageCompletionPercentage"),
                    TotalCompletedLessonItems = reader.GetInt32("TotalCompletedLessonItems"),
                    AverageScore = reader.IsDBNull("AverageScore") ? null : (decimal?)reader.GetDecimal("AverageScore")
                });
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            return new BusinessObject.DTOs.Response.Dashboards.TopStudentsResponse
            {
                Students = students,
                TotalStudents = totalCount
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.TopCoursesResponse> GetTopCoursesAsync(string? teacherId = null, int limit = 10)
        {
            var sqlParams = new List<MySqlParameter>();
            
            var sql = @"
                SELECT 
                    c.CourseID AS CourseId,
                    c.Name AS CourseName,
                    c.Description AS CourseDescription,
                    c.ImageUrl AS ImageUrl,
                    c.Price AS Price,
                    COUNT(DISTINCT se.StudentID) AS TotalEnrollments,
                    COUNT(DISTINCT p.StudentID) AS ActiveStudents,
                    COUNT(DISTINCT CASE WHEN completion.CompletionPercentage >= 100 THEN p.StudentID END) AS CompletedStudents,
                    COALESCE(AVG(completion.CompletionPercentage), 0) AS AverageCompletionPercentage,
                    COALESCE(AVG(fqr.Score), 0) AS AverageFinalScore,
                    COALESCE(SUM(th.Amount), 0) AS TotalIncome
                FROM course c
                LEFT JOIN studentenrollment se ON se.CourseID = c.CourseID
                LEFT JOIN process p ON p.CourseID = c.CourseID
                LEFT JOIN (
                    SELECT 
                        p.StudentID,
                        p.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p
                    INNER JOIN course c ON p.CourseID = c.CourseID
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p.StudentID, p.CourseID
                ) completion ON completion.CourseID = c.CourseID
                LEFT JOIN finalquizresult fqr ON fqr.UserID IN (
                    SELECT StudentID FROM process WHERE CourseID = c.CourseID
                )
                LEFT JOIN transactionhistory th ON th.ItemID = c.CourseID";

            if (!CommonUtils.IsEmptyString(teacherId))
            {
                sql += " WHERE c.CreatedBy = @TeacherId";
                sqlParams.Add(new MySqlParameter("@TeacherId", teacherId));
            }

            sql += @"
                GROUP BY c.CourseID, c.Name, c.Description, c.ImageUrl, c.Price
                HAVING TotalEnrollments > 0
                ORDER BY 
                    TotalEnrollments DESC,
                    ActiveStudents DESC,
                    CompletedStudents DESC,
                    AverageCompletionPercentage DESC
                LIMIT @Limit";

            sqlParams.Add(new MySqlParameter("@Limit", limit));

            // Get total count
            var countSql = "SELECT COUNT(*) FROM course c";
            if (!CommonUtils.IsEmptyString(teacherId))
            {
                countSql += " WHERE c.CreatedBy = @TeacherId";
            }

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Get total count
            var countCommand = connection.CreateCommand();
            countCommand.CommandText = countSql;
            if (!CommonUtils.IsEmptyString(teacherId))
            {
                countCommand.Parameters.Add(new MySqlParameter("@TeacherId", teacherId));
            }
            var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            // Get top courses
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();
            var courses = new List<BusinessObject.DTOs.Response.Dashboards.TopCourseItem>();

            while (await reader.ReadAsync())
            {
                var imageUrlValue = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl");
                var imageUrl = !string.IsNullOrEmpty(imageUrlValue)
                    ? CommonUtils.GetMediaUrl(imageUrlValue, _webHostEnvironment.WebRootPath, _httpContextAccessor)
                    : null;

                courses.Add(new BusinessObject.DTOs.Response.Dashboards.TopCourseItem
                {
                    CourseId = reader.GetInt32("CourseId"),
                    CourseName = reader.GetString("CourseName"),
                    CourseDescription = reader.IsDBNull("CourseDescription") ? null : reader.GetString("CourseDescription"),
                    ImageUrl = imageUrl,
                    Price = reader.IsDBNull("Price") ? null : (decimal?)reader.GetDecimal("Price"),
                    TotalEnrollments = reader.GetInt32("TotalEnrollments"),
                    ActiveStudents = reader.GetInt32("ActiveStudents"),
                    CompletedStudents = reader.GetInt32("CompletedStudents"),
                    AverageCompletionPercentage = reader.GetDecimal("AverageCompletionPercentage"),
                    AverageFinalScore = reader.IsDBNull("AverageFinalScore") || reader.GetDecimal("AverageFinalScore") == 0 
                        ? null 
                        : (decimal?)reader.GetDecimal("AverageFinalScore"),
                    TotalIncome = reader.GetDecimal("TotalIncome")
                });
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            return new BusinessObject.DTOs.Response.Dashboards.TopCoursesResponse
            {
                Courses = courses,
                TotalCourses = totalCount
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.TopAnalyticsResponse> GetTopAnalyticsAsync(string? teacherId = null, int limit = 10)
        {
            // Get top students and top courses sequentially to avoid connection conflicts
            var topStudents = await GetTopStudentsAsync(teacherId, limit);
            var topCourses = await GetTopCoursesAsync(teacherId, limit);

            return new BusinessObject.DTOs.Response.Dashboards.TopAnalyticsResponse
            {
                TopStudents = topStudents.Students,
                TotalStudents = topStudents.TotalStudents,
                TopCourses = topCourses.Courses,
                TotalCourses = topCourses.TotalCourses
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.StudentLearningProgressResponse> GetStudentLearningProgressAsync(
            string studentId, 
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            // Set default dates: đầu năm và cuối năm hiện tại
            var now = DateTime.Now;
            var startDate = dateRange?.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            // Tự động xác định time unit dựa trên khoảng thời gian
            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                timeUnit = TimeUnitEnum.Month;
            }

            // Build SQL query với time unit tương ứng
            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        DAY(pi.CreatedAt) AS Day,
                        HOUR(pi.CreatedAt) AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt), DAY(pi.CreatedAt), HOUR(pi.CreatedAt)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        DAY(pi.CreatedAt) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt), DAY(pi.CreatedAt)";
                    break;
                default: // Month
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    COUNT(DISTINCT pi.ProcessItemID) AS CompletedLessonItems,
                    ROUND(COUNT(DISTINCT pi.ProcessItemID) * 0.5, 2) AS StudyHours
                FROM processitem pi
                INNER JOIN process p ON pi.ProcessID = p.ProcessID
                WHERE pi.CreatedAt IS NOT NULL
                AND pi.CreatedAt >= @StartDate
                AND pi.CreatedAt <= @EndDate
                AND p.StudentID = @StudentId";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));
            sqlParams.Add(new MySqlParameter("@StudentId", studentId));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            // Execute raw SQL query
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, int CompletedLessonItems, decimal StudyHours)>();
            var totalCompletedLessonItems = 0;
            var totalStudyHours = 0m;

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var completedLessonItems = reader.GetInt32("CompletedLessonItems");
                var studyHours = reader.GetDecimal("StudyHours");

                string key = timeUnit switch
                {
                    TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                statsDict[key] = (year, month, day, hour, completedLessonItems, studyHours);
                totalCompletedLessonItems += completedLessonItems;
                totalStudyHours += studyHours;
            }

            await reader.CloseAsync();

            // Get active courses count
            var activeCoursesSql = @"
                SELECT COUNT(DISTINCT p.CourseID)
                FROM process p
                WHERE p.StudentID = @StudentId
                AND EXISTS (
                    SELECT 1 FROM processitem pi 
                    WHERE pi.ProcessID = p.ProcessID 
                    AND pi.CreatedAt >= @StartDate 
                    AND pi.CreatedAt <= @EndDate
                )";

            var activeCoursesCommand = connection.CreateCommand();
            activeCoursesCommand.CommandText = activeCoursesSql;
            activeCoursesCommand.Parameters.Add(new MySqlParameter("@StudentId", studentId));
            activeCoursesCommand.Parameters.Add(new MySqlParameter("@StartDate", startDate));
            activeCoursesCommand.Parameters.Add(new MySqlParameter("@EndDate", endDate));
            var activeCourses = Convert.ToInt32(await activeCoursesCommand.ExecuteScalarAsync());

            await connection.CloseAsync();

            // Build metadata
            var metadata = new BusinessObject.DTOs.Response.Dashboards.StudentLearningProgressMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalCompletedLessonItems = totalCompletedLessonItems,
                TotalStudyHours = totalStudyHours,
                ActiveCourses = activeCourses
            };

            // Build chart data - fill đầy đủ các time slots
            var chart = new List<BusinessObject.DTOs.Response.Dashboards.StudentLearningProgressChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Day}/{current.Month} {current.Hour}:00";
                        current = current.AddHours(1);
                        break;

                    case TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        current = current.AddDays(1);
                        break;

                    default: // Month
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        current = current.AddMonths(1);
                        break;
                }

                var stat = statsDict.GetValueOrDefault(key);
                chart.Add(new BusinessObject.DTOs.Response.Dashboards.StudentLearningProgressChartDataPoint
                {
                    Label = label,
                    CompletedLessonItems = stat.CompletedLessonItems,
                    StudyHours = stat.StudyHours
                });
            }

            return new BusinessObject.DTOs.Response.Dashboards.StudentLearningProgressResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.StudentCourseStatisticsResponse> GetStudentCourseStatisticsAsync(string studentId)
        {
            var sql = @"
                SELECT 
                    c.CourseID AS CourseId,
                    c.Name AS CourseName,
                    COUNT(DISTINCT li.LessonItemID) AS TotalLessonItems,
                    COUNT(DISTINCT pi.ProcessItemID) AS CompletedLessonItems,
                    CASE 
                        WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                        ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                    END AS CompletionPercentage,
                    CASE 
                        WHEN COUNT(DISTINCT pi.ProcessItemID) = COUNT(DISTINCT li.LessonItemID) AND COUNT(DISTINCT li.LessonItemID) > 0 THEN 'Completed'
                        WHEN COUNT(DISTINCT pi.ProcessItemID) > 0 THEN 'InProgress'
                        ELSE 'NotStarted'
                    END AS Status
                FROM studentenrollment se
                INNER JOIN course c ON se.CourseID = c.CourseID
                LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                LEFT JOIN process p ON p.CourseID = c.CourseID AND p.StudentID = se.StudentID
                LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                WHERE se.StudentID = @StudentId
                GROUP BY c.CourseID, c.Name
                ORDER BY CompletionPercentage DESC, c.Name";

            var sqlParams = new List<MySqlParameter>
            {
                new MySqlParameter("@StudentId", studentId)
            };

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var courses = new List<BusinessObject.DTOs.Response.Dashboards.StudentCourseStatisticsChartDataPoint>();
            var totalEnrolledCourses = 0;
            var completedCourses = 0;
            var inProgressCourses = 0;
            var notStartedCourses = 0;
            var totalCompletionPercentage = 0m;

            while (await reader.ReadAsync())
            {
                var courseId = reader.GetInt32("CourseId");
                var courseName = reader.GetString("CourseName");
                var totalLessonItems = reader.GetInt32("TotalLessonItems");
                var completedLessonItems = reader.GetInt32("CompletedLessonItems");
                var completionPercentage = reader.GetDecimal("CompletionPercentage");
                var status = reader.GetString("Status");

                totalEnrolledCourses++;
                if (status == "Completed") completedCourses++;
                else if (status == "InProgress") inProgressCourses++;
                else notStartedCourses++;

                totalCompletionPercentage += completionPercentage;

                courses.Add(new BusinessObject.DTOs.Response.Dashboards.StudentCourseStatisticsChartDataPoint
                {
                    Label = courseName,
                    CourseId = courseId,
                    CourseName = courseName,
                    CompletionPercentage = completionPercentage,
                    CompletedLessonItems = completedLessonItems,
                    TotalLessonItems = totalLessonItems,
                    Status = status
                });
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            // Build metadata
            var metadata = new BusinessObject.DTOs.Response.Dashboards.StudentCourseStatisticsMetadata
            {
                TotalEnrolledCourses = totalEnrolledCourses,
                CompletedCourses = completedCourses,
                InProgressCourses = inProgressCourses,
                NotStartedCourses = notStartedCourses,
                AverageCompletionPercentage = totalEnrolledCourses > 0 ? totalCompletionPercentage / totalEnrolledCourses : 0
            };

            return new BusinessObject.DTOs.Response.Dashboards.StudentCourseStatisticsResponse
            {
                Metadata = metadata,
                Chart = courses
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.CourseRecommendationResponse> GetCourseRecommendationsAsync(string studentId, int limit = 10)
        {
            // 1. Lấy thông tin thành tích học tập của học viên
            var achievementSql = @"
                SELECT 
                    COUNT(DISTINCT CASE WHEN completion.CompletionPercentage >= 100 THEN se.CourseID END) AS CompletedCourses,
                    COUNT(DISTINCT cc.CourseCertificateID) AS TotalCertificates,
                    COUNT(DISTINCT pi.ProcessItemID) AS TotalCompletedLessonItems,
                    COALESCE(AVG(COALESCE(fqr.Score, qr.Score)), 0) AS AverageScore
                FROM studentenrollment se
                LEFT JOIN (
                    SELECT 
                        p.StudentID,
                        p.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p
                    INNER JOIN course c ON p.CourseID = c.CourseID
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p.StudentID, p.CourseID
                ) completion ON completion.StudentID = se.StudentID AND completion.CourseID = se.CourseID
                LEFT JOIN processitem pi ON pi.ProcessID IN (
                    SELECT ProcessID FROM process WHERE StudentID = se.StudentID
                )
                LEFT JOIN coursecertificate cc ON cc.UserID = se.StudentID
                LEFT JOIN finalquizresult fqr ON fqr.UserID = se.StudentID
                LEFT JOIN quizresult qr ON qr.UserID = se.StudentID
                WHERE se.StudentID = @StudentId";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var achievementCommand = connection.CreateCommand();
            achievementCommand.CommandText = achievementSql;
            achievementCommand.Parameters.Add(new MySqlParameter("@StudentId", studentId));

            var achievementReader = await achievementCommand.ExecuteReaderAsync();
            int completedCourses = 0;
            int totalCertificates = 0;
            int totalCompletedLessonItems = 0;
            decimal averageScore = 0;

            if (await achievementReader.ReadAsync())
            {
                completedCourses = achievementReader.GetInt32("CompletedCourses");
                totalCertificates = achievementReader.GetInt32("TotalCertificates");
                totalCompletedLessonItems = achievementReader.GetInt32("TotalCompletedLessonItems");
                averageScore = achievementReader.GetDecimal("AverageScore");
            }
            await achievementReader.CloseAsync();

            // 2. Lấy embedding data từ các courses đã học của học viên
            var enrolledCourses = await _context.Studentenrollments
                .Where(se => se.StudentId == studentId)
                .Include(se => se.Course)
                .Select(se => se.Course)
                .Where(c => !string.IsNullOrEmpty(c.EmbeddingData))
                .ToListAsync();

            // Tính embedding trung bình từ các courses đã học
            List<float>? averageEmbedding = null;
            if (enrolledCourses.Any())
            {
                var embeddings = enrolledCourses
                    .Select(c => CommonUtils.StringToObject<List<float>>(c.EmbeddingData))
                    .Where(e => e != null && e.Any())
                    .ToList();

                if (embeddings.Any())
                {
                    var dimension = embeddings.First().Count;
                    averageEmbedding = new List<float>();
                    for (int i = 0; i < dimension; i++)
                    {
                        averageEmbedding.Add(embeddings.Average(e => e[i]));
                    }
                }
            }

            // 3. Lấy tất cả courses chưa đăng ký và tính recommendation score
            var allCourses = await _context.Courses
                .Where(c => c.Status == "Open" && !string.IsNullOrEmpty(c.EmbeddingData))
                .Where(c => !_context.Studentenrollments.Any(se => se.StudentId == studentId && se.CourseId == c.CourseId))
                .ToListAsync();

            var recommendations = allCourses
                .AsEnumerable()
                .Select(c =>
                {
                    // Tính embedding similarity (40%)
                    decimal embeddingScore = 0;
                    if (averageEmbedding != null && averageEmbedding.Any())
                    {
                        var courseEmbedding = CommonUtils.StringToObject<List<float>>(c.EmbeddingData);
                        if (courseEmbedding != null && courseEmbedding.Any())
                        {
                            embeddingScore = (decimal)CommonUtils.CosineSimilarity(averageEmbedding, courseEmbedding);
                        }
                    }

                    // Tính score dựa trên certificates (20%) - courses có nhiều certificates được học viên có certificates sẽ được ưu tiên
                    decimal certificateScore = totalCertificates > 0 ? 0.2m : 0;

                    // Tính score dựa trên số lesson items đã hoàn thành (20%)
                    decimal lessonItemsScore = Math.Min(totalCompletedLessonItems / 100.0m, 0.2m);

                    // Tính score dựa trên điểm trung bình (20%)
                    decimal averageScoreWeight = Math.Min(averageScore / 10.0m, 0.2m);

                    // Recommendation score tổng hợp
                    decimal recommendationScore = (embeddingScore * 0.4m) + certificateScore + lessonItemsScore + averageScoreWeight;

                    // Lý do được gợi ý
                    var reasons = new List<string>();
                    if (embeddingScore > 0.7m) reasons.Add("Tương đồng với các khoá học bạn đã học");
                    if (totalCertificates > 0) reasons.Add("Phù hợp với trình độ của bạn");
                    if (totalCompletedLessonItems > 50) reasons.Add("Dựa trên tiến độ học tập của bạn");
                    if (averageScore > 8) reasons.Add("Phù hợp với năng lực học tập của bạn");

                    return new
                    {
                        Course = c,
                        SimilarityScore = embeddingScore,
                        RecommendationScore = recommendationScore,
                        Reason = reasons.Any() ? string.Join(", ", reasons) : "Được gợi ý dựa trên sở thích học tập"
                    };
                })
                .OrderByDescending(r => r.RecommendationScore)
                .ThenByDescending(r => r.SimilarityScore)
                .Take(limit)
                .ToList();

            await connection.CloseAsync();

            // Build response
            var recommendedCourses = recommendations.Select(r =>
            {
                var imageUrl = !string.IsNullOrEmpty(r.Course.ImageUrl)
                    ? CommonUtils.GetMediaUrl(r.Course.ImageUrl, _webHostEnvironment.WebRootPath, _httpContextAccessor)
                    : null;

                return new BusinessObject.DTOs.Response.Dashboards.RecommendedCourseItem
                {
                    CourseId = r.Course.CourseId,
                    CourseName = r.Course.Name,
                    CourseDescription = r.Course.Description,
                    ImageUrl = imageUrl,
                    Price = r.Course.Price,
                    SimilarityScore = r.SimilarityScore,
                    RecommendationScore = r.RecommendationScore,
                    RecommendationReason = r.Reason
                };
            }).ToList();

            return new BusinessObject.DTOs.Response.Dashboards.CourseRecommendationResponse
            {
                RecommendedCourses = recommendedCourses,
                TotalRecommended = recommendedCourses.Count,
                StudentAchievement = new BusinessObject.DTOs.Response.Dashboards.StudentAchievementInfo
                {
                    CompletedCourses = completedCourses,
                    TotalCertificates = totalCertificates,
                    TotalCompletedLessonItems = totalCompletedLessonItems,
                    AverageScore = averageScore > 0 ? (decimal?)averageScore : null
                }
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.AdminOverviewResponse> GetAdminOverviewAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Accounts overview - Raw SQL
            var accountsSql = @"
                SELECT 
                    COUNT(DISTINCT u.Id) AS TotalAccounts,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Teacher') THEN u.Id END) AS Teachers,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Student') THEN u.Id END) AS Students,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Parent') THEN u.Id END) AS Parents,
                    COUNT(DISTINCT CASE WHEN se.EnrolledAt >= @StartOfMonth THEN u.Id END) AS NewThisMonth
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                LEFT JOIN studentenrollment se ON se.StudentID = u.Id";

            var accountsCommand = connection.CreateCommand();
            accountsCommand.CommandText = accountsSql;
            accountsCommand.Parameters.Add(new MySqlParameter("@StartOfMonth", startOfMonth));

            var accountsReader = await accountsCommand.ExecuteReaderAsync();
            int totalAccounts = 0, teachers = 0, students = 0, parents = 0, newAccountsThisMonth = 0;
            if (await accountsReader.ReadAsync())
            {
                totalAccounts = accountsReader.GetInt32("TotalAccounts");
                teachers = accountsReader.GetInt32("Teachers");
                students = accountsReader.GetInt32("Students");
                parents = accountsReader.GetInt32("Parents");
                newAccountsThisMonth = accountsReader.GetInt32("NewThisMonth");
            }
            await accountsReader.CloseAsync();

            // Courses overview - Raw SQL
            var coursesSql = @"
                SELECT 
                    Status,
                    COUNT(*) AS Count
                FROM course
                GROUP BY Status";

            var coursesCommand = connection.CreateCommand();
            coursesCommand.CommandText = coursesSql;
            var coursesReader = await coursesCommand.ExecuteReaderAsync();

            var courseDict = new Dictionary<string, int>();
            while (await coursesReader.ReadAsync())
            {
                var status = coursesReader.GetString("Status");
                var count = coursesReader.GetInt32("Count");
                courseDict[status] = count;
            }
            await coursesReader.CloseAsync();

            var newCoursesThisMonthSql = "SELECT COUNT(*) FROM course WHERE CreatedAt >= @StartOfMonth";
            var newCoursesCommand = connection.CreateCommand();
            newCoursesCommand.CommandText = newCoursesThisMonthSql;
            newCoursesCommand.Parameters.Add(new MySqlParameter("@StartOfMonth", startOfMonth));
            var newCoursesThisMonth = Convert.ToInt32(await newCoursesCommand.ExecuteScalarAsync());

            // Revenue overview - Raw SQL
            var revenueSql = @"
                SELECT 
                    COALESCE(SUM(Amount), 0) AS TotalRevenue,
                    COALESCE(SUM(CASE WHEN TransactionDate >= @StartOfMonth THEN Amount ELSE 0 END), 0) AS ThisMonth,
                    COALESCE(SUM(CASE WHEN TransactionDate >= @StartOfLastMonth AND TransactionDate <= @EndOfLastMonth THEN Amount ELSE 0 END), 0) AS LastMonth,
                    COUNT(*) AS TotalTransactions
                FROM transactionhistory
                WHERE TransactionDate IS NOT NULL";

            var revenueCommand = connection.CreateCommand();
            revenueCommand.CommandText = revenueSql;
            revenueCommand.Parameters.Add(new MySqlParameter("@StartOfMonth", startOfMonth));
            revenueCommand.Parameters.Add(new MySqlParameter("@StartOfLastMonth", startOfLastMonth));
            revenueCommand.Parameters.Add(new MySqlParameter("@EndOfLastMonth", endOfLastMonth));

            var revenueReader = await revenueCommand.ExecuteReaderAsync();
            decimal totalRevenue = 0, revenueThisMonth = 0, revenueLastMonth = 0;
            int totalTransactions = 0;
            if (await revenueReader.ReadAsync())
            {
                totalRevenue = revenueReader.GetDecimal("TotalRevenue");
                revenueThisMonth = revenueReader.GetDecimal("ThisMonth");
                revenueLastMonth = revenueReader.GetDecimal("LastMonth");
                totalTransactions = revenueReader.GetInt32("TotalTransactions");
            }
            await revenueReader.CloseAsync();

            // Enrollments overview - Raw SQL
            var enrollmentsSql = @"
                SELECT 
                    COUNT(*) AS TotalEnrollments,
                    COUNT(CASE WHEN EnrolledAt >= @StartOfMonth THEN 1 END) AS ThisMonth
                FROM studentenrollment";

            var enrollmentsCommand = connection.CreateCommand();
            enrollmentsCommand.CommandText = enrollmentsSql;
            enrollmentsCommand.Parameters.Add(new MySqlParameter("@StartOfMonth", startOfMonth));

            var enrollmentsReader = await enrollmentsCommand.ExecuteReaderAsync();
            int totalEnrollments = 0, enrollmentsThisMonth = 0;
            if (await enrollmentsReader.ReadAsync())
            {
                totalEnrollments = enrollmentsReader.GetInt32("TotalEnrollments");
                enrollmentsThisMonth = enrollmentsReader.GetInt32("ThisMonth");
            }
            await enrollmentsReader.CloseAsync();

            // Active and completed students - Raw SQL
            var activeStudentsSql = "SELECT COUNT(DISTINCT StudentID) FROM process";
            var activeStudentsCommand = connection.CreateCommand();
            activeStudentsCommand.CommandText = activeStudentsSql;
            var activeStudents = Convert.ToInt32(await activeStudentsCommand.ExecuteScalarAsync());

            var completedStudentsSql = @"
                SELECT COUNT(DISTINCT p.StudentID)
                FROM process p
                INNER JOIN (
                    SELECT 
                        p2.StudentID,
                        p2.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p2
                    INNER JOIN course c ON p2.CourseID = c.CourseID
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p2.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p2.StudentID, p2.CourseID
                    HAVING CompletionPercentage >= 100
                ) completed ON completed.StudentID = p.StudentID AND completed.CourseID = p.CourseID";

            var completedStudentsCommand = connection.CreateCommand();
            completedStudentsCommand.CommandText = completedStudentsSql;
            var completedStudents = Convert.ToInt32(await completedStudentsCommand.ExecuteScalarAsync());

            await connection.CloseAsync();

            return new BusinessObject.DTOs.Response.Dashboards.AdminOverviewResponse
            {
                Accounts = new BusinessObject.DTOs.Response.Dashboards.AccountOverview
                {
                    Total = totalAccounts,
                    Teachers = teachers,
                    Students = students,
                    Parents = parents,
                    NewThisMonth = newAccountsThisMonth
                },
                Courses = new BusinessObject.DTOs.Response.Dashboards.CourseOverview
                {
                    Total = courseDict.Values.Sum(),
                    Open = courseDict.GetValueOrDefault("Open", 0),
                    Pending = courseDict.GetValueOrDefault("Pending", 0),
                    Rejected = courseDict.GetValueOrDefault("Rejected", 0),
                    NewThisMonth = newCoursesThisMonth
                },
                Revenue = new BusinessObject.DTOs.Response.Dashboards.RevenueOverview
                {
                    Total = totalRevenue,
                    ThisMonth = revenueThisMonth,
                    LastMonth = revenueLastMonth,
                    TotalTransactions = totalTransactions
                },
                Enrollments = new BusinessObject.DTOs.Response.Dashboards.EnrollmentOverview
                {
                    Total = totalEnrollments,
                    ThisMonth = enrollmentsThisMonth,
                    ActiveStudents = activeStudents,
                    CompletedStudents = completedStudents
                }
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.AdminUserStatisticsResponse> GetAdminUserStatisticsAsync(
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            var now = DateTime.Now;
            var startDate = dateRange?.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                timeUnit = TimeUnitEnum.Month;
            }

            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            // Note: AspNetUsers không có CreatedAt, sử dụng studentenrollment.EnrolledAt như proxy cho students
            // và process.CreatedAt cho teachers (nếu có). Hoặc có thể dùng bảng audit log riêng.
            // Ở đây tôi sẽ track dựa trên hoạt động đầu tiên của user (enrollment hoặc process)
            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(first_activity.ActivityDate) AS Year,
                        MONTH(first_activity.ActivityDate) AS Month,
                        DAY(first_activity.ActivityDate) AS Day,
                        HOUR(first_activity.ActivityDate) AS Hour";
                    groupByClause = "YEAR(first_activity.ActivityDate), MONTH(first_activity.ActivityDate), DAY(first_activity.ActivityDate), HOUR(first_activity.ActivityDate)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(first_activity.ActivityDate) AS Year,
                        MONTH(first_activity.ActivityDate) AS Month,
                        DAY(first_activity.ActivityDate) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(first_activity.ActivityDate), MONTH(first_activity.ActivityDate), DAY(first_activity.ActivityDate)";
                    break;
                default:
                    selectClause = @"
                        YEAR(first_activity.ActivityDate) AS Year,
                        MONTH(first_activity.ActivityDate) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(first_activity.ActivityDate), MONTH(first_activity.ActivityDate)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    COUNT(DISTINCT first_activity.UserId) AS NewAccounts,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Teacher') THEN first_activity.UserId END) AS NewTeachers,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Student') THEN first_activity.UserId END) AS NewStudents
                FROM (
                    SELECT 
                        UserId,
                        MIN(ActivityDate) AS ActivityDate
                    FROM (
                        SELECT 
                            se.StudentID AS UserId,
                            se.EnrolledAt AS ActivityDate
                        FROM studentenrollment se
                        WHERE se.EnrolledAt >= @StartDate AND se.EnrolledAt <= @EndDate
                        UNION ALL
                        SELECT 
                            p.StudentID AS UserId,
                            p.CreatedAt AS ActivityDate
                        FROM process p
                        WHERE p.CreatedAt >= @StartDate AND p.CreatedAt <= @EndDate
                    ) all_activities
                    GROUP BY UserId
                ) first_activity
                INNER JOIN AspNetUsers u ON u.Id = first_activity.UserId
                LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                WHERE first_activity.ActivityDate >= @StartDate AND first_activity.ActivityDate <= @EndDate";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();
            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, int NewAccounts, int NewTeachers, int NewStudents)>();

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var newAccounts = reader.GetInt32("NewAccounts");
                var newTeachers = reader.GetInt32("NewTeachers");
                var newStudents = reader.GetInt32("NewStudents");

                string key = timeUnit switch
                {
                    TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                statsDict[key] = (year, month, day, hour, newAccounts, newTeachers, newStudents);
            }

            await reader.CloseAsync();

            // Get total counts - Raw SQL
            var totalCountsSql = @"
                SELECT 
                    COUNT(DISTINCT u.Id) AS TotalAccounts,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Teacher') THEN u.Id END) AS TotalTeachers,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Student') THEN u.Id END) AS TotalStudents,
                    COUNT(DISTINCT CASE WHEN ur.RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Parent') THEN u.Id END) AS TotalParents
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id";

            var totalCountsCommand = connection.CreateCommand();
            totalCountsCommand.CommandText = totalCountsSql;
            var totalCountsReader = await totalCountsCommand.ExecuteReaderAsync();

            int totalAccounts = 0, totalTeachers = 0, totalStudents = 0, totalParents = 0;
            if (await totalCountsReader.ReadAsync())
            {
                totalAccounts = totalCountsReader.GetInt32("TotalAccounts");
                totalTeachers = totalCountsReader.GetInt32("TotalTeachers");
                totalStudents = totalCountsReader.GetInt32("TotalStudents");
                totalParents = totalCountsReader.GetInt32("TotalParents");
            }
            await totalCountsReader.CloseAsync();
            await connection.CloseAsync();

            var metadata = new BusinessObject.DTOs.Response.Dashboards.AdminUserStatisticsMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalAccounts = totalAccounts,
                TotalTeachers = totalTeachers,
                TotalStudents = totalStudents,
                TotalParents = totalParents
            };

            var chart = new List<BusinessObject.DTOs.Response.Dashboards.AdminUserStatisticsChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Day}/{current.Month} {current.Hour}:00";
                        current = current.AddHours(1);
                        break;
                    case TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        current = current.AddDays(1);
                        break;
                    default:
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        current = current.AddMonths(1);
                        break;
                }

                var stat = statsDict.GetValueOrDefault(key);
                chart.Add(new BusinessObject.DTOs.Response.Dashboards.AdminUserStatisticsChartDataPoint
                {
                    Label = label,
                    NewAccounts = stat.NewAccounts,
                    NewTeachers = stat.NewTeachers,
                    NewStudents = stat.NewStudents
                });
            }

            return new BusinessObject.DTOs.Response.Dashboards.AdminUserStatisticsResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.AdminCourseStatisticsResponse> GetAdminCourseStatisticsAsync(
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            var now = DateTime.Now;
            var startDate = dateRange?.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                timeUnit = TimeUnitEnum.Month;
            }

            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(c.CreatedAt) AS Year,
                        MONTH(c.CreatedAt) AS Month,
                        DAY(c.CreatedAt) AS Day,
                        HOUR(c.CreatedAt) AS Hour";
                    groupByClause = "YEAR(c.CreatedAt), MONTH(c.CreatedAt), DAY(c.CreatedAt), HOUR(c.CreatedAt)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(c.CreatedAt) AS Year,
                        MONTH(c.CreatedAt) AS Month,
                        DAY(c.CreatedAt) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(c.CreatedAt), MONTH(c.CreatedAt), DAY(c.CreatedAt)";
                    break;
                default:
                    selectClause = @"
                        YEAR(c.CreatedAt) AS Year,
                        MONTH(c.CreatedAt) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(c.CreatedAt), MONTH(c.CreatedAt)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    COUNT(DISTINCT c.CourseID) AS NewCourses,
                    COUNT(DISTINCT CASE WHEN c.Status = 'Open' AND c.UpdatedAt >= @StartDate AND c.UpdatedAt <= @EndDate THEN c.CourseID END) AS ApprovedCourses
                FROM course c
                WHERE c.CreatedAt >= @StartDate
                AND c.CreatedAt <= @EndDate";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();
            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, int NewCourses, int ApprovedCourses)>();

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var newCourses = reader.GetInt32("NewCourses");
                var approvedCourses = reader.GetInt32("ApprovedCourses");

                string key = timeUnit switch
                {
                    TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                statsDict[key] = (year, month, day, hour, newCourses, approvedCourses);
            }

            await reader.CloseAsync();

            // Get total counts - Raw SQL
            var totalCoursesSql = @"
                SELECT 
                    Status,
                    COUNT(*) AS Count
                FROM course
                GROUP BY Status";

            var totalCoursesCommand = connection.CreateCommand();
            totalCoursesCommand.CommandText = totalCoursesSql;
            var totalCoursesReader = await totalCoursesCommand.ExecuteReaderAsync();

            var courseDict = new Dictionary<string, int>();
            while (await totalCoursesReader.ReadAsync())
            {
                var status = totalCoursesReader.GetString("Status");
                var count = totalCoursesReader.GetInt32("Count");
                courseDict[status] = count;
            }
            await totalCoursesReader.CloseAsync();

            var newCoursesTotalSql = "SELECT COUNT(*) FROM course WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate";
            var newCoursesTotalCommand = connection.CreateCommand();
            newCoursesTotalCommand.CommandText = newCoursesTotalSql;
            newCoursesTotalCommand.Parameters.Add(new MySqlParameter("@StartDate", startDate));
            newCoursesTotalCommand.Parameters.Add(new MySqlParameter("@EndDate", endDate));
            var newCoursesTotal = Convert.ToInt32(await newCoursesTotalCommand.ExecuteScalarAsync());

            await connection.CloseAsync();

            var metadata = new BusinessObject.DTOs.Response.Dashboards.AdminCourseStatisticsMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalCourses = courseDict.Values.Sum(),
                NewCourses = newCoursesTotal,
                OpenCourses = courseDict.GetValueOrDefault("Open", 0),
                PendingCourses = courseDict.GetValueOrDefault("Pending", 0)
            };

            var chart = new List<BusinessObject.DTOs.Response.Dashboards.AdminCourseStatisticsChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Day}/{current.Month} {current.Hour}:00";
                        current = current.AddHours(1);
                        break;
                    case TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        current = current.AddDays(1);
                        break;
                    default:
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        current = current.AddMonths(1);
                        break;
                }

                var stat = statsDict.GetValueOrDefault(key);
                chart.Add(new BusinessObject.DTOs.Response.Dashboards.AdminCourseStatisticsChartDataPoint
                {
                    Label = label,
                    NewCourses = stat.NewCourses,
                    ApprovedCourses = stat.ApprovedCourses
                });
            }

            return new BusinessObject.DTOs.Response.Dashboards.AdminCourseStatisticsResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.AdminRevenueStatisticsResponse> GetAdminRevenueStatisticsAsync(
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            var now = DateTime.Now;
            var startDate = dateRange?.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                timeUnit = TimeUnitEnum.Month;
            }

            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(th.TransactionDate) AS Year,
                        MONTH(th.TransactionDate) AS Month,
                        DAY(th.TransactionDate) AS Day,
                        HOUR(th.TransactionDate) AS Hour";
                    groupByClause = "YEAR(th.TransactionDate), MONTH(th.TransactionDate), DAY(th.TransactionDate), HOUR(th.TransactionDate)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(th.TransactionDate) AS Year,
                        MONTH(th.TransactionDate) AS Month,
                        DAY(th.TransactionDate) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(th.TransactionDate), MONTH(th.TransactionDate), DAY(th.TransactionDate)";
                    break;
                default:
                    selectClause = @"
                        YEAR(th.TransactionDate) AS Year,
                        MONTH(th.TransactionDate) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(th.TransactionDate), MONTH(th.TransactionDate)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    SUM(th.Amount) AS Revenue,
                    COUNT(DISTINCT th.TransactionID) AS TransactionCount
                FROM transactionhistory th
                WHERE th.TransactionDate IS NOT NULL
                AND th.TransactionDate >= @StartDate
                AND th.TransactionDate <= @EndDate";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();
            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, decimal Revenue, int TransactionCount)>();
            var totalRevenue = 0m;
            var totalTransactions = 0;

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var revenue = reader.GetDecimal("Revenue");
                var transactionCount = reader.GetInt32("TransactionCount");

                string key = timeUnit switch
                {
                    TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                statsDict[key] = (year, month, day, hour, revenue, transactionCount);
                totalRevenue += revenue;
                totalTransactions += transactionCount;
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            var metadata = new BusinessObject.DTOs.Response.Dashboards.AdminRevenueStatisticsMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalRevenue = totalRevenue,
                TotalTransactions = totalTransactions,
                AverageTransactionAmount = totalTransactions > 0 ? totalRevenue / totalTransactions : 0
            };

            var chart = new List<BusinessObject.DTOs.Response.Dashboards.AdminRevenueStatisticsChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Day}/{current.Month} {current.Hour}:00";
                        current = current.AddHours(1);
                        break;
                    case TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        current = current.AddDays(1);
                        break;
                    default:
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        current = current.AddMonths(1);
                        break;
                }

                var stat = statsDict.GetValueOrDefault(key);
                chart.Add(new BusinessObject.DTOs.Response.Dashboards.AdminRevenueStatisticsChartDataPoint
                {
                    Label = label,
                    Revenue = stat.Revenue,
                    TransactionCount = stat.TransactionCount
                });
            }

            return new BusinessObject.DTOs.Response.Dashboards.AdminRevenueStatisticsResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.AdminEnrollmentStatisticsResponse> GetAdminEnrollmentStatisticsAsync(
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            var now = DateTime.Now;
            var startDate = dateRange?.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                timeUnit = TimeUnitEnum.Month;
            }

            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(se.EnrolledAt) AS Year,
                        MONTH(se.EnrolledAt) AS Month,
                        DAY(se.EnrolledAt) AS Day,
                        HOUR(se.EnrolledAt) AS Hour";
                    groupByClause = "YEAR(se.EnrolledAt), MONTH(se.EnrolledAt), DAY(se.EnrolledAt), HOUR(se.EnrolledAt)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(se.EnrolledAt) AS Year,
                        MONTH(se.EnrolledAt) AS Month,
                        DAY(se.EnrolledAt) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(se.EnrolledAt), MONTH(se.EnrolledAt), DAY(se.EnrolledAt)";
                    break;
                default:
                    selectClause = @"
                        YEAR(se.EnrolledAt) AS Year,
                        MONTH(se.EnrolledAt) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(se.EnrolledAt), MONTH(se.EnrolledAt)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    COUNT(DISTINCT se.EnrollmentID) AS NewEnrollments,
                    COUNT(DISTINCT CASE WHEN completion.CompletionPercentage >= 100 THEN se.EnrollmentID END) AS CompletedEnrollments
                FROM studentenrollment se
                LEFT JOIN (
                    SELECT 
                        p.StudentID,
                        p.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p
                    INNER JOIN course c ON p.CourseID = c.CourseID
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p.StudentID, p.CourseID
                ) completion ON completion.StudentID = se.StudentID AND completion.CourseID = se.CourseID
                WHERE se.EnrolledAt >= @StartDate
                AND se.EnrolledAt <= @EndDate";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();
            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, int NewEnrollments, int CompletedEnrollments)>();
            var totalEnrollments = 0;
            var activeStudents = 0;
            var completedStudents = 0;

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var newEnrollments = reader.GetInt32("NewEnrollments");
                var completedEnrollments = reader.GetInt32("CompletedEnrollments");

                string key = timeUnit switch
                {
                    TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                statsDict[key] = (year, month, day, hour, newEnrollments, completedEnrollments);
                totalEnrollments += newEnrollments;
            }

            await reader.CloseAsync();

            // Get active and completed students - Raw SQL
            var activeStudentsSql = "SELECT COUNT(DISTINCT StudentID) FROM process";
            var activeStudentsCommand = connection.CreateCommand();
            activeStudentsCommand.CommandText = activeStudentsSql;
            activeStudents = Convert.ToInt32(await activeStudentsCommand.ExecuteScalarAsync());

            var completedStudentsSql = @"
                SELECT COUNT(DISTINCT p.StudentID)
                FROM process p
                INNER JOIN (
                    SELECT 
                        p2.StudentID,
                        p2.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p2
                    INNER JOIN course c ON p2.CourseID = c.CourseID
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p2.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p2.StudentID, p2.CourseID
                    HAVING CompletionPercentage >= 100
                ) completed ON completed.StudentID = p.StudentID AND completed.CourseID = p.CourseID";

            var completedStudentsCommand = connection.CreateCommand();
            completedStudentsCommand.CommandText = completedStudentsSql;
            completedStudents = Convert.ToInt32(await completedStudentsCommand.ExecuteScalarAsync());

            await connection.CloseAsync();

            var metadata = new BusinessObject.DTOs.Response.Dashboards.AdminEnrollmentStatisticsMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalEnrollments = totalEnrollments,
                ActiveStudents = activeStudents,
                CompletedStudents = completedStudents
            };

            var chart = new List<BusinessObject.DTOs.Response.Dashboards.AdminEnrollmentStatisticsChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Day}/{current.Month} {current.Hour}:00";
                        current = current.AddHours(1);
                        break;
                    case TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        current = current.AddDays(1);
                        break;
                    default:
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        current = current.AddMonths(1);
                        break;
                }

                var stat = statsDict.GetValueOrDefault(key);
                chart.Add(new BusinessObject.DTOs.Response.Dashboards.AdminEnrollmentStatisticsChartDataPoint
                {
                    Label = label,
                    NewEnrollments = stat.NewEnrollments,
                    CompletedEnrollments = stat.CompletedEnrollments
                });
            }

            return new BusinessObject.DTOs.Response.Dashboards.AdminEnrollmentStatisticsResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.ParentChildLearningProgressResponse> GetParentChildLearningProgressAsync(
            string parentId,
            BusinessObject.Payload.Request.Filter.AnalyticsDateRangeRequest? dateRange = null)
        {
            // Set default dates: đầu năm và cuối năm hiện tại
            var now = DateTime.Now;
            var startDate = dateRange?.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = dateRange?.EndDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);

            // Tự động xác định time unit dựa trên khoảng thời gian
            var timeSpan = endDate - startDate;
            TimeUnitEnum timeUnit;

            if (timeSpan.TotalHours <= 24)
            {
                timeUnit = TimeUnitEnum.Hour;
            }
            else if (timeSpan.TotalDays <= 31)
            {
                timeUnit = TimeUnitEnum.Day;
            }
            else
            {
                timeUnit = TimeUnitEnum.Month;
            }

            // Build SQL query với time unit tương ứng
            var sqlParams = new List<MySqlParameter>();
            string groupByClause;
            string selectClause;

            switch (timeUnit)
            {
                case TimeUnitEnum.Hour:
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        DAY(pi.CreatedAt) AS Day,
                        HOUR(pi.CreatedAt) AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt), DAY(pi.CreatedAt), HOUR(pi.CreatedAt)";
                    break;
                case TimeUnitEnum.Day:
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        DAY(pi.CreatedAt) AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt), DAY(pi.CreatedAt)";
                    break;
                default: // Month
                    selectClause = @"
                        YEAR(pi.CreatedAt) AS Year,
                        MONTH(pi.CreatedAt) AS Month,
                        NULL AS Day,
                        NULL AS Hour";
                    groupByClause = "YEAR(pi.CreatedAt), MONTH(pi.CreatedAt)";
                    break;
            }

            var sql = $@"
                SELECT 
                    {selectClause},
                    COUNT(DISTINCT pi.ProcessItemID) AS CompletedLessonItems,
                    ROUND(COUNT(DISTINCT pi.ProcessItemID) * 0.5, 2) AS StudyHours,
                    COUNT(DISTINCT p.StudentID) AS ActiveChildren
                FROM processitem pi
                INNER JOIN process p ON pi.ProcessID = p.ProcessID
                INNER JOIN parentstudentlink psl ON psl.StudentID = p.StudentID
                WHERE pi.CreatedAt IS NOT NULL
                AND pi.CreatedAt >= @StartDate
                AND pi.CreatedAt <= @EndDate
                AND psl.ParentID = @ParentId";

            sqlParams.Add(new MySqlParameter("@StartDate", startDate));
            sqlParams.Add(new MySqlParameter("@EndDate", endDate));
            sqlParams.Add(new MySqlParameter("@ParentId", parentId));

            sql += $@"
                GROUP BY {groupByClause}
                ORDER BY Year, Month, Day, Hour";

            // Execute raw SQL query
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var statsDict = new Dictionary<string, (int Year, int Month, int? Day, int? Hour, int CompletedLessonItems, decimal StudyHours, int ActiveChildren)>();
            var defaultStat = (Year: 0, Month: 0, Day: (int?)null, Hour: (int?)null, CompletedLessonItems: 0, StudyHours: 0m, ActiveChildren: 0);
            var totalCompletedLessonItems = 0;
            var totalStudyHours = 0m;

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var month = reader.GetInt32("Month");
                var day = reader.IsDBNull("Day") ? (int?)null : reader.GetInt32("Day");
                var hour = reader.IsDBNull("Hour") ? (int?)null : reader.GetInt32("Hour");
                var completedLessonItems = reader.GetInt32("CompletedLessonItems");
                var studyHours = reader.IsDBNull("StudyHours") ? 0m : reader.GetDecimal("StudyHours");
                var activeChildren = reader.GetInt32("ActiveChildren");

                string key = timeUnit switch
                {
                    TimeUnitEnum.Hour => $"{year}-{month:D2}-{day:D2}-{hour:D2}",
                    TimeUnitEnum.Day => $"{year}-{month:D2}-{day:D2}",
                    _ => $"{year}-{month:D2}"
                };

                statsDict[key] = (year, month, day, hour, completedLessonItems, studyHours, activeChildren);
                totalCompletedLessonItems += completedLessonItems;
                totalStudyHours += studyHours;
            }

            await reader.CloseAsync();

            // Get total children count
            var totalChildrenSql = @"
                SELECT COUNT(DISTINCT StudentID)
                FROM parentstudentlink
                WHERE ParentID = @ParentId";

            var totalChildrenCommand = connection.CreateCommand();
            totalChildrenCommand.CommandText = totalChildrenSql;
            totalChildrenCommand.Parameters.Add(new MySqlParameter("@ParentId", parentId));
            var totalChildren = Convert.ToInt32(await totalChildrenCommand.ExecuteScalarAsync());

            await connection.CloseAsync();

            // Build metadata
            var metadata = new BusinessObject.DTOs.Response.Dashboards.ParentChildLearningProgressMetadata
            {
                TimeUnit = timeUnit.ToString(),
                TotalChildren = totalChildren,
                TotalCompletedLessonItems = totalCompletedLessonItems,
                TotalStudyHours = totalStudyHours
            };

            // Build chart data - fill đầy đủ các time slots
            var chart = new List<BusinessObject.DTOs.Response.Dashboards.ParentChildLearningProgressChartDataPoint>();
            var current = startDate;

            while (current <= endDate)
            {
                string key;
                string label;

                switch (timeUnit)
                {
                    case TimeUnitEnum.Hour:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}-{current.Hour:D2}";
                        label = $"{current.Day}/{current.Month} {current.Hour}:00";
                        current = current.AddHours(1);
                        break;

                    case TimeUnitEnum.Day:
                        key = $"{current.Year}-{current.Month:D2}-{current.Day:D2}";
                        label = $"{current.Day}/{current.Month}";
                        current = current.AddDays(1);
                        break;

                    default: // Month
                        key = $"{current.Year}-{current.Month:D2}";
                        label = $"T{current.Month}";
                        current = current.AddMonths(1);
                        break;
                }

                var stat = statsDict.GetValueOrDefault(key, defaultStat);
                chart.Add(new BusinessObject.DTOs.Response.Dashboards.ParentChildLearningProgressChartDataPoint
                {
                    Label = label,
                    CompletedLessonItems = stat.CompletedLessonItems,
                    StudyHours = stat.StudyHours,
                    ActiveChildren = stat.ActiveChildren
                });
            }

            return new BusinessObject.DTOs.Response.Dashboards.ParentChildLearningProgressResponse
            {
                Metadata = metadata,
                Chart = chart
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.ParentChildCourseStatisticsResponse> GetParentChildCourseStatisticsAsync(string parentId)
        {
            var sql = @"
                SELECT 
                    u.Id AS ChildId,
                    u.UserName AS ChildName,
                    COUNT(DISTINCT se.CourseID) AS EnrolledCourses,
                    COUNT(DISTINCT CASE 
                        WHEN completion.CompletionPercentage >= 100 THEN se.CourseID 
                    END) AS CompletedCourses,
                    COUNT(DISTINCT CASE 
                        WHEN completion.CompletionPercentage > 0 AND completion.CompletionPercentage < 100 THEN se.CourseID 
                    END) AS InProgressCourses,
                    CASE 
                        WHEN COUNT(DISTINCT se.CourseID) = 0 THEN 0
                        ELSE ROUND(AVG(completion.CompletionPercentage), 2)
                    END AS AverageCompletionPercentage
                FROM parentstudentlink psl
                INNER JOIN AspNetUsers u ON u.Id = psl.StudentID
                LEFT JOIN studentenrollment se ON se.StudentID = psl.StudentID
                LEFT JOIN (
                    SELECT 
                        p.StudentID,
                        p.CourseID,
                        CASE 
                            WHEN COUNT(DISTINCT li.LessonItemID) = 0 THEN 0
                            ELSE ROUND(COUNT(DISTINCT pi.ProcessItemID) * 100.0 / COUNT(DISTINCT li.LessonItemID), 2)
                        END AS CompletionPercentage
                    FROM process p
                    INNER JOIN course c ON p.CourseID = c.CourseID
                    LEFT JOIN coursesection cs ON cs.CourseID = c.CourseID
                    LEFT JOIN lesson l ON l.CourseSectionID = cs.CourseSectionID
                    LEFT JOIN lessonitem li ON li.LessonID = l.LessonID
                    LEFT JOIN processitem pi ON pi.ProcessID = p.ProcessID AND pi.LessonItemID = li.LessonItemID
                    GROUP BY p.StudentID, p.CourseID
                ) completion ON completion.StudentID = se.StudentID AND completion.CourseID = se.CourseID
                WHERE psl.ParentID = @ParentId
                GROUP BY u.Id, u.UserName
                ORDER BY AverageCompletionPercentage DESC, u.UserName";

            var sqlParams = new List<MySqlParameter>
            {
                new MySqlParameter("@ParentId", parentId)
            };

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var children = new List<BusinessObject.DTOs.Response.Dashboards.ParentChildCourseStatisticsChartDataPoint>();
            var totalChildren = 0;
            var totalEnrolledCourses = 0;
            var totalCompletedCourses = 0;
            var totalInProgressCourses = 0;
            var totalCompletionPercentage = 0m;

            while (await reader.ReadAsync())
            {
                var childId = reader.GetString("ChildId");
                var childName = reader.GetString("ChildName");
                var enrolledCourses = reader.GetInt32("EnrolledCourses");
                var completedCourses = reader.GetInt32("CompletedCourses");
                var inProgressCourses = reader.GetInt32("InProgressCourses");
                var averageCompletionPercentage = reader.IsDBNull("AverageCompletionPercentage") ? 0m : reader.GetDecimal("AverageCompletionPercentage");

                totalChildren++;
                totalEnrolledCourses += enrolledCourses;
                totalCompletedCourses += completedCourses;
                totalInProgressCourses += inProgressCourses;
                totalCompletionPercentage += averageCompletionPercentage;

                children.Add(new BusinessObject.DTOs.Response.Dashboards.ParentChildCourseStatisticsChartDataPoint
                {
                    Label = childName,
                    ChildId = childId,
                    ChildName = childName,
                    EnrolledCourses = enrolledCourses,
                    CompletedCourses = completedCourses,
                    InProgressCourses = inProgressCourses,
                    AverageCompletionPercentage = averageCompletionPercentage
                });
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            // Build metadata
            var metadata = new BusinessObject.DTOs.Response.Dashboards.ParentChildCourseStatisticsMetadata
            {
                TotalChildren = totalChildren,
                TotalEnrolledCourses = totalEnrolledCourses,
                CompletedCourses = totalCompletedCourses,
                InProgressCourses = totalInProgressCourses,
                AverageCompletionPercentage = totalChildren > 0 ? totalCompletionPercentage / totalChildren : 0
            };

            return new BusinessObject.DTOs.Response.Dashboards.ParentChildCourseStatisticsResponse
            {
                Metadata = metadata,
                Chart = children
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.ParentChildPerformanceResponse> GetParentChildPerformanceAsync(string parentId)
        {
            var sql = @"
                SELECT 
                    u.Id AS ChildId,
                    u.UserName AS ChildName,
                    COUNT(DISTINCT COALESCE(fqr.FinalQuizResultID, qr.QuizResultID)) AS QuizzesTaken,
                    COUNT(DISTINCT CASE 
                        WHEN COALESCE(fqr.Score, qr.Score) >= 50 THEN COALESCE(fqr.FinalQuizResultID, qr.QuizResultID)
                    END) AS PassedQuizzes,
                    CASE 
                        WHEN COUNT(DISTINCT COALESCE(fqr.FinalQuizResultID, qr.QuizResultID)) = 0 THEN 0
                        ELSE ROUND(AVG(COALESCE(fqr.Score, qr.Score)), 2)
                    END AS AverageScore,
                    CASE 
                        WHEN COUNT(DISTINCT COALESCE(fqr.FinalQuizResultID, qr.QuizResultID)) = 0 THEN 0
                        ELSE ROUND(COUNT(DISTINCT CASE 
                            WHEN COALESCE(fqr.Score, qr.Score) >= 50 THEN COALESCE(fqr.FinalQuizResultID, qr.QuizResultID)
                        END) * 100.0 / COUNT(DISTINCT COALESCE(fqr.FinalQuizResultID, qr.QuizResultID)), 2)
                    END AS PassRate
                FROM parentstudentlink psl
                INNER JOIN AspNetUsers u ON u.Id = psl.StudentID
                LEFT JOIN finalquizresult fqr ON fqr.UserID = psl.StudentID
                LEFT JOIN quizresult qr ON qr.UserID = psl.StudentID
                WHERE psl.ParentID = @ParentId
                GROUP BY u.Id, u.UserName
                ORDER BY AverageScore DESC, u.UserName";

            var sqlParams = new List<MySqlParameter>
            {
                new MySqlParameter("@ParentId", parentId)
            };

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var children = new List<BusinessObject.DTOs.Response.Dashboards.ParentChildPerformanceChartDataPoint>();
            var totalChildren = 0;
            var totalQuizzesTaken = 0;
            var totalPassedQuizzes = 0;
            var totalAverageScore = 0m;

            while (await reader.ReadAsync())
            {
                var childId = reader.GetString("ChildId");
                var childName = reader.GetString("ChildName");
                var quizzesTaken = reader.GetInt32("QuizzesTaken");
                var passedQuizzes = reader.GetInt32("PassedQuizzes");
                var averageScore = reader.IsDBNull("AverageScore") ? 0m : reader.GetDecimal("AverageScore");
                var passRate = reader.IsDBNull("PassRate") ? 0m : reader.GetDecimal("PassRate");

                totalChildren++;
                totalQuizzesTaken += quizzesTaken;
                totalPassedQuizzes += passedQuizzes;
                totalAverageScore += averageScore;

                children.Add(new BusinessObject.DTOs.Response.Dashboards.ParentChildPerformanceChartDataPoint
                {
                    Label = childName,
                    ChildId = childId,
                    ChildName = childName,
                    AverageScore = averageScore,
                    QuizzesTaken = quizzesTaken,
                    PassedQuizzes = passedQuizzes,
                    PassRate = passRate
                });
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            // Build metadata
            var metadata = new BusinessObject.DTOs.Response.Dashboards.ParentChildPerformanceMetadata
            {
                TotalChildren = totalChildren,
                AverageScore = totalChildren > 0 ? totalAverageScore / totalChildren : 0,
                TotalQuizzesTaken = totalQuizzesTaken,
                PassedQuizzes = totalPassedQuizzes,
                QuizPassRate = totalQuizzesTaken > 0 ? (decimal)totalPassedQuizzes * 100 / totalQuizzesTaken : 0
            };

            return new BusinessObject.DTOs.Response.Dashboards.ParentChildPerformanceResponse
            {
                Metadata = metadata,
                Chart = children
            };
        }

        public async Task<BusinessObject.DTOs.Response.Dashboards.ParentChildCertificateResponse> GetParentChildCertificatesAsync(string parentId)
        {
            var sql = @"
                SELECT 
                    cc.CourseCertificateID AS CertificateId,
                    cc.CourseID AS CourseId,
                    c.Name AS CourseName,
                    cc.UserID AS ChildId,
                    u.UserName AS ChildName,
                    cc.ImageUrl AS ImageUrl,
                    cc.CreatedAt AS CreatedAt
                FROM parentstudentlink psl
                INNER JOIN AspNetUsers u ON u.Id = psl.StudentID
                INNER JOIN coursecertificate cc ON cc.UserID = psl.StudentID
                INNER JOIN course c ON c.CourseID = cc.CourseID
                WHERE psl.ParentID = @ParentId
                ORDER BY cc.CreatedAt DESC";

            var sqlParams = new List<MySqlParameter>
            {
                new MySqlParameter("@ParentId", parentId)
            };

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParams.ToArray());

            var reader = await command.ExecuteReaderAsync();

            var allCertificates = new List<BusinessObject.DTOs.Response.Dashboards.ChildCertificateItem>();
            var childrenDict = new Dictionary<string, BusinessObject.DTOs.Response.Dashboards.ChildCertificateStatistic>();

            while (await reader.ReadAsync())
            {
                var certificateId = reader.GetInt32("CertificateId");
                var courseId = reader.GetInt32("CourseId");
                var courseName = reader.GetString("CourseName");
                var childId = reader.GetString("ChildId");
                var childName = reader.GetString("ChildName");
                var imageUrl = reader.IsDBNull("ImageUrl") ? string.Empty : reader.GetString("ImageUrl");
                var createdAt = reader.GetDateTime("CreatedAt");

                var certificate = new BusinessObject.DTOs.Response.Dashboards.ChildCertificateItem
                {
                    CertificateId = certificateId,
                    CourseId = courseId,
                    CourseName = courseName,
                    ChildId = childId,
                    ChildName = childName,
                    ImageUrl = imageUrl,
                    CreatedAt = createdAt
                };

                allCertificates.Add(certificate);

                if (!childrenDict.ContainsKey(childId))
                {
                    childrenDict[childId] = new BusinessObject.DTOs.Response.Dashboards.ChildCertificateStatistic
                    {
                        ChildId = childId,
                        ChildName = childName,
                        CertificateCount = 0,
                        Certificates = new List<BusinessObject.DTOs.Response.Dashboards.ChildCertificateItem>()
                    };
                }

                childrenDict[childId].CertificateCount++;
                childrenDict[childId].Certificates.Add(certificate);
            }

            await reader.CloseAsync();
            await connection.CloseAsync();

            return new BusinessObject.DTOs.Response.Dashboards.ParentChildCertificateResponse
            {
                TotalCertificates = allCertificates.Count,
                Certificates = allCertificates,
                ChildrenStatistics = childrenDict.Values.OrderByDescending(c => c.CertificateCount).ToList()
            };
        }
    }
}
