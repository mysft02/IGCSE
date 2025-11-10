using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Background.Attributes;
using Service.Background.Interfaces;

namespace Service.Background;

/// <summary>
/// Invoker để execute methods được đánh dấu với BackgroundTask attribute
/// </summary>
public class BackgroundTaskInvoker : IBackgroundTaskInvoker
{
    private readonly IBackgroundExecutorRegistry _executorRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundTaskInvoker> _logger;

    public BackgroundTaskInvoker(
        IBackgroundExecutorRegistry executorRegistry,
        IServiceProvider serviceProvider,
        ILogger<BackgroundTaskInvoker> logger)
    {
        _executorRegistry = executorRegistry;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Invoke method trong background (synchronous wrapper)
    /// </summary>
    public void InvokeBackgroundTask<T>(T service, string methodName, params object[] args) where T : class
    {
        InvokeBackgroundTaskAsync<T>(service, methodName, args).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Invoke method trong background (asynchronous)
    /// </summary>
    public async Task InvokeBackgroundTaskAsync<T>(T service, string methodName, params object[] args) where T : class
    {
        var serviceType = typeof(T);
        var method = serviceType.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (method == null)
        {
            throw new InvalidOperationException(
                $"Method '{methodName}' not found in {serviceType.Name}. " +
                "Make sure the method name is correct and accessible."
            );
        }

        var attribute = method.GetCustomAttribute<BackgroundTaskAttribute>();
        if (attribute == null)
        {
            throw new InvalidOperationException(
                $"Method '{methodName}' in {serviceType.Name} must have [BackgroundTask] attribute " +
                "to be executed in background."
            );
        }

        var executor = _executorRegistry.GetExecutor(attribute.ExecutorName);

        _logger.LogDebug(
            "Invoking method '{MethodName}' from {ServiceType} in background using executor '{ExecutorName}'",
            methodName,
            serviceType.Name,
            attribute.ExecutorName
        );

        await executor.ExecuteAsync(async (sp, ct) =>
        {
            try
            {
                // Resolve service từ scope mới
                var scopedService = sp.GetRequiredService<T>();

                // Invoke method
                var result = method.Invoke(scopedService, args);

                // Handle async results
                if (result is Task task)
                {
                    await task;
                }
                else if (result is ValueTask valueTask)
                {
                    await valueTask;
                }
                else if (result is Task<object> taskWithResult)
                {
                    await taskWithResult;
                }

                _logger.LogDebug(
                    "Successfully executed background method '{MethodName}' from {ServiceType}",
                    methodName,
                    serviceType.Name
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing background method '{MethodName}' from {ServiceType}",
                    methodName,
                    serviceType.Name
                );
                throw;
            }
        });
    }
}

