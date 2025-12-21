using BusinessObject.DTOs.Response.Assistant;

namespace Service.Assistant;

public interface ISystemDataService
{
    /// <summary>
    /// Lấy system data context (text) cho AI
    /// </summary>
    Task<string> GetSystemDataContextAsync(string userId, string userRole, string userQuery);
    
    /// <summary>
    /// Lấy structured system data để frontend render đẹp
    /// </summary>
    Task<List<SystemDataResponse>> GetStructuredSystemDataAsync(string userId, string userRole, string userQuery);
}

