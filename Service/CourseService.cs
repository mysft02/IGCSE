using Repository.IRepositories;
using AutoMapper;
using Common.Constants;
using BusinessObject.Model;
using Service.OpenAI;
using Common.Utils;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.CourseContent;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Request.CourseContent;
using BusinessObject.Payload.Response.Trello;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.ParentStudentLink;
using Microsoft.AspNetCore.Identity;

namespace Service
{
    public class CourseService
    {
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ICoursesectionRepository _coursesectionRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonitemRepository _lessonitemRepository;
        private readonly OpenAIEmbeddingsApiService _openAIEmbeddingsApiService;
        private readonly IModuleRepository _moduleRepository;
        private readonly IProcessRepository _processRepository;
        private readonly IProcessitemRepository _processitemRepository;
        private readonly IParentStudentLinkRepository _parentStudentLinkRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<Account> _userManager;
        // private readonly IChapterRepository _chapterRepository;

        public CourseService(
            IMapper mapper,
            ICourseRepository courseRepository,
            ICoursesectionRepository coursesectionRepository,
            ILessonRepository lessonRepository,
            ILessonitemRepository lessonitemRepository,
            OpenAIEmbeddingsApiService openAIEmbeddingsApiService,
            IModuleRepository moduleRepository,
            IProcessRepository processRepository,
            IProcessitemRepository processitemRepository,
            IParentStudentLinkRepository parentStudentLinkRepository,
            IAccountRepository accountRepository,
            UserManager<Account> userManager)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
            _coursesectionRepository = coursesectionRepository;
            _lessonRepository = lessonRepository;
            _lessonitemRepository = lessonitemRepository;
            _openAIEmbeddingsApiService = openAIEmbeddingsApiService;
            _moduleRepository = moduleRepository;
            _processRepository = processRepository;
            _processitemRepository = processitemRepository;
            _parentStudentLinkRepository = parentStudentLinkRepository;
            _accountRepository = accountRepository;
            _userManager = userManager;
            // _chapterRepository = chapterRepository; // Chapter disabled
        }

        public async Task<BaseResponse<CourseResponse>> CreateCourseAsync(CourseRequest request, string createdBy = null)
        {
            // Create new course
            var course = new Course
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                ModuleId = request.ModuleId,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            var embeddingData = await _openAIEmbeddingsApiService.EmbedData(course);
            course.EmbeddingData = CommonUtils.ObjectToString(embeddingData);

            var createdCourse = await _courseRepository.AddAsync(course);

            var courseResponse = _mapper.Map<CourseResponse>(createdCourse);

            return new BaseResponse<CourseResponse>(
                "Course created successfully",
                StatusCodeEnum.Created_201,
                courseResponse
            );
        }

        public async Task<BaseResponse<CourseResponse>> ApproveCourseAsync(long courseId)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            course.Status = "Approved";
            course.UpdatedAt = DateTime.UtcNow;

            var updated = await _courseRepository.UpdateAsync(course);
            var response = _mapper.Map<CourseResponse>(updated);
            return new BaseResponse<CourseResponse>(
                "Course approved successfully",
                StatusCodeEnum.OK_200,
                response
            );
        }

        public async Task<BaseResponse<CourseResponse>> RejectCourseAsync(long courseId, string? reason)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            course.Status = "Rejected";
            course.UpdatedAt = DateTime.UtcNow;

            var updated = await _courseRepository.UpdateAsync(course);
            var response = _mapper.Map<CourseResponse>(updated);
            return new BaseResponse<CourseResponse>(
                string.IsNullOrWhiteSpace(reason) ? "Course rejected" : $"Course rejected: {reason}",
                StatusCodeEnum.OK_200,
                response
            );
        }

        public async Task<BaseResponse<PaginatedResponse<CourseResponse>>> GetPendingCoursesPagedAsync(int page, int pageSize, string? searchByName)
        {
            var (items, total) = await _courseRepository.SearchAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 10 : pageSize, searchByName, null, "Pending");
            var courseResponses = items.Select(i => _mapper.Map<CourseResponse>(i)).ToList();
            var totalPages = (int)Math.Ceiling(total / (double)(pageSize <= 0 ? 10 : pageSize));

            var paginated = new PaginatedResponse<CourseResponse>
            {
                Items = courseResponses,
                TotalCount = total,
                Page = page - 1,
                Size = pageSize <= 0 ? 10 : pageSize,
                TotalPages = totalPages
            };

            return new BaseResponse<PaginatedResponse<CourseResponse>>(
                "Pending courses retrieved successfully",
                StatusCodeEnum.OK_200,
                paginated
            );
        }

        public async Task<BaseResponse<CourseResponse>> UpdateCourseAsync(long courseId, CourseRequest request)
        {
            // Get existing course
            var existingCourse = await _courseRepository.GetByCourseIdAsync(courseId);
            if (existingCourse == null)
            {
                throw new Exception("Course not found");
            }

            if (request.ModuleId != existingCourse.ModuleId)
            {
                var module = await _moduleRepository.GetByIdAsync(request.ModuleId);
                if (module == null)
                {
                    throw new Exception("Module not found or inactive");
                }
            }

            // Update course properties
            existingCourse.Name = request.Name;
            existingCourse.Description = request.Description;
            existingCourse.Status = request.Status;
            existingCourse.Price = request.Price;
            existingCourse.ImageUrl = request.ImageUrl;
            existingCourse.ModuleId = request.ModuleId;
            existingCourse.UpdatedAt = DateTime.UtcNow;

            var updatedCourse = await _courseRepository.UpdateAsync(existingCourse);

            var courseResponse = _mapper.Map<CourseResponse>(updatedCourse);

            return new BaseResponse<CourseResponse>(
                "Course updated successfully",
                StatusCodeEnum.OK_200,
                courseResponse
            );
        }

        public async Task<BaseResponse<CourseResponse>> GetCourseByIdAsync(long courseId)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var courseResponse = _mapper.Map<CourseResponse>(course);

            return new BaseResponse<CourseResponse>(
                "Course retrieved successfully",
                StatusCodeEnum.OK_200,
                courseResponse
            );
        }

        public async Task<BaseResponse<IEnumerable<CourseResponse>>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllAsync();

            var courseResponses = new List<CourseResponse>();
            foreach (var course in courses)
            {
                var courseResponse = _mapper.Map<CourseResponse>(course);
                courseResponses.Add(courseResponse);
            }

            return new BaseResponse<IEnumerable<CourseResponse>>(
                "Courses retrieved successfully",
                StatusCodeEnum.OK_200,
                courseResponses
            );
        }

        public async Task<BaseResponse<PaginatedResponse<CourseResponse>>> GetCoursesPagedAsync(CourseListQuery query)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var (items, total) = await _courseRepository.SearchAsync(page, pageSize, query.SearchByName, query.CouseId, query.Status);
            var courseResponses = items.Select(i => _mapper.Map<CourseResponse>(i)).ToList();

            var paginated = new PaginatedResponse<CourseResponse>
            {
                Items = courseResponses,
                TotalCount = total,
                Page = page - 1,
                Size = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return new BaseResponse<PaginatedResponse<CourseResponse>>(
                "Courses retrieved successfully",
                StatusCodeEnum.OK_200,
                paginated
            );
        }

        // Course Content Management Methods

        public async Task<BaseResponse<CourseSectionResponse>> CreateCourseSectionAsync(CourseSectionRequest request)
        {
            var courseSection = _mapper.Map<Coursesection>(request);
            courseSection.CreatedAt = DateTime.UtcNow;
            courseSection.UpdatedAt = DateTime.UtcNow;

            var createdSection = await _coursesectionRepository.AddAsync(courseSection);
            var response = _mapper.Map<CourseSectionResponse>(createdSection);

            return new BaseResponse<CourseSectionResponse>(
                "Course section created successfully",
                StatusCodeEnum.Created_201,
                response
            );
        }

        public async Task<BaseResponse<LessonResponse>> CreateLessonAsync(LessonRequest request)
        {
            var lesson = new Lesson
            {
                CourseSectionId = request.CourseSectionId,
                Name = request.Name,
                Description = request.Description,
                Order = request.Order,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdLesson = await _lessonRepository.AddAsync(lesson);
            var response = _mapper.Map<LessonResponse>(createdLesson);

            return new BaseResponse<LessonResponse>(
                "Lesson created successfully",
                StatusCodeEnum.Created_201,
                response
            );
        }

        public async Task<BaseResponse<LessonItemResponse>> CreateLessonItemAsync(LessonItemRequest request)
        {
            var lessonItem = new Lessonitem
            {
                LessonId = request.LessonId,
                Name = request.Name,
                Description = request.Description,
                Content = request.Content,
                ItemType = request.ItemType,
                Order = request.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdLessonItem = await _lessonitemRepository.AddAsync(lessonItem);
            var response = _mapper.Map<LessonItemResponse>(createdLessonItem);

            return new BaseResponse<LessonItemResponse>(
                "Lesson item created successfully",
                StatusCodeEnum.Created_201,
                response
            );
        }

        public async Task<BaseResponse<IEnumerable<CourseSectionResponse>>> GetCourseSectionsAsync(long courseId)
        {
            var sections = await _coursesectionRepository.GetByCourseIdAsync(courseId);
            var responses = new List<CourseSectionResponse>();

            foreach (var section in sections)
            {
                var response = _mapper.Map<CourseSectionResponse>(section);
                response.Lessons = new List<LessonResponse>();

                var lessons = await _lessonRepository.GetActiveLessonsBySectionAsync(section.CourseSectionId);
                foreach (var lesson in lessons)
                {
                    response.Lessons.Add(_mapper.Map<LessonResponse>(lesson));
                }

                responses.Add(response);
            }

            return new BaseResponse<IEnumerable<CourseSectionResponse>>(
                "Course sections retrieved successfully",
                StatusCodeEnum.OK_200,
                responses
            );
        }

        public async Task<BaseResponse<IEnumerable<LessonItemResponse>>> GetLessonItemsAsync(long lessonId)
        {
            var lessonItems = await _lessonitemRepository.GetByLessonIdAsync(lessonId);
            var responses = new List<LessonItemResponse>();

            foreach (var item in lessonItems)
            {
                responses.Add(_mapper.Map<LessonItemResponse>(item));
            }

            return new BaseResponse<IEnumerable<LessonItemResponse>>(
                "Lesson items retrieved successfully",
                StatusCodeEnum.OK_200,
                responses
            );
        }

        public async Task<BaseResponse<IEnumerable<CourseResponse>>> GetAllSimilarCoursesAsync(long courseId, decimal score)
        {
            var similarCourses = await _courseRepository.GetAllSimilarCoursesAsync(courseId, score);

            var courseResponses = new List<CourseResponse>();
            foreach (var course in similarCourses)
            {
                var courseResponse = _mapper.Map<CourseResponse>(course);
                courseResponses.Add(courseResponse);
            }

            return new BaseResponse<IEnumerable<CourseResponse>>(
                "Courses retrieved successfully",
                StatusCodeEnum.OK_200,
                courseResponses
            );
        }

        public async Task<BaseResponse<IEnumerable<CourseResponse>>> GetTeacherCoursesAsync(string teacherAccountId)
        {
            var courses = await _courseRepository.GetCoursesByCreatorAsync(teacherAccountId);
            var responses = courses.Select(_mapper.Map<CourseResponse>).ToList();
            return new BaseResponse<IEnumerable<CourseResponse>>(
                "Teacher courses retrieved successfully",
                StatusCodeEnum.OK_200,
                responses
            );
        }

        public async Task<BaseResponse<PaginatedResponse<CourseDashboardQueryResponse>>> GetCourseAnalyticsAsync(CourseDashboardQueryRequest request)
        {
            var filter = request.BuildFilter<Course>();

            // Get total count first (for pagination info)
            var totalCount = await _courseRepository.CountAsync(filter);

            // Get filtered items
            var items = await _courseRepository.GetCourseAnalyticsAsync(request, filter);

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items);

            var pagedItems = sortedItems
                .Skip(request.Page * request.GetPageSize())
                .Take(request.GetPageSize())
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<CourseDashboardQueryResponse>>
            {
                Data = new PaginatedResponse<CourseDashboardQueryResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Lấy thống kê khoá học thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }
        public async Task<BaseResponse<CourseDetailResponse>> GetCourseDetailAsync(int courseId, string? studentId = null)
        {
            try
            {
                // 1. Get course with related data
                var course = await _courseRepository.GetByCourseIdWithCategoryAsync(courseId);
                if (course == null) 
                    throw new Exception("Course not found");

                // 2. Map course to DTO
                var courseDetailResponse = _mapper.Map<CourseDetailResponse>(course);

                // 3. Lấy tiến trình học của student nếu có studentId
                List<Process>? studentProcesses = null;
                Dictionary<int, List<Processitem>>? processItemsDict = null;

                if (!string.IsNullOrEmpty(studentId))
                {
                    studentProcesses = (await _processRepository.GetByStudentAndCourseAsync(studentId, courseId)).ToList();
                    
                    // Lấy tất cả process items cho student
                    processItemsDict = new Dictionary<int, List<Processitem>>();
                    foreach (var process in studentProcesses)
                    {
                        var items = (await _processitemRepository.GetByProcessIdAsync(process.ProcessId)).ToList();
                        processItemsDict[process.ProcessId] = items;
                    }
                }

                var sections = await _coursesectionRepository.GetByCourseIdAsync(courseId);
                
                // Biến để tính overall progress
                int totalLessons = 0;
                int completedLessons = 0;
                
                if (sections != null && sections.Any())
                {
                    courseDetailResponse.Sections = _mapper.Map<List<CourseSectionDetailResponse>>(sections);

                    // For each section, load lessons and lesson items
                    foreach (var section in courseDetailResponse.Sections)
                    {
                        var lessons = await _lessonRepository.GetActiveLessonsBySectionAsync((int)section.CourseSectionId);
                        if (lessons != null && lessons.Any())
                        {
                            section.Lessons = _mapper.Map<List<LessonDetailResponse>>(lessons);
                            
                            foreach (var lesson in section.Lessons)
                            {
                                // Lấy lesson items
                                var lessonItems = await _lessonitemRepository.GetByLessonIdAsync((int)lesson.LessonId);
                                lesson.LessonItems = _mapper.Map<List<LessonItemDetailResponse>>(lessonItems ?? new List<Lessonitem>());

                                // Đếm tổng số lessons
                                totalLessons++;

                                // Nếu có studentId, cập nhật thông tin tiến trình
                                if (studentProcesses != null && processItemsDict != null)
                                {
                                    var process = studentProcesses.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                                    if (process != null)
                                    {
                                        // Cập nhật trạng thái mở khóa của lesson
                                        lesson.IsUnlocked = process.IsUnlocked;

                                        // Kiểm tra lesson đã hoàn thành chưa
                                        var completedItems = processItemsDict.GetValueOrDefault(process.ProcessId, new List<Processitem>());
                                        lesson.IsCompleted = lessonItems.Count() > 0 && completedItems.Count == lessonItems.Count();
                                        
                                        // Đếm số lessons đã hoàn thành
                                        if (lesson.IsCompleted)
                                        {
                                            completedLessons++;
                                        }

                                        // Cập nhật trạng thái hoàn thành của từng lesson item
                                        foreach (var lessonItemResponse in lesson.LessonItems)
                                        {
                                            var completedItem = completedItems.FirstOrDefault(pi => pi.LessonItemId == lessonItemResponse.LessonItemId);
                                            if (completedItem != null)
                                            {
                                                lessonItemResponse.IsCompleted = true;
                                                lessonItemResponse.CompletedAt = completedItem.CreatedAt;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Cập nhật thông tin enrollment và progress
                if (studentProcesses != null && studentProcesses.Any())
                {
                    courseDetailResponse.IsEnrolled = true;
                    
                    // Tính overall progress (phần trăm hoàn thành)
                    if (totalLessons > 0)
                    {
                        courseDetailResponse.OverallProgress = Math.Round((double)completedLessons / totalLessons * 100, 2);
                    }
                    else
                    {
                        courseDetailResponse.OverallProgress = 0;
                    }
                }
                else
                {
                    courseDetailResponse.IsEnrolled = false;
                    courseDetailResponse.OverallProgress = null; // Không có progress nếu chưa enroll
                }

                return new BaseResponse<CourseDetailResponse>(
                    "Course detail retrieved successfully",
                    StatusCodeEnum.OK_200,
                    courseDetailResponse
                );
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logging mechanism
                throw new Exception($"Failed to get course detail: {ex.Message}");
            }
        }

        public async Task<Course> CreateCourseForTrelloAsync(string courseName, List<TrelloCardResponse> trelloCardResponses)
        {
            courseName = courseName.Replace("[course]", "").Trim();
            string description = "This is a course imported from Trello.";
            decimal price = 0;
            
            foreach (var trelloCardResponse in trelloCardResponses)
            {
                if (trelloCardResponse.Name.Contains("Description"))
                {
                    description = trelloCardResponse.Description;
                }
                if (trelloCardResponse.Name.Contains("Price"))
                {
                    price = decimal.Parse(trelloCardResponse.Description);
                }
            }
            
            var course = new Course
            {
                Name = courseName,
                Description = description,
                Price = price,
                ImageUrl = "",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var embeddingData = await _openAIEmbeddingsApiService.EmbedData(course);
            course.EmbeddingData = CommonUtils.ObjectToString(embeddingData);
            var createdCourse = await _courseRepository.AddAsync(course);
            return createdCourse;
        }

        // ========== Methods from CourseRegistrationService ==========

        public async Task<BaseResponse<IEnumerable<CourseRegistrationResponse>>> GetStudentRegistrationsAsync(string studentId)
        {
            try
            {
                var processes = await _processRepository.GetByStudentAsync(studentId);
                var grouped = processes.GroupBy(p => p.CourseId).ToList();
                var responses = new List<CourseRegistrationResponse>();
                foreach (var g in grouped)
                {
                    var course = await _courseRepository.GetByCourseIdWithCategoryAsync(g.Key);
                    if (course == null) continue;
                    var first = g.MinBy(p => p.CreatedAt ?? DateTime.UtcNow);
                    responses.Add(new CourseRegistrationResponse
                    {
                        CourseId = (int)g.Key,
                        CourseName = course.Name,
                        StudentId = studentId,
                        StudentName = "",
                        EnrollmentDate = first?.CreatedAt ?? DateTime.UtcNow,
                        Status = "Active"
                    });
                }

                return new BaseResponse<IEnumerable<CourseRegistrationResponse>>(
                    "Lấy danh sách khóa học của học sinh thành công",
                    StatusCodeEnum.OK_200,
                    responses
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách thất bại: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourseSectionResponse>> GetCourseContentAsync(long courseSectionId)
        {
            try
            {
                var courseSection = await _coursesectionRepository.GetByCourseSectionIdAsync(courseSectionId);
                if (courseSection == null)
                {
                    throw new Exception("Course section not found");
                }

                var lessons = await _lessonRepository.GetActiveLessonsBySectionAsync(courseSectionId);
                var allProcesses = new List<Process>();

                var response = new CourseSectionResponse
                {
                    CourseSectionId = courseSection.CourseSectionId,
                    CourseId = courseSection.CourseId,
                    Name = courseSection.Name,
                    Description = courseSection.Description,
                    Order = courseSection.Order,
                    IsActive = courseSection.IsActive == 1,
                    Lessons = new List<LessonResponse>()
                };

                foreach (var lesson in lessons)
                {
                    var process = allProcesses.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                    response.Lessons.Add(new LessonResponse
                    {
                        LessonId = lesson.LessonId,
                        CourseSectionId = lesson.CourseSectionId,
                        Name = lesson.Name,
                        Description = lesson.Description,
                        Order = lesson.Order,
                        IsActive = lesson.IsActive == 1,
                        IsUnlocked = process != null && process.IsUnlocked
                    });
                }

                return new BaseResponse<CourseSectionResponse>(
                    "Course content retrieved successfully",
                    StatusCodeEnum.OK_200,
                    response
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get course content: {ex.Message}");
            }
        }

        public async Task<BaseResponse<StudentProgressResponse>> GetStudentProgressAsync(string studentId, long courseId)
        {
            try
            {
                var course = await _courseRepository.GetByCourseIdWithCategoryAsync(courseId);
                if (course == null)
                {
                    throw new Exception("Course not found");
                }

                var processes = (await _processRepository.GetByStudentAndCourseAsync(studentId, courseId)).ToList();
                var courseSections = (await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseId)).OrderBy(s => s.Order).ToList();

                var progressResponse = new StudentProgressResponse
                {
                    CourseId = courseId,
                    CourseName = course.Name,
                    CategoryName = course.Module?.ModuleName ?? "Unknown",
                    StudentName = "",
                    Sections = new List<SectionProgressResponse>(),
                    OverallProgress = 0
                };

                int totalLessons = 0;
                int completedLessons = 0;

                foreach (var section in courseSections)
                {
                    var sectionProgress = new SectionProgressResponse
                    {
                        CourseSectionId = section.CourseSectionId,
                        SectionName = section.Name,
                        Order = section.Order,
                        IsActive = section.IsActive == 1,
                        Lessons = new List<LessonProgressResponse>()
                    };

                    var lessons = (await _lessonRepository.GetActiveLessonsBySectionAsync(section.CourseSectionId)).OrderBy(l => l.Order).ToList();

                    foreach (var lesson in lessons)
                    {
                        totalLessons++;
                        var process = processes.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                        var isCompleted = process != null && await _processRepository.IsLessonCompletedForStudentAsync(studentId, lesson.LessonId);
                        var isUnlocked = process != null && process.IsUnlocked;
                        if (isCompleted) completedLessons++;
                        var lessonProgress = new LessonProgressResponse
                        {
                            LessonId = lesson.LessonId,
                            LessonName = lesson.Name,
                            IsCompleted = isCompleted,
                            CompletedAt = process?.CreatedAt,
                            ItemProgress = new List<LessonItemProgressResponse>(),
                            IsUnlocked = isUnlocked
                        };
                        if (process != null)
                        {
                            var lessonItems = await _lessonitemRepository.GetByLessonIdAsync(lesson.LessonId);
                            var processItems = await _processitemRepository.GetByProcessIdAsync(process.ProcessId);
                            foreach (var item in lessonItems)
                            {
                                var isItemCompleted = processItems.Any(pi => pi.LessonItemId == item.LessonId);
                                lessonProgress.ItemProgress.Add(new LessonItemProgressResponse
                                {
                                    LessonItemId = item.LessonItemId,
                                    LessonItemName = item.Name,
                                    IsCompleted = isItemCompleted,
                                    CompletedAt = processItems.FirstOrDefault(pi => pi.LessonItemId == item.LessonId)?.CreatedAt
                                });
                            }
                        }
                        sectionProgress.Lessons.Add(lessonProgress);
                    }
                    progressResponse.Sections.Add(sectionProgress);
                }
                progressResponse.OverallProgress = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;
                return new BaseResponse<StudentProgressResponse>(
                    "Student progress retrieved successfully",
                    StatusCodeEnum.OK_200,
                    progressResponse
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get student progress: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> CompleteLessonItemAsync(string studentId, int lessonItemId)
        {
            try
            {
                // 1. Lấy thông tin lesson item
                var lessonItem = await _lessonitemRepository.GetByLessonItemIdAsync(lessonItemId);
                if (lessonItem == null)
                {
                    throw new Exception("Lesson item not found");
                }

                // 2. Tìm Process của học sinh cho bài học này
                var process = await _processRepository.GetByStudentAndLessonAsync(studentId, lessonItem.LessonId);
                if (process == null)
                {
                    throw new Exception("Student is not enrolled in this course or lesson not found");
                }

                // 3. Kiểm tra bài học đã được mở khóa chưa
                if (!process.IsUnlocked)
                {
                    throw new Exception("This lesson is locked. Please complete previous lessons first.");
                }

                // 4. Kiểm tra lesson item đã hoàn thành chưa (tránh duplicate)
                var existingProcessItem = await _processitemRepository.GetByProcessAndLessonItemAsync(process.ProcessId, lessonItemId);
                if (existingProcessItem != null)
                {
                    return new BaseResponse<bool>(
                        "Lesson item already completed",
                        StatusCodeEnum.OK_200,
                        true
                    );
                }

                // 5. Tạo Processitem mới để đánh dấu hoàn thành
                var processItem = new Processitem
                {
                    ProcessId = process.ProcessId,
                    LessonItemId = lessonItemId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _processitemRepository.AddAsync(processItem);

                // 6. Kiểm tra xem tất cả lesson items trong bài học đã hoàn thành chưa
                var allLessonItems = await _lessonitemRepository.GetByLessonIdAsync(lessonItem.LessonId);
                var completedProcessItems = await _processitemRepository.GetByProcessIdAsync(process.ProcessId);
                
                bool isLessonCompleted = allLessonItems.Count() == completedProcessItems.Count();

                // 7. Nếu bài học hoàn thành, mở khóa bài học tiếp theo
                if (isLessonCompleted)
                {
                    process.UpdatedAt = DateTime.UtcNow;
                    await _processRepository.UpdateAsync(process);

                    // Tìm bài học tiếp theo
                    var lesson = await _lessonRepository.GetByLessonIdAsync(lessonItem.LessonId);
                    if (lesson != null)
                    {
                        // Lấy tất cả lessons trong section hiện tại
                        var lessonsInSection = (await _lessonRepository.GetActiveLessonsBySectionAsync(lesson.CourseSectionId))
                            .OrderBy(l => l.Order)
                            .ToList();
                        
                        var currentLessonIndex = lessonsInSection.FindIndex(l => l.LessonId == lesson.LessonId);
                        
                        // Kiểm tra có bài học tiếp theo trong section không
                        if (currentLessonIndex >= 0 && currentLessonIndex < lessonsInSection.Count - 1)
                        {
                            // Mở khóa bài học tiếp theo trong cùng section
                            var nextLesson = lessonsInSection[currentLessonIndex + 1];
                            var nextProcess = await _processRepository.GetByStudentAndLessonAsync(studentId, nextLesson.LessonId);
                            if (nextProcess != null && !nextProcess.IsUnlocked)
                            {
                                nextProcess.IsUnlocked = true;
                                nextProcess.UpdatedAt = DateTime.UtcNow;
                                await _processRepository.UpdateAsync(nextProcess);
                            }
                        }
                        else
                        {
                            // Đã hết bài học trong section, mở bài học đầu tiên của section tiếp theo
                            var courseSection = await _coursesectionRepository.GetByCourseSectionIdAsync(lesson.CourseSectionId);
                            if (courseSection != null)
                            {
                                var allSections = (await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseSection.CourseId))
                                    .OrderBy(s => s.Order)
                                    .ToList();
                                
                                var currentSectionIndex = allSections.FindIndex(s => s.CourseSectionId == courseSection.CourseSectionId);
                                
                                if (currentSectionIndex >= 0 && currentSectionIndex < allSections.Count - 1)
                                {
                                    var nextSection = allSections[currentSectionIndex + 1];
                                    var firstLessonInNextSection = (await _lessonRepository.GetActiveLessonsBySectionAsync(nextSection.CourseSectionId))
                                        .OrderBy(l => l.Order)
                                        .FirstOrDefault();
                                    
                                    if (firstLessonInNextSection != null)
                                    {
                                        var nextProcess = await _processRepository.GetByStudentAndLessonAsync(studentId, firstLessonInNextSection.LessonId);
                                        if (nextProcess != null && !nextProcess.IsUnlocked)
                                        {
                                            nextProcess.IsUnlocked = true;
                                            nextProcess.UpdatedAt = DateTime.UtcNow;
                                            await _processRepository.UpdateAsync(nextProcess);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return new BaseResponse<bool>(
                    isLessonCompleted 
                        ? "Lesson item completed! Next lesson unlocked." 
                        : "Lesson item completed successfully",
                    StatusCodeEnum.OK_200,
                    true
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to complete lesson item: {ex.Message}");
            }
        }

        public async Task InitializeCourseProgressForStudentAsync(string studentId, int courseId)
        {
            try
            {
                var courseSections = (await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseId))?.OrderBy(s => s.Order).ToList();
                if (courseSections == null || !courseSections.Any()) return;

                foreach (var section in courseSections)
                {
                    var lessons = (await _lessonRepository.GetActiveLessonsBySectionAsync(section.CourseSectionId))?.OrderBy(l => l.Order).ToList();
                    if (lessons == null || !lessons.Any()) continue;
                    if (section == courseSections.First())
                    {
                        for (int i = 0; i < lessons.Count; i++)
                        {
                            var isUnlocked = (i == 0);
                            var process = new Process
                            {
                                StudentId = studentId,
                                CourseId = courseId,
                                LessonId = lessons[i].LessonId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsUnlocked = isUnlocked
                            };
                            await _processRepository.AddAsync(process);
                        }
                    }
                    else
                    {
                        foreach (var lesson in lessons)
                        {
                            var process = new Process
                            {
                                StudentId = studentId,
                                CourseId = courseId,
                                LessonId = lesson.LessonId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsUnlocked = false
                            };
                            await _processRepository.AddAsync(process);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<BaseResponse<IEnumerable<StudentProgressOverviewResponse>>> GetLinkedStudentsProgressAsync(string parentId)
        {
            try
            {
                // Kiểm tra parent tồn tại
                var parent = await _accountRepository.GetByStringId(parentId);
                if (parent == null)
                {
                    throw new Exception("Parent not found.");
                }

                // Kiểm tra role là Parent
                var parentRole = await _userManager.GetRolesAsync(parent);
                if (!parentRole.Contains("Parent"))
                {
                    throw new Exception("You are not a parent.");
                }

                // Lấy danh sách students liên kết
                var linkedStudents = await _parentStudentLinkRepository.GetByParentId(parentId);
                
                var result = new List<StudentProgressOverviewResponse>();

                foreach (var student in linkedStudents)
                {
                    var studentProgress = new StudentProgressOverviewResponse
                    {
                        StudentId = student.Id,
                        StudentName = student.Name ?? student.UserName ?? "Unknown",
                        StudentEmail = student.Email ?? "",
                        Courses = new List<CourseProgressSummary>()
                    };

                    // Lấy tất cả processes của student
                    var processes = await _processRepository.GetByStudentAsync(student.Id);
                    
                    // Group theo CourseId để tính progress
                    var courseGroups = processes.GroupBy(p => p.CourseId).ToList();

                    foreach (var courseGroup in courseGroups)
                    {
                        var courseId = courseGroup.Key;
                        var course = await _courseRepository.GetByCourseIdAsync(courseId);
                        
                        if (course == null) continue;

                        // Lấy enrollment date (ngày tạo process đầu tiên)
                        var enrolledAt = courseGroup.Min(p => p.CreatedAt ?? DateTime.UtcNow);

                        // Tính tổng số lessons và lessons đã hoàn thành
                        int totalLessons = 0;
                        int completedLessons = 0;

                        foreach (var process in courseGroup)
                        {
                            totalLessons++;
                            
                            // Kiểm tra lesson đã hoàn thành chưa
                            var processItems = await _processitemRepository.GetByProcessIdAsync(process.ProcessId);
                            var lessonItems = await _lessonitemRepository.GetByLessonIdAsync(process.LessonId);
                            
                            if (lessonItems.Any() && processItems.Count() == lessonItems.Count())
                            {
                                completedLessons++;
                            }
                        }

                        // Tính progress percentage
                        double overallProgress = totalLessons > 0 
                            ? Math.Round((double)completedLessons / totalLessons * 100, 2) 
                            : 0;

                        studentProgress.Courses.Add(new CourseProgressSummary
                        {
                            CourseId = courseId,
                            CourseName = course.Name,
                            CourseImageUrl = course.ImageUrl ?? "",
                            EnrolledAt = enrolledAt,
                            OverallProgress = overallProgress,
                            TotalLessons = totalLessons,
                            CompletedLessons = completedLessons
                        });
                    }

                    result.Add(studentProgress);
                }

                return new BaseResponse<IEnumerable<StudentProgressOverviewResponse>>(
                    $"Lấy tiến trình học của {result.Count} học sinh thành công",
                    StatusCodeEnum.OK_200,
                    result
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy tiến trình học: {ex.Message}");
            }
        }
    }
}
