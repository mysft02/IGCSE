namespace Service.Background.Interfaces;

/// <summary>
/// Registry để quản lý các background executors
/// </summary>
public interface IBackgroundExecutorRegistry
{
    /// <summary>
    /// Lấy executor theo tên
    /// </summary>
    IBackgroundExecutor GetExecutor(string name);
    
    /// <summary>
    /// Đăng ký executor
    /// </summary>
    void RegisterExecutor(string name, IBackgroundExecutor executor);
}

