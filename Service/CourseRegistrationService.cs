using AutoMapper;
using BusinessObject.Model;
using Repository.IRepositories;
using Common.Constants;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.CourseContent;

namespace Service
{
    public class CourseRegistrationService
    {
        private readonly IMapper _mapper;
        private readonly ICoursesectionRepository _coursesectionRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonitemRepository _lessonitemRepository;
        private readonly IProcessRepository _processRepository;
        private readonly IProcessitemRepository _processitemRepository;
        private readonly ICourseRepository _courseRepository;

        public CourseRegistrationService(
            IMapper mapper,
            ICoursesectionRepository coursesectionRepository,
            ILessonRepository lessonRepository,
            ILessonitemRepository lessonitemRepository,
            IProcessRepository processRepository,
            IProcessitemRepository processitemRepository,
            ICourseRepository courseRepository)
        {
            _mapper = mapper;
            // coursekey removed
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
    }
}
