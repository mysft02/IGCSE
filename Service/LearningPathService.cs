using AutoMapper;
using BusinessObject.DTOs.Request.LearningPaths;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.LearningPaths;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.Model;
using Common.Constants;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Org.BouncyCastle.Asn1.Ocsp;
using Repository.IRepositories;

namespace Service
{
    public class LearningPathService
    {
        private readonly ILearningPathRepository _learningPathRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly MediaService _mediaService;

        public LearningPathService(ILearningPathRepository learningPathRepository, IModuleRepository moduleRepository, IMapper mapper, ICourseRepository courseRepository, MediaService mediaService)
        {
            _learningPathRepository = learningPathRepository;
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _mediaService = mediaService;
        }

        public async Task<BaseResponse<LearningPathResponse>> CreateLearningPathAsync(LearningPathCreateRequest request)
        {
            if(request.ModuleId == null)
            {
                throw new Exception("Không có id module.");
            }

            string moduleList = "";
            foreach(var item in request.ModuleId)
            {
                var module = await _moduleRepository.FindOneAsync(x => x.ModuleID == item);
                if (module == null)
                {
                    throw new Exception($"không tìm thấy module {item}");
                }
                moduleList += item.ToString() + "_";
            }

            var learningPath = new Learningpath
            {
                Title = request.Title,
                Description = request.Description,
                ModulePath = moduleList.TrimEnd('_'),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var query = await _learningPathRepository.AddAsync(learningPath);
            var result = _mapper.Map<LearningPathResponse>(query);

            return new BaseResponse<LearningPathResponse>(
                "Tạo lộ trình học tập thành công.",
                StatusCodeEnum.OK_200,
                result
                );
        }

        public async Task<BaseResponse<LearningPathResponse>> GetLearningPath(int id)
        {
            var learningPath = await _learningPathRepository.FindOneAsync(x => x.LearningPathId == id);
            if (learningPath == null)
            {
                throw new Exception("Không tìm thấy lộ trình học tập tìm kiếm.");
            }
            var result = _mapper.Map<LearningPathResponse>(learningPath);

            result.ModuleResponses = new List<ModuleWithCourseResponse>();
            List<int> moduleIdList = learningPath.ModulePath.TrimEnd('_').Split('_').Select(int.Parse).ToList();
            foreach ( var moduleId in moduleIdList)
            {
                var module = await _moduleRepository.FindOneWithIncludeAsync(x => x.ModuleID == moduleId, c => c.Courses);
                var moduleMapper = _mapper.Map<ModuleWithCourseResponse>(module);
                moduleMapper.Course = _mapper.Map<List<CourseModuleResponse>>(module?.Courses);
                moduleMapper.Course = moduleMapper.Course.Take(3);
                foreach (var course in moduleMapper.Course)
                {
                    course.ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(course.ImageUrl);
                }
                result.ModuleResponses.Add(moduleMapper);
            }

            return new BaseResponse<LearningPathResponse>(
                "Tạo lộ trình học tập thành công.",
                StatusCodeEnum.OK_200,
                result
                );
        }

        public async Task<BaseResponse<PaginatedResponse<LearningPathResponse>>> GetAllLearningPath(LearningPathQuery query)
        {
            var learningPath = await _learningPathRepository.GetAllAsync();
            var result = _mapper.Map<IEnumerable<LearningPathResponse>>(learningPath);
            var pagedItem = result
                .Skip(query.Page * query.Size)
                .Take(query.Size)
                .ToList();

            var totalCount = result.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / query.Size);

            return new BaseResponse<PaginatedResponse<LearningPathResponse>>
            {
                Data = new PaginatedResponse<LearningPathResponse>
                {
                    Items = pagedItem,
                    TotalCount = totalCount,
                    Page = query.Page,
                    Size = query.Size,
                    TotalPages = totalPages
                },
                Message = "Lấy toàn bộ lộ trình học tập thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }
    }
}
