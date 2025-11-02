namespace Service.Background.Interfaces;

/// <summary>
/// Interface để invoke method với BackgroundTask attribute
/// </summary>
public interface IBackgroundTaskInvoker
{
    /// <summary>
    /// Invoke method trong background (synchronous)
    /// </summary>
    void InvokeBackgroundTask<T>(T service, string methodName, params object[] args) where T : class;
    
    /// <summary>
    /// Invoke method trong background (asynchronous)
    /// </summary>
    Task InvokeBackgroundTaskAsync<T>(T service, string methodName, params object[] args) where T : class;
}

