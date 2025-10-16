using Repository.IRepositories;
using AutoMapper;
using Common.Constants;
using DTOs.Request.Courses;
using DTOs.Response.Courses;
using DTOs.Request.CourseContent;
using DTOs.Response.CourseContent;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BusinessObject.Model;
using DTOs.Response.Accounts;

namespace Service
{
    public class CourseService
    {
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ICoursesectionRepository _coursesectionRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonitemRepository _lessonitemRepository;

        public CourseService(
            IMapper mapper,
            ICourseRepository courseRepository,
            ICoursesectionRepository coursesectionRepository,
            ILessonRepository lessonRepository,
            ILessonitemRepository lessonitemRepository)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
            _coursesectionRepository = coursesectionRepository;
            _lessonRepository = lessonRepository;
            _lessonitemRepository = lessonitemRepository;
        }

        public async Task<BaseResponse<CourseResponse>> CreateCourseAsync(CourseRequest request)
        {
            try
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

                var createdCourse = await _courseRepository.AddAsync(course);

                var courseResponse = _mapper.Map<CourseResponse>(createdCourse);

                return new DTOs.Response.Accounts.BaseResponse<CourseResponse>(
                    "Course created successfully",
                    StatusCodeEnum.Created_201,
                    courseResponse
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create course: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourseResponse>> UpdateCourseAsync(long courseId, CourseRequest request)
        {
            try
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

                return new DTOs.Response.Accounts.BaseResponse<CourseResponse>(
                    "Course updated successfully",
                    StatusCodeEnum.OK_200,
                    courseResponse
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update course: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourseResponse>> GetCourseByIdAsync(long courseId)
        {
            try
            {
                var course = await _courseRepository.GetByCourseIdAsync(courseId);
                if (course == null)
                {
                    throw new Exception("Course not found");
                }

                var courseResponse = _mapper.Map<CourseResponse>(course);

                return new DTOs.Response.Accounts.BaseResponse<CourseResponse>(
                    "Course retrieved successfully",
                    StatusCodeEnum.OK_200,
                    courseResponse
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get course: {ex.Message}");
            }
        }

        public async Task<DTOs.Response.Accounts.BaseResponse<IEnumerable<CourseResponse>>> GetAllCoursesAsync()
        {
            try
            {
                var courses = await _courseRepository.GetAllAsync();

                var courseResponses = new List<CourseResponse>();
                foreach (var course in courses)
                {
                    var courseResponse = _mapper.Map<CourseResponse>(course);
                    courseResponses.Add(courseResponse);
                }

                return new DTOs.Response.Accounts.BaseResponse<IEnumerable<CourseResponse>>(
                    "Courses retrieved successfully",
                    StatusCodeEnum.OK_200,
                    courseResponses
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get courses: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PagedResponse<CourseResponse>>> GetCoursesPagedAsync(CourseListQuery query)
        {
            try
            {
                var page = query.Page <= 0 ? 1 : query.Page;
                var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

                var (items, total) = await _courseRepository.SearchAsync(page, pageSize, query.SearchByName, query.CouseId, query.Status);
                var courseResponses = items.Select(i => _mapper.Map<CourseResponse>(i)).ToList();

                var totalPages = (int)Math.Ceiling(total / (double)pageSize);
                var paged = new PagedResponse<CourseResponse>
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = total,
                    Data = courseResponses
                };

                return new DTOs.Response.Accounts.BaseResponse<PagedResponse<CourseResponse>>(
                    "Courses retrieved successfully",
                    StatusCodeEnum.OK_200,
                    paged
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get courses: {ex.Message}");
            }
        }

        // Course Content Management Methods

        public async Task<BaseResponse<CourseSectionResponse>> CreateCourseSectionAsync(CourseSectionRequest request)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to create course section: {ex.Message}");
            }
        }

        public async Task<BaseResponse<LessonResponse>> CreateLessonAsync(LessonRequest request)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to create lesson: {ex.Message}");
            }
        }

        public async Task<BaseResponse<LessonItemResponse>> CreateLessonItemAsync(LessonItemRequest request)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to create lesson item: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<CourseSectionResponse>>> GetCourseSectionsAsync(long courseId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to get course sections: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<LessonItemResponse>>> GetLessonItemsAsync(long lessonId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to get lesson items: {ex.Message}");
            }
        }
    }
}
