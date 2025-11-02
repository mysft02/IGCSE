using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Background.Executors;

namespace Service.Background;

/// <summary>
/// Hosted service để xử lý background tasks từ queue
/// </summary>
public class QueuedHostedService : BackgroundService
{
    private readonly QueueBackgroundExecutor _queueExecutor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        QueueBackgroundExecutor queueExecutor,
        IServiceProvider serviceProvider,
        ILogger<QueuedHostedService> logger)
    {
        _queueExecutor = queueExecutor;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _queueExecutor.DequeueAsync(stoppingToken);

                // Tạo scope mới cho mỗi task để có thể inject scoped services
                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        await workItem(scope.ServiceProvider, stoppingToken);
                        _logger.LogDebug("Background task completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing background task");
                        // Có thể thêm retry logic hoặc dead letter queue ở đây
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Service đang shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in QueuedHostedService");
            }
        }

        _logger.LogInformation("Queued Hosted Service is stopping");
    }
}

