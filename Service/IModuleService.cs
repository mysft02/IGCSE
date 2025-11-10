using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.Enums;

namespace Service
{
    public interface IModuleService
    {
        /// <summary>
        /// Gets all modules with subject information
        /// </summary>
        Task<BaseResponse<List<ModuleResponse>>> GetAllModulesAsync();

        /// <summary>
        /// Gets a module by ID
        /// </summary>
        Task<BaseResponse<ModuleResponse>> GetModuleByIdAsync(int moduleId);

        /// <summary>
        /// Creates a new module with the specified subject
        /// </summary>
        Task<BaseResponse<ModuleResponse>> CreateModuleAsync(ModuleRequest request);

        /// <summary>
        /// Updates an existing module with the specified subject
        /// </summary>
        Task<BaseResponse<ModuleResponse>> UpdateModuleAsync(int moduleId, ModuleRequest request);

        /// <summary>
        /// Deletes a module by ID
        /// </summary>
        Task DeleteModuleAsync(int moduleId);

        /// <summary>
        /// Gets modules by course subject
        /// </summary>
        Task<BaseResponse<List<ModuleResponse>>> GetModulesBySubjectAsync(CourseSubject courseSubject);

        /// <summary>
        /// Gets modules by course ID
        /// </summary>
        Task<List<ModuleResponse>> GetModulesByCourseIdAsync(int courseId);
    }
}
