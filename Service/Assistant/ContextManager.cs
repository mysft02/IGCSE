using Repository.IRepositories;
using BusinessObject.Model;
using Microsoft.AspNetCore.Identity;

namespace Service.Assistant;

public class ContextManager : IContextManager
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IProcessRepository _processRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly UserManager<Account> _userManager;
    private readonly ISystemDataService _systemDataService;

    public ContextManager(
        IAccountRepository accountRepository,
        ICourseRepository courseRepository,
        IProcessRepository processRepository,
        IChatMessageRepository messageRepository,
        UserManager<Account> userManager,
        ISystemDataService systemDataService)
    {
        _accountRepository = accountRepository;
        _courseRepository = courseRepository;
        _processRepository = processRepository;
        _messageRepository = messageRepository;
        _userManager = userManager;
        _systemDataService = systemDataService;
    }

    public async Task<ChatContext> BuildContextAsync(string userId, string? userRole = null, string? userQuery = null)
    {
        var context = new ChatContext
        {
            UserId = userId
        };

        // Get user role - ưu tiên từ parameter (JWT token), nếu không có thì query database
        if (!string.IsNullOrEmpty(userRole))
        {
            context.UserRole = userRole;
        }
        else
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                context.UserRole = roles.FirstOrDefault() ?? "Student";
            }
            else
            {
                context.UserRole = "Student"; // Default fallback
            }
        }

        // Get current course (most recent enrollment)
        var currentProcess = await _processRepository.FindOneAsync(p => p.StudentId == userId);
        if (currentProcess != null)
        {
            var course = await _courseRepository.GetByCourseIdAsync(currentProcess.CourseId);
            if (course != null)
            {
                context.CurrentCourseId = course.CourseId;
                context.CurrentCourseName = course.Name;
            }
        }

        // Get recent messages for context
        var recentMessages = await _messageRepository.GetRecentMessagesAsync(userId, limit: 5);
        context.RecentMessages = recentMessages
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.Content)
            .ToList();

        // Get system data context if userQuery is provided
        if (!string.IsNullOrEmpty(userQuery) && !string.IsNullOrEmpty(context.UserRole))
        {
            try
            {
                context.SystemDataContext = await _systemDataService.GetSystemDataContextAsync(
                    userId,
                    context.UserRole,
                    userQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting system data context: {ex.Message}");
                // Continue without system data if error
            }
        }

        return context;
    }
}

