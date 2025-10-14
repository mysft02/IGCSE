using Repository.IRepositories;
using AutoMapper;
using Common.Constants;
using DTOs.Request.Courses;
using DTOs.Response.Courses;
using DTOs.Request.CourseContent;
using DTOs.Response.CourseContent;
using BusinessObject.Model;
using DTOs.Response.Accounts;
using Service.OpenAI;
using Common.Utils;

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

        public CourseService(
            IMapper mapper,
            ICourseRepository courseRepository,
            ICoursesectionRepository coursesectionRepository,
            ILessonRepository lessonRepository,
            ILessonitemRepository lessonitemRepository,
            OpenAIEmbeddingsApiService openAIEmbeddingsApiService)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
            _coursesectionRepository = coursesectionRepository;
            _lessonRepository = lessonRepository;
            _lessonitemRepository = lessonitemRepository;
            _openAIEmbeddingsApiService = openAIEmbeddingsApiService;
        }

        public async Task<BaseResponse<CourseResponse>> CreateCourseAsync(CourseRequest request)
        {
                // Create new course
                var course = new Course
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    ImageUrl = request.ImageUrl,
                    CategoryId = request.CategoryId,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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

        public async Task<BaseResponse<CourseResponse>> UpdateCourseAsync(long courseId, CourseRequest request)
        {
                // Get existing course
                var existingCourse = await _courseRepository.GetByCourseIdAsync(courseId);
                if (existingCourse == null)
                {
                    throw new Exception("Course not found");
                }

                // Validate Category exists if CategoryId is being updated
                if (request.CategoryId != existingCourse.CategoryId)
                {
                    var courses = await _courseRepository.GetAllAsync();
                    var categoryExists = courses.Any(c => c.CategoryId == request.CategoryId);
                    if (!categoryExists)
                    {
                        throw new Exception("Category not found or inactive");
                    }
                }

                // Update course properties
                existingCourse.Name = request.Name;
                existingCourse.Description = request.Description;
                existingCourse.Status = request.Status;
                existingCourse.Price = request.Price;
                existingCourse.ImageUrl = request.ImageUrl;
                existingCourse.CategoryId = request.CategoryId;
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

        // Course Content Management Methods

        public async Task<BaseResponse<CourseSectionResponse>> CreateCourseSectionAsync(CourseSectionRequest request)
        {
                var courseSection = new Coursesection
                {
                    CourseId = request.CourseId,
                    Name = request.Name,
                    Description = request.Description,
                    Order = request.Order,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

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
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to get lesson items: {ex.Message}");
            }
        }
    }
}
