using AutoMapper;
using BusinessObject.Model;
using DTOs.Response.CourseContent;
using DTOs.Response.CourseRegistration;
using Repository.IRepositories;
using Common.Constants;
using BusinessObject.DTOs.Response;

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
                var allProcesses = (await _processRepository.GetByCourseKeyAsync(courseKeyId)).ToList();

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
                // Tìm ra courseKey phù hợp cho student và course
                var courseKeys = await _coursekeyRepository.GetByStudentIdAsync(studentId);
                var courseKey = courseKeys.FirstOrDefault(k => k.CourseId == courseId);
                if (courseKey == null)
                {
                    throw new Exception("Student is not enrolled in this course");
                }

                var course = await _courseRepository.GetByCourseIdWithCategoryAsync(courseId);
                if (course == null)
                {
                    throw new Exception("Course not found");
                }

                var processes = (await _processRepository.GetByCourseKeyAsync(courseKey.CourseKeyId)).ToList();
                var courseSections = (await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseId)).OrderBy(s => s.Order).ToList();

                var progressResponse = new StudentProgressResponse
                {
                    CourseKeyId = courseKey.CourseKeyId,
                    CourseId = courseId,
                    CourseName = course.Name,
                    CategoryName = course.Category?.CategoryName ?? "Unknown",
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
                        var isCompleted = process != null && await _processRepository.IsLessonCompletedAsync(courseKey.CourseKeyId, lesson.LessonId);
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
                        UpdatedAt = DateTime.UtcNow,
                        IsUnlocked = true
                    };
                    process = await _processRepository.AddAsync(process);
                }

                // Kiểm tra nếu chưa unlock thì không cho học
                if (!process.IsUnlocked)
                {
                    return new BaseResponse<bool>(
                        "Bài học chưa được mở, cần hoàn thành các bài phía trước!",
                        StatusCodeEnum.BadRequest_400,
                        false
                    );
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

                // Kiểm tra nếu đã hoàn thành tất cả lesson item của lesson này
                var allLessonItems = (await _lessonitemRepository.GetByLessonIdAsync(lessonItem.LessonId)).ToList();
                var allProcessItems = (await _processitemRepository.GetByProcessIdAsync(process.ProcessId)).ToList();
                if (allLessonItems.Count > 0 && allLessonItems.Count == allProcessItems.Count + 1) // +1 vì item hiện tại vừa add chưa nằm trong list
                {
                    // unlock lesson tiếp theo cùng section
                    var lesson = await _lessonRepository.GetByLessonIdAsync(lessonItem.LessonId);
                    var lessonsOfSection = (await _lessonRepository.GetActiveLessonsBySectionAsync(lesson.CourseSectionId)).OrderBy(l => l.Order).ToList();
                    var curLessonIndex = lessonsOfSection.FindIndex(l => l.LessonId == lesson.LessonId);
                    if (curLessonIndex >= 0 && curLessonIndex + 1 < lessonsOfSection.Count)
                    {
                        var nextLesson = lessonsOfSection[curLessonIndex + 1];
                        var nextProcess = await _processRepository.GetByCourseKeyAndLessonAsync(courseKeyId, nextLesson.LessonId);
                        if (nextProcess != null && !nextProcess.IsUnlocked)
                        {
                            nextProcess.IsUnlocked = true;
                            await _processRepository.UpdateAsync(nextProcess);
                        }
                    }
                    else if (curLessonIndex == lessonsOfSection.Count - 1) // đã là lesson cuối của section, unlock section tiếp theo
                    {
                        var allSections = (await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseKey.CourseId)).OrderBy(s => s.Order).ToList();
                        var thisSectionIndex = allSections.FindIndex(s => s.CourseSectionId == lesson.CourseSectionId);
                        if (thisSectionIndex >= 0 && thisSectionIndex + 1 < allSections.Count)
                        {
                            var nextSection = allSections[thisSectionIndex + 1];
                            var nextSectionLessons = (await _lessonRepository.GetActiveLessonsBySectionAsync(nextSection.CourseSectionId)).OrderBy(l => l.Order).ToList();
                            if (nextSectionLessons.Any())
                            {
                                var firstLessonNextSection = nextSectionLessons.First();
                                var processFirstLesson = await _processRepository.GetByCourseKeyAndLessonAsync(courseKeyId, firstLessonNextSection.LessonId);
                                if (processFirstLesson != null && !processFirstLesson.IsUnlocked)
                                {
                                    processFirstLesson.IsUnlocked = true;
                                    await _processRepository.UpdateAsync(processFirstLesson);
                                }
                            }
                        }
                    }
                }

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
            return await _coursekeyRepository.GetAllCourseKeysWithNullHandlingAsync();
        }

        public async Task<IEnumerable<Coursekey>> GetAvailableCourseKeysAsync()
        {
            return await _coursekeyRepository.GetAvailableCourseKeysAsync();
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

                var courseSections = (await _coursesectionRepository.GetActiveSectionsByCourseAsync(courseKey.CourseId))?.OrderBy(s => s.Order).ToList();
                if (courseSections == null || !courseSections.Any()) return;
                // Chỉ unlock section đầu tiên (bằng cách unlock lesson đầu của nó)
                foreach (var section in courseSections)
                {
                    var lessons = (await _lessonRepository.GetActiveLessonsBySectionAsync(section.CourseSectionId))?.OrderBy(l => l.Order).ToList();
                    if (lessons == null || !lessons.Any()) continue;
                    if (section == courseSections.First())
                    {
                        for (int i = 0; i < lessons.Count; i++)
                        {
                            var isUnlocked = (i == 0); // lesson đầu tiên được mở
                            var process = new Process
                            {
                                CourseKeyId = courseKeyId,
                                LessonId = lessons[i].LessonId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsUnlocked = isUnlocked
                            };
                            await _processRepository.AddAsync(process);
                        }
                    }
                    else // Section sau: chưa unlock lesson nào
                    {
                        foreach (var lesson in lessons)
                        {
                            var process = new Process
                            {
                                CourseKeyId = courseKeyId,
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
            catch (Exception ex)
            {
                // Log error but don't throw - initialization shouldn't fail registration
            }
        }
    }
}
