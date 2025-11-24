using AutoMapper;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.TeacherProfile;
using Common.Constants;
using Repository.IRepositories;

namespace Service
{
    public class TeacherProfileService
    {
        private readonly ITeacherProfileRepository _teacherProfileRepository;
        private readonly IMapper _mapper;

        public TeacherProfileService(ITeacherProfileRepository teacherProfileRepository, IMapper mapper)
        {
            _teacherProfileRepository = teacherProfileRepository;
            _mapper = mapper;
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
    }
}
