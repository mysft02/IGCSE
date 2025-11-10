using Microsoft.Extensions.Logging;
using Service.Background.Interfaces;

namespace Service.Background;

/// <summary>
/// Registry để quản lý các background executors
/// </summary>
public class BackgroundExecutorRegistry : IBackgroundExecutorRegistry
{
    private readonly Dictionary<string, IBackgroundExecutor> _executors = new();
    private readonly ILogger<BackgroundExecutorRegistry> _logger;
    private readonly object _lock = new();

    public BackgroundExecutorRegistry(ILogger<BackgroundExecutorRegistry> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lấy executor theo tên, nếu không tìm thấy sẽ dùng default executor
    /// </summary>
    public IBackgroundExecutor GetExecutor(string name)
    {
        lock (_lock)
        {
            if (_executors.TryGetValue(name, out var executor))
            {
                return executor;
            }

            // Fallback về default executor nếu không tìm thấy
            if (_executors.TryGetValue("default", out var defaultExecutor))
            {
                _logger.LogWarning("Executor '{Name}' not found, using default executor", name);
                return defaultExecutor;
            }

            throw new InvalidOperationException(
                $"Executor '{name}' not found and no default executor available. " +
                "Please register at least one executor with name 'default'."
            );
        }
    }

    /// <summary>
    /// Đăng ký executor
    /// </summary>
    public void RegisterExecutor(string name, IBackgroundExecutor executor)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Executor name cannot be null or empty", nameof(name));
        }

        if (executor == null)
        {
            throw new ArgumentNullException(nameof(executor));
        }

        lock (_lock)
        {
            if (_executors.ContainsKey(name))
            {
                _logger.LogWarning("Executor '{Name}' already registered, replacing it", name);
            }
            _executors[name] = executor;
            _logger.LogInformation("Registered background executor: {Name}", name);
        }
    }
}

