using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Background.Interfaces;

namespace Service.Background.Executors;

/// <summary>
/// Executor chạy task ngay lập tức trong background thread
/// Không queue, thích hợp cho tasks cần xử lý nhanh
/// </summary>
public class SyncBackgroundExecutor : IBackgroundExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncBackgroundExecutor> _logger;

    public string Name => "syncExecutor";

    public SyncBackgroundExecutor(
        IServiceProvider serviceProvider,
        ILogger<SyncBackgroundExecutor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        CancellationToken cancellationToken = default)
    {
        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem));

        // Chạy trong Task.Run để không block caller
        _ = Task.Run(async () =>
        {
            // Tạo scope mới cho task
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    _logger.LogDebug("Executing task immediately in {ExecutorName} executor", Name);
                    await workItem(scope.ServiceProvider, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing task in {ExecutorName} executor", Name);
                    // Không throw để không ảnh hưởng đến caller
                }
            }
        }, cancellationToken);
    }
}

