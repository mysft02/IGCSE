using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Service.Background.Interfaces;

namespace Service.Background.Executors;

/// <summary>
/// Executor sử dụng queue để xử lý background tasks
/// Tasks được queue lại và xử lý bởi QueuedHostedService
/// </summary>
public class QueueBackgroundExecutor : IBackgroundExecutor
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> _queue;
    private readonly ILogger<QueueBackgroundExecutor>? _logger;

    public string Name => "default";

    public QueueBackgroundExecutor(int capacity = 100, ILogger<QueueBackgroundExecutor>? logger = null)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, ValueTask>>(options);
        _logger = logger;
    }

    public async Task ExecuteAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        CancellationToken cancellationToken = default)
    {
        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem));

        await _queue.Writer.WriteAsync(workItem, cancellationToken);
        _logger?.LogDebug("Task queued to {ExecutorName} executor", Name);
    }

    /// <summary>
    /// Dequeue một work item từ queue (dùng bởi QueuedHostedService)
    /// </summary>
    public async ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}

