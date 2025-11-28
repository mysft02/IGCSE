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
        private readonly IMapper _mapper;

        public CourseFeedbackService(
            ICourseFeedbackRepository courseFeedbackRepository,
            ICourseRepository courseRepository,
            IStudentEnrollmentRepository studentEnrollmentRepository,
            IAccountRepository accountRepository,
            IProcessRepository processRepository,
            IMapper mapper)
        {
            _courseFeedbackRepository = courseFeedbackRepository;
            _courseRepository = courseRepository;
            _studentEnrollmentRepository = studentEnrollmentRepository;
            _accountRepository = accountRepository;
            _processRepository = processRepository;
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

