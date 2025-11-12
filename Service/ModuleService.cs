using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.Enums;
using BusinessObject.Model;
using Common.Constants;
using Common.Utils;
using Repository.IRepositories;
using Service.OpenAI;

namespace Service
{
    public class ModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly OpenAIEmbeddingsApiService _embeddingsService;

        public ModuleService(
            IModuleRepository moduleRepository,
            IMapper mapper,
            OpenAIEmbeddingsApiService embeddingsService)
        {
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _embeddingsService = embeddingsService;
        }

        public async Task<BaseResponse<PaginatedResponse<ModuleResponse>>> GetModulesPagedAsync(ModuleListQuery query)
        {
            try
            {
                var (modules, total) = await _moduleRepository.SearchAsync(
                    query.Page,
                    query.PageSize,
                    query.SearchByName,
                    query.CourseSubject,
                    query.IsActive
                );

                if (modules == null || !modules.Any())
                {
                    return new BaseResponse<PaginatedResponse<ModuleResponse>>(
                        "No modules found",
                        StatusCodeEnum.OK_200,
                        new PaginatedResponse<ModuleResponse>
                        {
                            Items = new List<ModuleResponse>(),
                            TotalCount = 0,
                            Page = query.Page,
                            Size = query.PageSize,
                            TotalPages = 0
                        }
                    );
                }

                var moduleResponses = _mapper.Map<List<ModuleResponse>>(modules);

                var paginatedResponse = new PaginatedResponse<ModuleResponse>
                {
                    Items = moduleResponses,
                    TotalCount = total,
                    Page = query.Page,
                    Size = query.PageSize,
                    TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
                };

                return new BaseResponse<PaginatedResponse<ModuleResponse>>(
                    $"Successfully retrieved {moduleResponses.Count} modules out of {total} total",
                    StatusCodeEnum.OK_200,
                    paginatedResponse
                );
            }
            catch (Exception ex)
            {
                return new BaseResponse<PaginatedResponse<ModuleResponse>>(
                    "An error occurred while retrieving modules: " + ex.Message,
                    StatusCodeEnum.InternalServerError_500,
                    null
                );
            }
        }

        public async Task<BaseResponse<ModuleResponse>> GetModuleByIdAsync(int moduleId)
        {
            try
            {
                var module = await _moduleRepository.GetByIdAsync(moduleId);
                if (module == null)
                {
                    return new BaseResponse<ModuleResponse>(
                        $"Module with ID {moduleId} not found",
                        StatusCodeEnum.NotFound_404,
                        null
                    );
                }
                
                var response = _mapper.Map<ModuleResponse>(module);
                
                return new BaseResponse<ModuleResponse>(
                    "Module retrieved successfully",
                    StatusCodeEnum.OK_200,
                    response
                );
            }
            catch (Exception ex)
            {
                return new BaseResponse<ModuleResponse>(
                    $"An error occurred while retrieving module with ID {moduleId}: {ex.Message}",
                    StatusCodeEnum.InternalServerError_500,
                    null
                );
            }
        }

        public async Task<BaseResponse<ModuleResponse>> CreateModuleAsync(ModuleRequest request)
        {
            try
            {
                var module = _mapper.Map<Module>(request);
                
                var now = DateTime.UtcNow;
                module.CreatedAt = now;
                module.UpdatedAt = now;
                
                var createdModule = await _moduleRepository.AddAsync(module);
                var response = _mapper.Map<ModuleResponse>(createdModule);
                
                return new BaseResponse<ModuleResponse>(
                    "Module created successfully",
                    StatusCodeEnum.Created_201,
                    response
                );
            }
            catch (Exception ex)
            {
                return new BaseResponse<ModuleResponse>(
                    $"An error occurred while creating the module: {ex.Message}",
                    StatusCodeEnum.InternalServerError_500,
                    null
                );
            }
        }

        public async Task<BaseResponse<ModuleResponse>> UpdateModuleAsync(int moduleId, ModuleRequest request)
        {
            try
            {
                var existingModule = await _moduleRepository.GetByIdAsync(moduleId);
                if (existingModule == null)
                {
                    return new BaseResponse<ModuleResponse>(
                        $"Module with ID {moduleId} not found",
                        StatusCodeEnum.NotFound_404,
                        null
                    );
                }

                existingModule.ModuleName = request.ModuleName;
                existingModule.Description = request.Description;
                existingModule.IsActive = request.IsActive;
                existingModule.UpdatedAt = DateTime.UtcNow;
                existingModule.CourseSubject = request.CourseSubject;
                
                var updatedModule = await _moduleRepository.UpdateAsync(existingModule);
                
                return await GetModuleByIdAsync(updatedModule.ModuleID);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the module with ID {moduleId}: {ex.Message}");
            }
        }

        public async Task DeleteModuleAsync(int moduleId)
        {
            await _moduleRepository.DeleteAsync(moduleId);
        }
        public async Task<BaseResponse<List<ModuleResponse>>> GetModulesBySubjectAsync(CourseSubject courseSubject)
        {
            try
            {
                var modules = await _moduleRepository.GetByCourseSubjectAsync(courseSubject);
                if (modules == null || !modules.Any())
                {
                    return new BaseResponse<List<ModuleResponse>>(
                        $"No modules found for subject {courseSubject}",
                        StatusCodeEnum.NotFound_404,
                        new List<ModuleResponse>()
                    );
                }

                var moduleResponses = _mapper.Map<List<ModuleResponse>>(modules);
                
                return new BaseResponse<List<ModuleResponse>>(
                    $"Modules for subject {courseSubject} retrieved successfully",
                    StatusCodeEnum.OK_200,
                    moduleResponses
                );
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<ModuleResponse>>(
                    $"An error occurred while retrieving modules for subject {courseSubject}: {ex.Message}",
                    StatusCodeEnum.InternalServerError_500,
                    null
                );
            }
        }

        public async Task<List<ModuleResponse>> GetModulesByCourseIdAsync(int courseId)
        {
            var modules = await _moduleRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<List<ModuleResponse>>(modules);
        }
    }
}
