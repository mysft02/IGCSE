using AutoMapper;
using BusinessObject.Model;
using Common.Constants;
using DTOs.Request.Courses;
using DTOs.Response.Courses;
using Repository.IRepositories;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
                // Validate Category exists
                var categoryExists = await _courseRepository.GetAllAsync()
                    .ContinueWith(t => t.Result.Any(c => c.CategoryID == request.CategoryID));
                if (!categoryExists)
                {
                    throw new Exception("Category not found or inactive");
                }

                // Create new course
                var course = new Course
                {
                    CourseName = request.CourseName,
                    Description = request.Description,
                    Status = request.Status,
                    Price = request.Price,
                    ImageUrl = request.ImageUrl,
                    CategoryID = request.CategoryID,
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

        public async Task<DTOs.Response.Accounts.BaseResponse<CourseResponse>> UpdateCourseAsync(int courseId, CourseRequest request)
        {
            try
            {
                // Get existing course
                var existingCourse = await _courseRepository.GetByCourseIdAsync(courseId);
                if (existingCourse == null)
                {
                    throw new Exception("Course not found");
                }

                // Validate Category exists if CategoryID is being updated
                if (request.CategoryID != existingCourse.CategoryID)
                {
                    var categoryExists = await _courseRepository.GetAllAsync()
                        .ContinueWith(t => t.Result.Any(c => c.CategoryID == request.CategoryID));
                    if (!categoryExists)
                    {
                        throw new Exception("Category not found or inactive");
                    }
                }

                // Update course properties
                existingCourse.CourseName = request.CourseName;
                existingCourse.Description = request.Description;
                existingCourse.Status = request.Status;
                existingCourse.Price = request.Price;
                existingCourse.ImageUrl = request.ImageUrl;
                existingCourse.CategoryID = request.CategoryID;
                existingCourse.UpdatedAt = DateTime.UtcNow;

                var updatedCourse = await _courseRepository.UpdateAsync(existingCourse);

                var courseResponse = _mapper.Map<CourseResponse>(updatedCourse);
                courseResponse.CategoryName = updatedCourse.Category?.CategoryName ?? "";

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

        public async Task<DTOs.Response.Accounts.BaseResponse<CourseResponse>> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var course = await _courseRepository.GetByCourseIdAsync(courseId);
                if (course == null)
                {
                    throw new Exception("Course not found");
                }

                var courseResponse = _mapper.Map<CourseResponse>(course);
                courseResponse.CategoryName = course.Category?.CategoryName ?? "";

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
                    courseResponse.CategoryName = course.Category?.CategoryName ?? "";
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
