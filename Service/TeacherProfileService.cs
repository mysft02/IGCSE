using AutoMapper;
using BusinessObject.DTOs.Request.TeacherProfiles;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.TeacherProfile;
using BusinessObject.Model;
using Common.Constants;
using Common.Utils;
using Microsoft.AspNetCore.Hosting;
using Repository.IRepositories;

namespace Service
{
    public class TeacherProfileService
    {
        private readonly ITeacherProfileRepository _teacherProfileRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeacherProfileService(
            ITeacherProfileRepository teacherProfileRepository,
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment)
        {
            _teacherProfileRepository = teacherProfileRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<BaseResponse<TeacherProfileResponse>> GetProfileByIdAsync(string teacherId)
        {
            var teacherProfile = await _teacherProfileRepository.FindOneWithIncludeAsync(x => x.TeacherId == teacherId, c => c.Certificates);
            var result = _mapper.Map<TeacherProfileResponse>(teacherProfile);

            return new BaseResponse<TeacherProfileResponse>(
                "Lấy profile giáo viên thành công",
                StatusCodeEnum.OK_200,
                result);
        }

        public async Task<BaseResponse<TeacherProfileResponse>> CreateTeacherProfile(TeacherProfileCreateRequest request, string userId)
        {
            var result = new TeacherProfileResponse();
            var teacherProfile = await _teacherProfileRepository.FindOneAsync(x => x.TeacherId == userId);

            if(teacherProfile == null)
            {
                teacherProfile = new Teacherprofile
                {
                    TeacherId = userId,
                    TeacherName = request.TeacherName ?? "",
                    Description = request.Description ?? "",
                    Experience = request.Experience,
                };
            }
            else
            {
                if (!string.IsNullOrEmpty(request.TeacherName))
                {
                    teacherProfile.TeacherName = request.TeacherName;
                }
                if (!string.IsNullOrEmpty(request.Description))
                {
                    teacherProfile.Description = request.Description;
                }
                if (!string.IsNullOrEmpty(request.Experience.ToString()))
                {
                    teacherProfile.Experience = request.Experience;
                }
            }

            if (request.Avatar == null || request.Avatar.Length == 0)
            {
                teacherProfile.AvatarUrl = "/avatar/da98cf74-c30c-4212-8fbe-cbf10af0b096.jpg";
            }
            else
            {
                teacherProfile.AvatarUrl = await FileUploadHelper.UploadAvatarAsync(request.Avatar, _webHostEnvironment.WebRootPath);
            }

            var response = await _teacherProfileRepository.AddOrUpdateByColumnAsync(teacherProfile, x => x.TeacherId == userId);

            return new BaseResponse<TeacherProfileResponse>(
                "Tạo hồ sơ giáo viên thành công.",
                StatusCodeEnum.OK_200,
                _mapper.Map<TeacherProfileResponse>(response)
                );
        }
    }
}
