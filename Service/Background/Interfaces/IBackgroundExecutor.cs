namespace Service.Background.Interfaces;

/// <summary>
/// Interface cho background executor
/// </summary>
public interface IBackgroundExecutor
{
    /// <summary>
    /// Tên của executor
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Thực thi work item trong background
    /// </summary>
    /// <param name="workItem">Work item cần thực thi</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem, CancellationToken cancellationToken = default);
}

