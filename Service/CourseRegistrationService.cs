using AutoMapper;
using BusinessObject.Model;
using DTOs.Request.CourseRegistration;
using DTOs.Response.CourseContent;
using DTOs.Response.CourseRegistration;
using Repository.IRepositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTOs.Response.Accounts;
using Common.Constants;

namespace Service
{
    public class CourseRegistrationService
    {
        private readonly IMapper _mapper;
        private readonly ICoursekeyRepository _coursekeyRepository;
        private readonly ICoursesectionRepository _coursesectionRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonitemRepository _lessonitemRepository;
        private readonly IProcessRepository _processRepository;
        private readonly IProcessitemRepository _processitemRepository;
        private readonly ICourseRepository _courseRepository;

        public CourseRegistrationService(
            IMapper mapper,
            ICoursekeyRepository coursekeyRepository,
            ICoursesectionRepository coursesectionRepository,
            ILessonRepository lessonRepository,
            ILessonitemRepository lessonitemRepository,
            IProcessRepository processRepository,
            IProcessitemRepository processitemRepository,
            ICourseRepository courseRepository)
        {
            _mapper = mapper;
            _coursekeyRepository = coursekeyRepository;
            _coursesectionRepository = coursesectionRepository;
            _lessonRepository = lessonRepository;
            _lessonitemRepository = lessonitemRepository;
            _processRepository = processRepository;
            _processitemRepository = processitemRepository;
            _courseRepository = courseRepository;
        }

        public async Task<BaseResponse<CourseRegistrationResponse>> RegisterForCourseAsync(CourseRegistrationRequest request)
        {
            try
            {
                // Check if course exists
                var course = await _courseRepository.GetByCourseIdWithCategoryAsync(request.CourseId);
                if (course == null)
                {
                    throw new Exception("Course not found");
                }

                // Check if student is already registered for this course
                var existingRegistration = await _coursekeyRepository.GetByCourseAndStudentAsync(request.CourseId, request.StudentId);
                if (existingRegistration != null)
                {
                    throw new Exception("Student is already registered for this course");
                }

                // Generate unique course key
                var courseKey = await _coursekeyRepository.GenerateUniqueCourseKeyAsync(request.CourseId, request.StudentId);

                // Create course registration
                var coursekey = new Coursekey
                {
                    CourseId = request.CourseId,
                    StudentId = request.StudentId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = request.StudentId
                };

                var createdCoursekey = await _coursekeyRepository.AddAsync(coursekey);

                // Initialize course progress
                await InitializeCourseProgressAsync(createdCoursekey.CourseKeyId);

                var response = new CourseRegistrationResponse
                {
                    CourseKeyId = createdCoursekey.CourseKeyId,
                    CourseId = createdCoursekey.CourseId,
                    CourseName = course.Name,
                    CategoryName = course.Category?.CategoryName ?? "Unknown",
                    StudentId = request.StudentId,
                    StudentName = "", // Will be populated from Account service
                    CourseKey = courseKey,
                    EnrollmentDate = createdCoursekey.CreatedAt ?? DateTime.UtcNow,
                    Status = "Active"
                };

                return new BaseResponse<CourseRegistrationResponse>(
                    "Successfully registered for course",
                    StatusCodeEnum.Created_201,
                    response
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register for course: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<CourseRegistrationResponse>>> GetStudentRegistrationsAsync(string studentId)
        {
            try
            {
                var registrations = await _coursekeyRepository.GetByStudentIdAsync(studentId);

                var responses = new List<CourseRegistrationResponse>();
                foreach (var reg in registrations)
                {
                    var course = await _courseRepository.GetByCourseIdWithCategoryAsync(reg.CourseId);
                    if (course != null)
                    {
                        responses.Add(new CourseRegistrationResponse
                        {
                            CourseKeyId = reg.CourseKeyId,
                            CourseId = reg.CourseId,
                            CourseName = course.Name,
                            CategoryName = course.Category?.CategoryName ?? "Unknown",
                            StudentId = studentId,
                            StudentName = "", // Will be populated from Account service
                            CourseKey = $"{reg.CourseId}-{reg.StudentId}-{reg.CreatedAt?.Ticks}",
                            EnrollmentDate = reg.CreatedAt ?? DateTime.UtcNow,
                            Status = "Active"
                        });
                    }
                }

                return new BaseResponse<IEnumerable<CourseRegistrationResponse>>(
                    "Student registrations retrieved successfully",
                    StatusCodeEnum.OK_200,
                    responses
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get student registrations: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourseSectionResponse>> GetCourseContentAsync(long courseKeyId, long courseSectionId)
        {
            try
            {
                // Verify course key exists and is valid
                var courseKey = await _coursekeyRepository.GetByCourseKeyAsync(courseKeyId);
                if (courseKey == null)
                {
                    throw new Exception("Invalid course key");
                }

                var courseSection = await _coursesectionRepository.GetByCourseSectionIdAsync(courseSectionId);
                if (courseSection == null)
                {
                    throw new Exception("Course section not found");
                }

                var lessons = await _lessonRepository.GetActiveLessonsBySectionAsync(courseSectionId);

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
                    response.Lessons.Add(new LessonResponse
                    {
                        LessonId = lesson.LessonId,
                        CourseSectionId = lesson.CourseSectionId,
                        Name = lesson.Name,
                        Description = lesson.Description,
                        Order = lesson.Order,
                        IsActive = lesson.IsActive == 1
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

        public async Task<BaseResponse<StudentProgressResponse>> GetStudentProgressAsync(long courseKeyId)
        {
            try
            {
                var courseKey = await _coursekeyRepository.GetByCourseKeyAsync(courseKeyId);
                if (courseKey == null)
                {
                    throw new Exception("Invalid course key");
                }

                var course = await _courseRepository.GetByCourseIdWithCategoryAsync(courseKey.CourseId);
                if (course == null)
                {
                    throw new Exception("Course not found");
                }

                var processes = await _processRepository.GetByCourseKeyAsync(courseKeyId);
                var courseSections = await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseKey.CourseId);

                var progressResponse = new StudentProgressResponse
                {
                    CourseKeyId = courseKeyId,
                    CourseId = courseKey.CourseId,
                    CourseName = course.Name,
                    CategoryName = course.Category?.CategoryName ?? "Unknown",
                    StudentName = "",
                    LessonProgress = new List<LessonProgressResponse>(),
                    OverallProgress = 0
                };

                int totalLessons = 0;
                int completedLessons = 0;

                foreach (var section in courseSections)
                {
                    var lessons = await _lessonRepository.GetActiveLessonsBySectionAsync(section.CourseSectionId);

                    foreach (var lesson in lessons)
                    {
                        totalLessons++;
                        var process = processes.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                        var isCompleted = process != null && await _processRepository.IsLessonCompletedAsync(courseKeyId, lesson.LessonId);

                        if (isCompleted) completedLessons++;

                        var lessonProgress = new LessonProgressResponse
                        {
                            LessonId = lesson.LessonId,
                            LessonName = lesson.Name,
                            IsCompleted = isCompleted,
                            CompletedAt = process?.CreatedAt,
                            ItemProgress = new List<LessonItemProgressResponse>()
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

                        progressResponse.LessonProgress.Add(lessonProgress);
                    }
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

        public async Task<BaseResponse<bool>> CompleteLessonItemAsync(int courseKeyId, int lessonItemId)
        {
            try
            {
                var courseKey = await _coursekeyRepository.GetByCourseKeyAsync(courseKeyId);
                if (courseKey == null)
                {
                    throw new Exception("Invalid course key");
                }

                var lessonItem = await _lessonitemRepository.GetByLessonItemIdAsync(lessonItemId);
                if (lessonItem == null)
                {
                    throw new Exception("Lesson item not found");
                }

                // Get or create process for this lesson
                var process = await _processRepository.GetByCourseKeyAndLessonAsync(courseKeyId, lessonItem.LessonId);
                if (process == null)
                {
                    process = new Process
                    {
                        CourseKeyId = courseKeyId,
                        LessonId = lessonItem.LessonId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    process = await _processRepository.AddAsync(process);
                }

                // Check if lesson item is already completed
                var existingProcessItem = await _processitemRepository.GetByProcessAndLessonItemAsync(process.ProcessId, lessonItemId);
                if (existingProcessItem != null)
                {
                    return new BaseResponse<bool>(
                        "Lesson item already completed",
                        StatusCodeEnum.OK_200,
                        true
                    );
                }

                // Create process item to mark lesson item as completed
                var processItem = new Processitem
                {
                    ProcessId = process.ProcessId,
                    LessonItemId = lessonItemId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _processitemRepository.AddAsync(processItem);

                return new BaseResponse<bool>(
                    "Lesson item completed successfully",
                    StatusCodeEnum.Created_201,
                    true
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to complete lesson item: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Coursekey>> GetAllCourseKeysAsync()
        {
            return await _coursekeyRepository.GetAllAsync();
        }

        public async Task UpdateCourseKeyAsync(Coursekey key)
        {
            await _coursekeyRepository.UpdateAsync(key);
        }

        public async Task InitializeCourseProgressAsync(int courseKeyId)
        {
            try
            {
                var courseKey = await _coursekeyRepository.GetByCourseKeyAsync(courseKeyId);
                if (courseKey == null) return;

                var courseSections = await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseKey.CourseId);

                foreach (var section in courseSections)
                {
                    var lessons = await _lessonRepository.GetActiveLessonsBySectionAsync(section.CourseSectionId);

                    foreach (var lesson in lessons)
                    {
                        var process = new Process
                        {
                            CourseKeyId = courseKeyId,
                            LessonId = lesson.LessonId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _processRepository.AddAsync(process);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - initialization shouldn't fail registration
                Console.WriteLine($"Warning: Failed to initialize course progress: {ex.Message}");
            }
        }
    }
}
