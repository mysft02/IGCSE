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
            IProcessitemRepository processitemRepository)
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

        public async Task<BaseResponse<CourseAnalyticsResponse>> GetCourseAnalyticsAsync()
        {
            var courseAnalytics = await _courseRepository.GetCoursesSortedByStatus();

            var result = new CourseAnalyticsResponse
            {
                TotalCourse = await _courseRepository.CountAsync(),
                Partion = courseAnalytics
            };

            return new BaseResponse<CourseAnalyticsResponse>(
                "Course analytics retrieved successfully",
                StatusCodeEnum.OK_200,
                result
            );
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
    }
}
