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

    public ContextManager(
        IAccountRepository accountRepository,
        ICourseRepository courseRepository,
        IProcessRepository processRepository,
        IChatMessageRepository messageRepository,
        UserManager<Account> userManager)
    {
        _accountRepository = accountRepository;
        _courseRepository = courseRepository;
        _processRepository = processRepository;
        _messageRepository = messageRepository;
        _userManager = userManager;
    }

    public async Task<ChatContext> BuildContextAsync(string userId)
    {
        var context = new ChatContext
        {
            UserId = userId
        };

        // Get user role
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            context.UserRole = roles.FirstOrDefault() ?? "Student";
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

        return context;
    }
}

