using Repository.IRepositories;
using AutoMapper;
using Common.Constants;
using DTOs.Request.Courses;
using DTOs.Response.Courses;
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
        public CourseService(IMapper mapper, ICourseRepository courseRepository)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<DTOs.Response.Accounts.BaseResponse<CourseResponse>> CreateCourseAsync(CourseRequest request)
        {
            try
            {
                // Create new course
                var course = new Course
                {
                    CourseName = request.CourseName,
                    Description = request.Description,
                    Price = request.Price,
                    ImageUrl = request.ImageUrl,
                    CategoryId = request.CategoryId,
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

        public async Task<BaseResponse<CourseResponse>> UpdateCourseAsync(int courseId, CourseRequest request)
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
                existingCourse.CourseName = request.CourseName;
                existingCourse.Description = request.Description;
                existingCourse.Status = request.Status == 1;
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

        public async Task<BaseResponse<CourseResponse>> GetCourseByIdAsync(int courseId)
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
    }
}
