# Background Task System

Hệ thống background task cho phép thực thi các method trong background, tương tự `@Async` trong Spring Boot.

## Kiến trúc

```
Service/Background/
├── Attributes/
│   └── BackgroundTaskAttribute.cs      # Attribute để đánh dấu method
├── Interfaces/
│   ├── IBackgroundExecutor.cs          # Interface cho executor
│   ├── IBackgroundExecutorRegistry.cs  # Registry quản lý executors
│   └── IBackgroundTaskInvoker.cs       # Invoker để gọi methods
├── Executors/
│   ├── QueueBackgroundExecutor.cs     # Executor sử dụng queue
│   └── SyncBackgroundExecutor.cs       # Executor chạy ngay lập tức
├── BackgroundExecutorRegistry.cs       # Implementation của registry
├── BackgroundTaskInvoker.cs             # Implementation của invoker
└── QueuedHostedService.cs               # Hosted service xử lý queue
```

## Cách sử dụng

### 1. Đánh dấu method với [BackgroundTask]

```csharp
using Service.Background.Attributes;

public class TrelloBoardService
{
    [BackgroundTask("syncExecutor")]
    public async Task<List<TrelloBoardResponse>> getTrelloBoard(TrelloToken trelloToken)
    {
        // Logic của method
        return boards;
    }
}
```

**Các executor có sẵn:**

- `"default"` - Queue executor (tasks được queue và xử lý tuần tự)
- `"syncExecutor"` - Sync executor (chạy ngay lập tức trong background thread)

### 2. Gọi method trong background

```csharp
using Service.Background.Interfaces;

public class TrelloTokenService
{
    private readonly IBackgroundTaskInvoker _backgroundTaskInvoker;
    private readonly TrelloBoardService _trelloBoardService;

    public TrelloTokenService(
        IBackgroundTaskInvoker backgroundTaskInvoker,
        TrelloBoardService trelloBoardService)
    {
        _backgroundTaskInvoker = backgroundTaskInvoker;
        _trelloBoardService = trelloBoardService;
    }

    public async Task SyncBoardsAsync(TrelloToken token)
    {
        // Gọi method có [BackgroundTask] trong background
        await _backgroundTaskInvoker.InvokeBackgroundTaskAsync(
            _trelloBoardService,
            nameof(TrelloBoardService.getTrelloBoard),
            token
        );

        // Code tiếp tục chạy ngay lập tức, không đợi getTrelloBoard hoàn thành
        // ... các logic khác ...
    }
}
```

## Cách hoạt động

1. **Đánh dấu method**: Method được đánh dấu với `[BackgroundTask("executorName")]`
2. **Gọi qua Invoker**: Sử dụng `IBackgroundTaskInvoker` để gọi method
3. **Resolve Executor**: System tìm executor theo tên từ registry
4. **Thực thi**: Executor thực thi method trong background
   - **Queue executor**: Thêm vào queue, xử lý bởi `QueuedHostedService`
   - **Sync executor**: Chạy ngay trong `Task.Run`

## Lợi ích

- ✅ Pattern rõ ràng, dễ hiểu
- ✅ Tách biệt concerns (executor vs invoker)
- ✅ Dễ mở rộng với executor mới
- ✅ Tương thích với dependency injection
- ✅ Hỗ trợ scoped services (mỗi task có scope riêng)

## Ví dụ đầy đủ

Xem `TrelloTokenService.SyncTrelloBoardsInBackgroundAsync()` để biết cách sử dụng thực tế.
