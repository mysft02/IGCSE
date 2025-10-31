using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.Model;
using Repository.IRepositories;
using AutoMapper;

namespace Service
{
    public class ModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;
        public ModuleService(IModuleRepository moduleRepository, IMapper mapper)
        {
            _moduleRepository = moduleRepository;
            _mapper = mapper;
        }
        public async Task<List<ModuleResponse>> GetModulesByCourseIdAsync(int courseId)
        {
            var modules = await _moduleRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<List<ModuleResponse>>(modules);
        }
        public async Task<ModuleResponse?> GetModuleByIdAsync(int moduleId)
        {
            var module = await _moduleRepository.GetByIdAsync(moduleId);
            return _mapper.Map<ModuleResponse>(module);
        }
        public async Task<ModuleResponse> CreateModuleAsync(ModuleRequest request)
        {
            var module = _mapper.Map<Module>(request);
            module.CreatedAt = DateTime.UtcNow;
            module.UpdatedAt = DateTime.UtcNow;
            var created = await _moduleRepository.AddAsync(module);
            return _mapper.Map<ModuleResponse>(created);
        }
        public async Task<ModuleResponse> UpdateModuleAsync(int moduleId, ModuleRequest request)
        {
            var module = await _moduleRepository.GetByIdAsync(moduleId);
            if (module == null) throw new Exception("Module not found");
            module.ModuleName = request.ModuleName;
            module.Description = request.Description;
            module.IsActive = request.IsActive;
            module.UpdatedAt = DateTime.UtcNow;
            var updated = await _moduleRepository.UpdateAsync(module);
            return _mapper.Map<ModuleResponse>(updated);
        }
        public async Task DeleteModuleAsync(int moduleId)
        {
            await _moduleRepository.DeleteAsync(moduleId);
        }
    }
}
