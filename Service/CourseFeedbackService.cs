using AutoMapper;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.Model;
using Common.Constants;
using Repository.IRepositories;

namespace Service
{
    public class CourseFeedbackService
    {
        private readonly ICourseFeedbackRepository _courseFeedbackRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentEnrollmentRepository _studentEnrollmentRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IProcessRepository _processRepository;
        private readonly ICourseFeedbackReactionRepository _reactionRepository;
        private readonly IMapper _mapper;

        public CourseFeedbackService(
            ICourseFeedbackRepository courseFeedbackRepository,
            ICourseRepository courseRepository,
            IStudentEnrollmentRepository studentEnrollmentRepository,
            IAccountRepository accountRepository,
            IProcessRepository processRepository,
            ICourseFeedbackReactionRepository reactionRepository,
            IMapper mapper)
        {
            _courseFeedbackRepository = courseFeedbackRepository;
            _courseRepository = courseRepository;
            _studentEnrollmentRepository = studentEnrollmentRepository;
            _accountRepository = accountRepository;
            _processRepository = processRepository;
            _reactionRepository = reactionRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<CourseFeedbackResponse>> CreateFeedbackAsync(string studentId, int courseId, CourseFeedbackRequest request)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var isEnrolled = await _studentEnrollmentRepository.IsStudentEnrolledAsync(studentId, courseId);
            if (!isEnrolled)
            {
                throw new Exception("Bạn cần đăng ký khóa học này trước khi gửi feedback");
            }

            var hasCompletedCourse = await _processRepository.HasStudentCompletedCourseAsync(studentId, courseId);
            if (!hasCompletedCourse)
            {
                throw new Exception("Chỉ học viên đã hoàn thành khóa học mới có thể gửi feedback");
            }

            var student = await _accountRepository.GetByStringId(studentId);
            if (student == null)
            {
                throw new Exception("Không tìm thấy thông tin học viên");
            }

            var existingFeedback = await _courseFeedbackRepository.GetByCourseAndStudentAsync(courseId, studentId);
            if (existingFeedback != null)
            {
                throw new Exception("Bạn đã gửi feedback cho khóa học này");
            }

            var timestamp = DateTime.UtcNow;
            var feedback = new Coursefeedback
            {
                CourseId = courseId,
                StudentId = studentId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            feedback = await _courseFeedbackRepository.AddAsync(feedback);
            feedback.Student = student;
            var response = _mapper.Map<CourseFeedbackResponse>(feedback);
            return new BaseResponse<CourseFeedbackResponse>(
                "Tạo feedback thành công",
                StatusCodeEnum.OK_200,
                response);
        }

        public async Task<BaseResponse<IEnumerable<CourseFeedbackResponse>>> GetCourseFeedbacksAsync(int courseId)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var feedbacks = await _courseFeedbackRepository.GetByCourseIdAsync(courseId);
            var responses = _mapper.Map<IEnumerable<CourseFeedbackResponse>>(feedbacks);

            return new BaseResponse<IEnumerable<CourseFeedbackResponse>>(
                "Lấy danh sách feedback thành công",
                StatusCodeEnum.OK_200,
                responses);
        }

        public async Task<BaseResponse<PaginatedResponse<CourseFeedbackResponse>>> GetCourseFeedbacksPagedAsync(int courseId, CourseFeedbackQueryRequest request, string? currentUserId = null)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var (feedbacks, total) = await _courseFeedbackRepository.GetFeedbacksPagedAsync(courseId, request);
            var feedbackList = feedbacks.ToList();
            
            var responses = new List<CourseFeedbackResponse>();
            foreach (var feedback in feedbackList)
            {
                var response = _mapper.Map<CourseFeedbackResponse>(feedback);
                // Sử dụng cache từ database
                response.LikeCount = feedback.LikeCount;
                response.UnlikeCount = feedback.UnlikeCount;
                
                // Kiểm tra trạng thái của current user nếu đã đăng nhập
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    response.IsLikedByCurrentUser = await _reactionRepository.HasUserLikedAsync(feedback.CourseFeedbackId, currentUserId);
                    response.IsUnlikedByCurrentUser = await _reactionRepository.HasUserUnlikedAsync(feedback.CourseFeedbackId, currentUserId);
                }
                
                responses.Add(response);
            }

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var paginatedResponse = new PaginatedResponse<CourseFeedbackResponse>
            {
                Items = responses,
                TotalCount = total,
                Page = page - 1,
                Size = pageSize,
                TotalPages = totalPages
            };

            return new BaseResponse<PaginatedResponse<CourseFeedbackResponse>>(
                "Lấy danh sách feedback thành công",
                StatusCodeEnum.OK_200,
                paginatedResponse);
        }

        public async Task<BaseResponse<CourseFeedbackResponse>> ReactToFeedbackAsync(int courseFeedbackId, string userId, string reactionType)
        {
            var feedback = await _courseFeedbackRepository.GetByIdAsync(courseFeedbackId);
            if (feedback == null)
            {
                throw new Exception("Feedback not found");
            }

            var existingReaction = await _reactionRepository.GetByFeedbackAndUserAsync(courseFeedbackId, userId);
            var timestamp = DateTime.UtcNow;

            if (existingReaction == null)
            {
                // Tạo reaction mới
                var newReaction = new CourseFeedbackReaction
                {
                    CourseFeedbackId = courseFeedbackId,
                    UserId = userId,
                    ReactionType = reactionType,
                    CreatedAt = timestamp,
                    UpdatedAt = timestamp
                };
                await _reactionRepository.AddAsync(newReaction);

                // Cập nhật cache
                if (reactionType == "Like")
                {
                    feedback.LikeCount += 1;
                }
                else
                {
                    feedback.UnlikeCount += 1;
                }
            }
            else
            {
                // Đã có reaction trước đó
                if (existingReaction.ReactionType == reactionType)
                {
                    // Nếu click lại cùng loại reaction thì xóa (toggle off)
                    await _reactionRepository.DeleteAsync(existingReaction);
                    
                    // Cập nhật cache
                    if (reactionType == "Like")
                    {
                        feedback.LikeCount = Math.Max(0, feedback.LikeCount - 1);
                    }
                    else
                    {
                        feedback.UnlikeCount = Math.Max(0, feedback.UnlikeCount - 1);
                    }
                }
                else
                {
                    // Đổi từ Like sang Unlike hoặc ngược lại
                    var oldType = existingReaction.ReactionType;
                    existingReaction.ReactionType = reactionType;
                    existingReaction.UpdatedAt = timestamp;
                    await _reactionRepository.UpdateAsync(existingReaction);

                    // Cập nhật cache
                    if (oldType == "Like")
                    {
                        feedback.LikeCount = Math.Max(0, feedback.LikeCount - 1);
                        feedback.UnlikeCount += 1;
                    }
                    else
                    {
                        feedback.UnlikeCount = Math.Max(0, feedback.UnlikeCount - 1);
                        feedback.LikeCount += 1;
                    }
                }
            }

            feedback.UpdatedAt = timestamp;
            await _courseFeedbackRepository.UpdateAsync(feedback);

            var response = _mapper.Map<CourseFeedbackResponse>(feedback);
            response.LikeCount = feedback.LikeCount;
            response.UnlikeCount = feedback.UnlikeCount;
            response.IsLikedByCurrentUser = await _reactionRepository.HasUserLikedAsync(courseFeedbackId, userId);
            response.IsUnlikedByCurrentUser = await _reactionRepository.HasUserUnlikedAsync(courseFeedbackId, userId);

            return new BaseResponse<CourseFeedbackResponse>(
                $"{(existingReaction == null ? "Thêm" : existingReaction.ReactionType == reactionType ? "" : "Đổi")} {reactionType} thành công",
                StatusCodeEnum.OK_200,
                response);
        }

        public async Task<BaseResponse<object>> GetCourseFeedbackSummaryAsync(int courseId)
        {
            var course = await _courseRepository.GetByCourseIdAsync(courseId);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            var averageRating = await _courseFeedbackRepository.GetAverageRatingAsync(courseId);
            var totalFeedback = await _courseFeedbackRepository.GetFeedbackCountAsync(courseId);

            return new BaseResponse<object>(
                "Lấy thống kê feedback thành công",
                StatusCodeEnum.OK_200,
                new
                {
                    AverageRating = Math.Round(averageRating, 2),
                    TotalFeedback = totalFeedback
                });
        }
    }
}

