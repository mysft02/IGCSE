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
using Repository.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Service.Trello;
using BusinessObject.DTOs.Response.FinalQuizzes;
using MimeKit.Tnef;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Bcpg;

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
        private readonly IStudentEnrollmentRepository _studentEnrollmentRepository;
        private readonly MediaService _mediaService;
        private readonly TrelloCardService _trelloCardService;
        private readonly ICreateSlotRepository _createSlotRepository;
        private readonly IFinalQuizResultRepository _finalQuizResultRepository;
        private readonly IFinalQuizRepository _finalQuizRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

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
            UserManager<Account> userManager,
            IStudentEnrollmentRepository studentEnrollmentRepository,
            MediaService mediaService,
            TrelloCardService trelloCardService,
            ICreateSlotRepository createSlotRepository,
            IFinalQuizResultRepository finalQuizResultRepository,
            IFinalQuizRepository finalQuizRepository,
            IWebHostEnvironment webHostEnvironment)
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
            _studentEnrollmentRepository = studentEnrollmentRepository;
            _mediaService = mediaService;
            _trelloCardService = trelloCardService;
            _createSlotRepository = createSlotRepository;
            _finalQuizResultRepository = finalQuizResultRepository;
            _finalQuizRepository = finalQuizRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<BaseResponse<CourseResponse>> ApproveCourseAsync(int courseId)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            if (course.Status != "Pending")
            {
                throw new Exception($"Không thể duyệt khóa học. Trạng thái hiện tại: {course.Status}");
            }

            // Đổi trạng thái thành Open, không thay đổi AvailableSlot (đã -1 khi tạo rồi)
            course.Status = "Open";
            course.UpdatedAt = DateTime.UtcNow;

            var updated = await _courseRepository.UpdateAsync(course);
            var response = _mapper.Map<CourseResponse>(updated);
            return new BaseResponse<CourseResponse>(
                "Course approved successfully",
                StatusCodeEnum.OK_200,
                response
            );
        }

        public async Task<BaseResponse<CourseResponse>> RejectCourseAsync(int courseId, string? reason)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            if (course.Status != "Pending")
            {
                throw new Exception($"Không thể từ chối khóa học. Trạng thái hiện tại: {course.Status}");
            }

            // Đổi trạng thái thành Rejected và tăng AvailableSlot + 1 (trả lại slot)
            course.Status = "Rejected";
            course.UpdatedAt = DateTime.UtcNow;

            // Trả lại AvailableSlot cho teacher
            if (!string.IsNullOrEmpty(course.CreatedBy))
            {
                var createSlot = await _createSlotRepository.FindOneAsync(x => x.TeacherId == course.CreatedBy);
                if (createSlot != null)
                {
                    createSlot.AvailableSlot += 1;
                    await _createSlotRepository.UpdateAsync(createSlot);
                }
            }

            var updated = await _courseRepository.UpdateAsync(course);
            var response = _mapper.Map<CourseResponse>(updated);
            return new BaseResponse<CourseResponse>(
                string.IsNullOrWhiteSpace(reason) ? "Course rejected" : $"Course rejected: {reason}",
                StatusCodeEnum.OK_200,
                response
            );
        }

        public async Task<BaseResponse<CourseResponse>> GetCourseByIdAsync(int courseId)
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
                if (course.ImageUrl != null)
                {
                    try
                    {
                        course.ImageUrl = await _mediaService.GetMediaUrlAsync(course.ImageUrl);
                    }
                    catch (FileNotFoundException)
                    {
                        course.ImageUrl = string.Empty;
                    }
                    catch (Exception)
                    {
                        course.ImageUrl = string.Empty;
                    }
                }
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

            var (items, total) = await _courseRepository.SearchAsync(page, pageSize, query);
            var courseResponses = items.Select(i => _mapper.Map<CourseResponse>(i)).ToList();
            foreach (var course in courseResponses)
            {
                course.ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(course.ImageUrl);
            }

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

        public async Task<BaseResponse<CourseSectionResponse>> CreateCourseSectionAsync(int courseId, CourseSectionRequest request, string userId)
        {
            // Kiểm tra course tồn tại và thuộc về teacher
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            if (course.CreatedBy != userId)
            {
                throw new Exception("Bạn không có quyền tạo section cho khóa học này.");
            }

            var courseSection = _mapper.Map<Coursesection>(request);
            courseSection.CourseId = courseId; // Set CourseId từ parameter
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

        public async Task<BaseResponse<LessonResponse>> CreateLessonAsync(int sectionId, LessonRequest request, string userId)
        {
            // Kiểm tra section tồn tại và thuộc về teacher
            var section = await _coursesectionRepository.GetByCourseSectionIdAsync(sectionId);
            if (section == null)
            {
                throw new Exception("Course section not found");
            }

            var course = await _courseRepository.GetByCourseIdAsync(section.CourseId);
            if (course == null || course.CreatedBy != userId)
            {
                throw new Exception("Bạn không có quyền tạo lesson cho section này.");
            }

            var lesson = new Lesson
            {
                CourseSectionId = sectionId, // Set CourseSectionId từ parameter
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

        public async Task<BaseResponse<LessonItemResponse>> CreateLessonItemAsync(int lessonId, LessonItemRequest request, string userId)
        {
            // Kiểm tra lesson tồn tại và thuộc về teacher
            var lesson = await _lessonRepository.GetByLessonIdAsync(lessonId);
            if (lesson == null)
            {
                throw new Exception("Lesson not found");
            }

            var section = await _coursesectionRepository.GetByCourseSectionIdAsync(lesson.CourseSectionId);
            if (section == null)
            {
                throw new Exception("Course section not found");
            }

            var course = await _courseRepository.GetByCourseIdAsync(section.CourseId);
            if (course == null || course.CreatedBy != userId)
            {
                throw new Exception("Bạn không có quyền tạo lesson item cho lesson này.");
            }

            // Xử lý upload file nếu có
            string content = request.Content ?? string.Empty;
            string itemType = request.ItemType ?? "text";

            if (request.File != null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath;
                
                if (FileUploadHelper.IsValidLessonVideo(request.File))
                {
                    content = await FileUploadHelper.UploadLessonVideoAsync(request.File, webRootPath);
                    itemType = "video";
                }
                else if (FileUploadHelper.IsValidLessonDocument(request.File))
                {
                    content = await FileUploadHelper.UploadLessonDocumentAsync(request.File, webRootPath);
                    itemType = "pdf";
                }
                else if (FileUploadHelper.IsValidImageFile(request.File))
                {
                    content = await FileUploadHelper.UploadCourseImageAsync(request.File, webRootPath);
                    itemType = "image";
                }
                else
                {
                    throw new Exception("File type not supported. Only video, PDF, and image files are allowed.");
                }
            }

            var lessonItem = new Lessonitem
            {
                LessonId = lessonId, // Set LessonId từ parameter
                Name = request.Name,
                Description = request.Description,
                Content = content,
                ItemType = itemType,
                Order = request.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdLessonItem = await _lessonitemRepository.AddAsync(lessonItem);
            var response = _mapper.Map<LessonItemResponse>(createdLessonItem);
            
            // Convert content to full URL
            if (!string.IsNullOrEmpty(response.Content))
            {
                response.Content = await _mediaService.GetMediaUrlAsync(response.Content);
            }

            return new BaseResponse<LessonItemResponse>(
                "Lesson item created successfully",
                StatusCodeEnum.Created_201,
                response
            );
        }

        public async Task<BaseResponse<IEnumerable<CourseSectionResponse>>> GetCourseSectionsAsync(int courseId)
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

        public async Task<BaseResponse<IEnumerable<LessonItemResponse>>> GetLessonItemsAsync(int lessonId)
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

        public async Task<BaseResponse<IEnumerable<CourseResponse>>> GetAllSimilarCoursesAsync(int courseId, string userId)
        {
            decimal score = 1;

            var userResult = await _finalQuizResultRepository.FindOneWithIncludeAsync(x => x.UserId == userId && x.FinalQuiz.CourseId == courseId && x.IsPassed == true, x => x.FinalQuiz);
            if(userResult != null)
            {
                score = userResult.Score == 0 ? 1 : userResult.Score;
            }

            var similarCourses = await _courseRepository.GetAllSimilarCoursesAsync(courseId, score);

            var courseResponses = new List<CourseResponse>();
            foreach (var course in similarCourses)
            {
                var courseResponse = _mapper.Map<CourseResponse>(course);
                if (courseResponse.ImageUrl != null)
                {
                    try
                    {
                        courseResponse.ImageUrl = await _mediaService.GetMediaUrlAsync(courseResponse.ImageUrl);
                    }
                    catch (FileNotFoundException)
                    {
                        courseResponse.ImageUrl = string.Empty;
                    }
                    catch (Exception)
                    {
                        courseResponse.ImageUrl = string.Empty;
                    }
                }
                courseResponses.Add(courseResponse);
            }

            return new BaseResponse<IEnumerable<CourseResponse>>(
                "Courses retrieved successfully",
                StatusCodeEnum.OK_200,
                courseResponses
            );
        }

        public async Task<BaseResponse<PaginatedResponse<CourseResponse>>> GetTeacherCoursesAsync(TeacherCourseQueryRequest request)
        {
            var filter = request.BuildFilter<Course>();

            var totalCount = await _courseRepository.CountAsync(filter);

            var items = await _courseRepository.FindWithPagingAsync(
            filter,
            request.Page,
            request.GetPageSize()
            );

            var pagedItems = await Task.WhenAll(
                items.Select(async x => new CourseResponse
                {
                    CourseId = x.CourseId,
                    Name = x.Name,
                    Description = x.Description,
                    Status = x.Status,
                    Price = x.Price,
                    ImageUrl = string.IsNullOrEmpty(x.ImageUrl) ? null : await _mediaService.GetMediaUrlAsync(x.ImageUrl),
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                }));

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            return new BaseResponse<PaginatedResponse<CourseResponse>>(
                    $"Tìm thấy {totalCount} enrollments",
                    StatusCodeEnum.OK_200,
                    new PaginatedResponse<CourseResponse>
                    {
                        Items = pagedItems.ToList(),
                        TotalCount = totalCount,
                        Page = request.Page,
                        Size = request.GetPageSize(),
                        TotalPages = totalPages
                    }
                );
        }

        public async Task<BaseResponse<CourseAnalyticsResponse>> GetCourseAnalyticsAsync(int courseId)
        {
            var result = await _courseRepository.GetCourseAnalyticsAsync(courseId);

            // Map to response
            return new BaseResponse<CourseAnalyticsResponse>
            {
                Data = result,
                Message = "Lấy thống kê khoá học thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }
        public async Task<BaseResponse<CourseDetailResponse>> GetCourseDetailForStudentAsync(int courseId, string? studentId = null)
        {
            // 1. Get course with related data
            var course = await _courseRepository.GetByCourseIdWithCategoryAsync(courseId);
            if (course == null)
                throw new Exception("Course not found");

            // 2. Map course to DTO
            var courseDetailResponse = _mapper.Map<CourseDetailResponse>(course);
            // Kiểm tra FinalQuiz có tồn tại không
            if (course.FinalQuiz != null)
            {
                courseDetailResponse.FinalQuiz = _mapper.Map<FinalQuizCourseDetailResponse>(course.FinalQuiz);
            }
            else
            {
                // Tạo FinalQuiz mặc định nếu chưa có
                courseDetailResponse.FinalQuiz = new FinalQuizCourseDetailResponse
                {
                    FinalQuizId = 0,
                    Title = $"{course.Name} final quiz",
                    Description = "Summary course content"
                };
            }

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
                        foreach (var c in section.Lessons)
                        {
                            var lesson = await _lessonRepository.FindOneWithIncludeAsync(x => x.LessonId == c.LessonId, xc => xc.Quiz);
                            // Kiểm tra Quiz có tồn tại không
                            if (lesson != null && lesson.Quiz != null)
                            {
                                c.Quiz = _mapper.Map<LessonQuizResponse>(lesson.Quiz);
                            }
                            else
                            {
                                // Tạo Quiz mặc định nếu chưa có
                                c.Quiz = new LessonQuizResponse
                                {
                                    QuizId = 0,
                                    QuizTitle = string.Empty,
                                    QuizDescription = string.Empty
                                };
                            }
                        }

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

            // Cập nhật thông tin enrollment và progress dựa trên process records
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

            if (!string.IsNullOrEmpty(course.ImageUrl))
            {
                try
                {
                    courseDetailResponse.ImageUrl = await _mediaService.GetMediaUrlAsync(courseDetailResponse.ImageUrl);
                }
                catch (FileNotFoundException)
                {
                    courseDetailResponse.ImageUrl = string.Empty;
                }
                catch (Exception)
                {
                    courseDetailResponse.ImageUrl = string.Empty;
                }
            }

            return new BaseResponse<CourseDetailResponse>(
                "Course detail retrieved successfully",
                StatusCodeEnum.OK_200,
                courseDetailResponse
            );
        }

        public async Task<BaseResponse<CourseDetailWithoutProgressResponse>> GetCourseDetailAsync(int courseId, string? userId = null, string? userRole = null)
        {
            var courseDetail = await _courseRepository.GetCourseDetailAsync(courseId);
            
            if (courseDetail == null)
            {
                throw new Exception("Course not found");
            }

            // Kiểm tra IsEnrolled: chỉ true khi user là Student và đã enroll khóa học
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                courseDetail.IsEnrolled = false;
            }
            else
            {
                // Kiểm tra xem Student có enroll khóa học này không
                courseDetail.IsEnrolled = await _studentEnrollmentRepository.IsStudentEnrolledAsync(userId, courseId);
            }

            // Convert ImageUrl to full URL
            if (!string.IsNullOrEmpty(courseDetail.ImageUrl))
            {
                try
                {
                    courseDetail.ImageUrl = await _mediaService.GetMediaUrlAsync(courseDetail.ImageUrl);
                }
                catch
                {
                    courseDetail.ImageUrl = string.Empty;
                }
            }

            // Convert Content URL to full URL for all lesson items
            if (courseDetail.Sections != null)
            {
                foreach (var section in courseDetail.Sections)
                {
                    if (section.Lessons != null)
                    {
                        foreach (var lesson in section.Lessons)
                        {
                            if (lesson.LessonItems != null)
                            {
                                foreach (var lessonItem in lesson.LessonItems)
                                {
                                    if (!string.IsNullOrEmpty(lessonItem.Content))
                                    {
                                        try
                                        {
                                            lessonItem.Content = await _mediaService.GetMediaUrlAsync(lessonItem.Content);
                                        }
                                        catch
                                        {
                                            // Keep original content if conversion fails
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new BaseResponse<CourseDetailWithoutProgressResponse>(
                "Lấy khoá học thành công",
                StatusCodeEnum.OK_200,
                courseDetail
                );
        }

        public async Task<BaseResponse<LessonItemDetail>> GetLessonItemDetailAsync(string userId, int lessonItemId, string userRole)
        {
            if(userRole == "Teacher")
            {
                var checkOwned = await _courseRepository.CheckOwnedByLessonItemId(lessonItemId, userId);
                if (!checkOwned)
                {
                    throw new Exception("Bạn không thể xem bài học của giáo viên khác.");
                }
            }

            var lessonItemDetail = await _lessonitemRepository.FindOneAsync(x => x.LessonItemId == lessonItemId);
            if (lessonItemDetail == null)
            {
                throw new Exception("Không tìm thấy nội dung bài học.");
            }

            var result = _mapper.Map<LessonItemDetail>(lessonItemDetail);
            try
            {
                result.Content = await _mediaService.GetMediaUrlAsync(result.Content);
            }
            catch (FileNotFoundException)
            {
                result.Content = string.Empty;
            }
            catch (Exception)
            {
                result.Content = string.Empty;
            }

            return new BaseResponse<LessonItemDetail>
            {
                Message = "Lấy nội dung bài học thành công.",
                Data = result,
                StatusCode = StatusCodeEnum.OK_200,
            };
        }

        public async Task<BaseResponse<CourseResponse>> CreateCourseAsync(CourseRequest request, string userId)
        {
            // Kiểm tra available slot
            var createSlot = await _createSlotRepository.FindOneAsync(x => x.TeacherId == userId);
            if (createSlot == null)
            {
                throw new Exception("Bạn chưa mua gói để tạo mới khoá học. Vui lòng mua gói trước.");
            }

            if (createSlot.AvailableSlot <= 0)
            {
                throw new Exception("Bạn không còn suất tạo mới khoá học. Vui lòng mua thêm gói.");
            }

            // Kiểm tra module tồn tại
            var module = await _moduleRepository.GetByIdAsync(request.ModuleId);
            if (module == null)
            {
                throw new Exception("Module not found");
            }

            // Xử lý upload image nếu có
            string imageUrl = request.ImageUrl;
            if (request.ImageFile != null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath;
                imageUrl = await FileUploadHelper.UploadCourseImageAsync(request.ImageFile, webRootPath);
            }
            else if (string.IsNullOrEmpty(imageUrl))
            {
                // Default image nếu không có image
                imageUrl = "/courses/images/7daee94e-f728-48a8-95aa-ee68566b617e.jpg";
            }

            // Tạo course
            var course = _mapper.Map<Course>(request);
            course.ImageUrl = imageUrl;
            course.Status = "Pending"; // Mặc định là Pending để manager duyệt
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.CreatedBy = userId;
            course.UpdatedBy = userId;

            // Tạo embedding data
            var embeddingData = await _openAIEmbeddingsApiService.EmbedData(course);
            course.EmbeddingData = CommonUtils.ObjectToString(embeddingData);

            // Lưu course
            var createdCourse = await _courseRepository.AddAsync(course);

            try
            {
                // Trừ available slot NGAY SAU KHI tạo course thành công
                // Reload createSlot để tránh stale data
                createSlot = await _createSlotRepository.FindOneAsync(x => x.TeacherId == userId);
                if (createSlot != null)
                {
                    createSlot.AvailableSlot -= 1;
                    await _createSlotRepository.UpdateAsync(createSlot);
                }

                // Tạo FinalQuiz cho course
                var finalQuiz = new Finalquiz
                {
                    CourseId = createdCourse.CourseId,
                    Title = $"{createdCourse.Name} final quiz",
                    Description = "Summary course content",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _finalQuizRepository.AddAsync(finalQuiz);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi sau khi tạo course, rollback bằng cách xóa course
                try
                {
                    await _courseRepository.DeleteAsync(createdCourse);
                }
                catch
                {
                    // Ignore delete error
                }
                throw new Exception($"Lỗi khi tạo khóa học: {ex.Message}");
            }

            // Map response
            var response = _mapper.Map<CourseResponse>(createdCourse);
            response.ImageUrl = string.IsNullOrEmpty(response.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(response.ImageUrl);

            return new BaseResponse<CourseResponse>(
                "Course created successfully",
                StatusCodeEnum.Created_201,
                response
            );
        }

        public async Task<Course> CreateCourseForTrelloAsync(string courseName, List<TrelloCardResponse> trelloCardResponses, string userId, TrelloToken trelloToken)
        {
            courseName = courseName.Replace("[course]", "").Trim();

            var course = new Course
            {
                Name = courseName,
                Status = "Pending",
                ImageUrl = "/courses/images/7daee94e-f728-48a8-95aa-ee68566b617e.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            foreach (var trelloCardResponse in trelloCardResponses)
            {
                if (trelloCardResponse.Name.Contains("Description"))
                {
                    course.Description = trelloCardResponse.Description;
                }
                if (trelloCardResponse.Name.Contains("Price"))
                {
                    course.Price = decimal.Parse(trelloCardResponse.Description);
                }
                if (trelloCardResponse.Name.Contains("Image"))
                {
                    var attachments = await _trelloCardService.GetTrelloCardAttachments(trelloCardResponse.Id, trelloToken);

                    // get first attachment that is image
                    var imageUrl = string.Empty;
                    if (!CommonUtils.isEmtyList(attachments))
                    {
                        var imageAttachment = attachments.FirstOrDefault();
                        imageUrl = await _trelloCardService.DownloadTrelloCardAttachment(imageAttachment.Url, trelloToken);
                    }

                    course.ImageUrl = imageUrl;
                }
            }

            var embeddingData = await _openAIEmbeddingsApiService.EmbedData(course);
            course.EmbeddingData = CommonUtils.ObjectToString(embeddingData);
            var createdCourse = await _courseRepository.AddAsync(course);
            return createdCourse;
        }

        // ========== Methods from CourseRegistrationService ==========

        public async Task<BaseResponse<PaginatedResponse<CourseRegistrationResponse>>> GetStudentRegistrationsAsync(string studentId, CourseRegistrationQueryRequest request, string userRole)
        {
            try
            {
                var processes = await _studentEnrollmentRepository.FindWithIncludeAsync(x => x.StudentId == studentId, c => c.Course);
                var grouped = processes.GroupBy(p => p.CourseId).ToList();
                var responses = new List<CourseRegistrationResponse>();
                
                foreach (var g in grouped)
                {
                    var course = g.First().Course;
                    if (course == null) continue;
                    var first = g.MinBy(p => p.Course?.CreatedAt ?? DateTime.UtcNow);

                    var finalQuizResult = await _finalQuizResultRepository.FindOneWithIncludeAsync(x => x.FinalQuiz.CourseId == course.CourseId && x.UserId == studentId && x.IsPassed == true, c => c.FinalQuiz);

                    var result = new CourseRegistrationResponse
                    {
                        CourseId = (int)g.Key,
                        CourseName = course.Name,
                        CourseDescription= course.Description,
                        ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(course.ImageUrl),
                        CreatedBy = studentId,
                        EnrollmentDate = first?.Course.CreatedAt ?? DateTime.UtcNow,
                        Status = finalQuizResult != null ? "Completed" : "In Progress"
                    };

                    responses.Add(result);
                }

                // Apply filters
                var filtered = responses.AsQueryable();
                
                if (!string.IsNullOrEmpty(request.SearchByCourseName))
                {
                    filtered = filtered.Where(r => r.CourseName.Contains(request.SearchByCourseName, StringComparison.OrdinalIgnoreCase));
                }
                var totalCount = filtered.Count();

                var pagedItems = filtered
                    .Skip(request.Page * request.GetPageSize())
                    .Take(request.GetPageSize())
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

                return new BaseResponse<PaginatedResponse<CourseRegistrationResponse>>(
                    "Lấy danh sách khóa học của học sinh thành công",
                    StatusCodeEnum.OK_200,
                    new PaginatedResponse<CourseRegistrationResponse>
                    {
                        Items = pagedItems,
                        TotalCount = totalCount,
                        Page = request.Page,
                        Size = request.GetPageSize(),
                        TotalPages = totalPages
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách thất bại: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourseSectionResponse>> GetCourseContentAsync(int courseSectionId)
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

        public async Task<BaseResponse<StudentProgressResponse>> GetStudentProgressAsync(string studentId, int courseId)
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

        public async Task<BaseResponse<PaginatedResponse<StudentProgressOverviewResponse>>> GetLinkedStudentsProgressAsync(string parentId, StudentProgressQueryRequest request)
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
                var linkedStudents = (await _parentStudentLinkRepository.GetByParentId(parentId)).ToList();
                
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
                    var processes = (await _processRepository.GetByStudentAsync(student.Id)).ToList();
                    
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
                            var processItems = (await _processitemRepository.GetByProcessIdAsync(process.ProcessId)).ToList();
                            var lessonItems = (await _lessonitemRepository.GetByLessonIdAsync(process.LessonId)).ToList();
                            
                            if (lessonItems.Any() && processItems.Count == lessonItems.Count)
                            {
                                completedLessons++;
                            }
                        }

                        // Tính progress percentage
                        double overallProgress = totalLessons > 0 
                            ? Math.Round((double)completedLessons / totalLessons * 100, 2) 
                            : 0;

                        var courseProgress = new CourseProgressSummary
                        {
                            CourseId = courseId,
                            CourseName = course.Name,
                            CourseImageUrl = course.ImageUrl ?? "",
                            EnrolledAt = enrolledAt,
                            OverallProgress = overallProgress,
                            TotalLessons = totalLessons,
                            CompletedLessons = completedLessons
                        };

                        if (!string.IsNullOrEmpty(courseProgress.CourseImageUrl))
                        {
                            try
                            {
                                courseProgress.CourseImageUrl = await _mediaService.GetMediaUrlAsync(courseProgress.CourseImageUrl);
                            }
                            catch (FileNotFoundException)
                            {
                                courseProgress.CourseImageUrl = string.Empty;
                            }
                            catch (Exception)
                            {
                                courseProgress.CourseImageUrl = string.Empty;
                            }
                        }

                        studentProgress.Courses.Add(courseProgress);
                    }

                    result.Add(studentProgress);
                }

                // Apply filters
                var filtered = result.AsQueryable();
                
                if (!string.IsNullOrEmpty(request.SearchByStudentName))
                {
                    filtered = filtered.Where(s => s.StudentName.Contains(request.SearchByStudentName, StringComparison.OrdinalIgnoreCase));
                }
                
                if (!string.IsNullOrEmpty(request.StudentId))
                {
                    filtered = filtered.Where(s => s.StudentId == request.StudentId);
                }
                
                if (request.CourseId.HasValue)
                {
                    filtered = filtered.Where(s => s.Courses.Any(c => c.CourseId == request.CourseId.Value));
                }
                
                if (request.MinProgress.HasValue)
                {
                    filtered = filtered.Where(s => s.Courses.Any(c => c.OverallProgress >= request.MinProgress.Value));
                }
                
                if (request.MaxProgress.HasValue)
                {
                    filtered = filtered.Where(s => s.Courses.Any(c => c.OverallProgress <= request.MaxProgress.Value));
                }

                else
                {
                    filtered = filtered.OrderBy(s => s.StudentName);
                }

                // Get total count before pagination
                var totalCount = filtered.Count();

                // Apply pagination
                var pagedItems = filtered
                    .Skip(request.Page * request.GetPageSize())
                    .Take(request.GetPageSize())
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

                return new BaseResponse<PaginatedResponse<StudentProgressOverviewResponse>>(
                    $"Lấy tiến trình học của {totalCount} học sinh thành công",
                    StatusCodeEnum.OK_200,
                    new PaginatedResponse<StudentProgressOverviewResponse>
                    {
                        Items = pagedItems,
                        TotalCount = totalCount,
                        Page = request.Page,
                        Size = request.GetPageSize(),
                        TotalPages = totalPages
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy tiến trình học: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PaginatedResponse<ParentEnrollmentResponse>>> GetCourseBuyByParentAsync(string parentId, ParentEnrollmentQueryRequest request)
        {
            var filter = request.BuildFilter<Studentenrollment>();

            // Get total count first (for pagination info)
            var totalCount = await _studentEnrollmentRepository.CountAsync(filter);

            // Get filtered items
            var items = await _studentEnrollmentRepository.GetListBoughtCourses(request, filter);
            foreach(var item in items)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    item.ImageUrl = await _mediaService.GetMediaUrlAsync(item.ImageUrl);
                }
            }

            // Apply pagination after sorting
            var pagedItems = items
                .Skip(request.Page * request.GetPageSize())
                .Take(request.GetPageSize())
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            return new BaseResponse<PaginatedResponse<ParentEnrollmentResponse>>(
                    $"Tìm thấy {totalCount} enrollments",
                    StatusCodeEnum.OK_200,
                    new PaginatedResponse<ParentEnrollmentResponse>
                    {
                        Items = pagedItems,
                        TotalCount = totalCount,
                        Page = request.Page,
                        Size = request.GetPageSize(),
                        TotalPages = totalPages
                    }
                );
        }

        public async Task<BaseResponse<IEnumerable<CourseResponse>>> GetSimilarCourses(string userId, string userRole)
        {
            var embeddingData = new List<float>();

            if(userRole == "Student")
            {
                var request = await _courseRepository.GetCourseAndFinalQuizResult(userId);
                embeddingData = await _openAIEmbeddingsApiService.EmbedData(request);
            }

            if(userRole == "Parent")
            {
                var request = new List<SimilarCourseForStudentRequest>();

                var studentList = await _parentStudentLinkRepository.FindAsync(x => x.ParentId == userId);
                if(studentList == null)
                {
                    var parentData = await _courseRepository.GetCourseAndFinalQuizResult(userId);
                }

                foreach(var student in studentList)
                {
                    var studentData = await _courseRepository.GetCourseAndFinalQuizResult(student.StudentId);
                    request.Add(studentData);
                }

                embeddingData = await _openAIEmbeddingsApiService.EmbedData(request);
            }

            var courseList = await _courseRepository.GetAllSimilarCoursesForStudentAsync(embeddingData);

            var result = new List<CourseResponse>();
            foreach (var course in courseList)
            {
                var courseResponse = _mapper.Map<CourseResponse>(course);
                courseResponse.ImageUrl = string.IsNullOrEmpty(courseResponse.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(courseResponse.ImageUrl);
                result.Add(courseResponse);
            }

            return new BaseResponse<IEnumerable<CourseResponse>>(
                "Lấy danh sách khoá học tương tự thành công.",
                StatusCodeEnum.OK_200,
                result
                );
        }

        public async Task<BaseResponse<List<ActivityCountResponse>>> GetStudentActivityCount(string userId, string userRole, string studentId)
        {
            string destStudentId;

            if(studentId != null)
            {
                if(userRole == "Student" && userId != studentId)
                {
                    throw new Exception("Bạn không được phép truy cập thông tin này.");
                }

                destStudentId = studentId;
            }
            else
            {
                if(userRole == "Parent")
                {
                    throw new Exception("Id của học sinh không được phép trống.");
                }

                destStudentId = userId;
            }

            int year = DateTime.UtcNow.Year;
            var result = await _courseRepository.GetActivityForYear(destStudentId, year);

            return new BaseResponse<List<ActivityCountResponse>>(
                "Lấy danh sách hoạt động thành công.",
                StatusCodeEnum.OK_200,
                result
                );
        }
    }
}
