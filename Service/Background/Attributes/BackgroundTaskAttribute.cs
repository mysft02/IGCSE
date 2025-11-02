namespace Service.Background.Attributes;

/// <summary>
/// Attribute để đánh dấu method sẽ được thực thi trong background task
/// Tương tự @Async trong Spring Boot
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class BackgroundTaskAttribute : Attribute
{
    /// <summary>
    /// Tên executor để thực thi task này
    /// </summary>
    public string ExecutorName { get; }
    
    /// <summary>
    /// Constructor với executor name
    /// </summary>
    /// <param name="executorName">Tên executor, mặc định là "default"</param>
    public BackgroundTaskAttribute(string executorName = "default")
    {
        ExecutorName = executorName;
    }
}

