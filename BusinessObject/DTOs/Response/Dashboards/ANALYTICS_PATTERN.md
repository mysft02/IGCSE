# Analytics Response Pattern

## Tổng quan

Tất cả Analytics APIs đều follow một pattern chung với 2 thành phần chính:
- **Metadata**: Chứa các thông tin tổng quan (totalCourses, pending, open, etc.)
- **Chart**: Chứa mảng các data points với label và các chỉ số

## Cấu trúc

### BaseAnalyticsResponse<TMetadata, TChartData>

```csharp
public class BaseAnalyticsResponse<TMetadata, TChartData>
    where TMetadata : class
    where TChartData : BaseChartDataPoint
{
    public TMetadata Metadata { get; set; }
    public List<TChartData> Chart { get; set; }
}
```

### BaseChartDataPoint

```csharp
public abstract class BaseChartDataPoint
{
    public string Label { get; set; } // Label hiển thị (bắt buộc)
}
```

## Ví dụ sử dụng

### 1. Teacher Analytics

```csharp
public class TeacherAnalyticsResponse : BaseAnalyticsResponse<TeacherAnalyticsMetadata, IncomeChartDataPoint>
{
}

// Metadata
public class TeacherAnalyticsMetadata
{
    public CourseCountMetadata Course { get; set; }
}

// Chart Data Point
public class IncomeChartDataPoint : BaseChartDataPoint
{
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal ReceivedIncome { get; set; }
}
```

**Response:**
```json
{
  "metadata": {
    "course": {
      "totalCourses": 40,
      "pending": 9,
      "open": 21,
      "rejected": 9,
      "draft": 1
    }
  },
  "chart": [
    {
      "label": "Tháng 1",
      "month": 1,
      "totalIncome": 0,
      "receivedIncome": 0
    }
  ]
}
```

### 2. Lesson Completion Analytics

```csharp
public class LessonCompletionResponse : BaseAnalyticsResponse<LessonCompletionMetadata, LessonCompletionChartDataPoint>
{
}

// Metadata
public class LessonCompletionMetadata
{
    public string TimeUnit { get; set; }
    public int TotalCompletedUsers { get; set; }
    public int TotalCompletedLessonItems { get; set; }
}

// Chart Data Point
public class LessonCompletionChartDataPoint : BaseChartDataPoint
{
    public int CompletedUsersCount { get; set; }
    public int CompletedLessonItemsCount { get; set; }
}
```

**Response:**
```json
{
  "metadata": {
    "timeUnit": "Month",
    "totalCompletedUsers": 30,
    "totalCompletedLessonItems": 270
  },
  "chart": [
    {
      "label": "2024-01",
      "completedUsersCount": 5,
      "completedLessonItemsCount": 30
    }
  ]
}
```

## Lợi ích

1. **Consistency**: Tất cả analytics APIs có cùng structure
2. **Type Safety**: Sử dụng generics để đảm bảo type safety
3. **Extensibility**: Dễ dàng thêm analytics mới chỉ cần extend base class
4. **Frontend Friendly**: Structure rõ ràng, dễ render charts và metadata

## Cách tạo Analytics mới

1. Tạo Metadata class (extend từ base hoặc tạo mới)
2. Tạo ChartDataPoint class (extend từ `BaseChartDataPoint`)
3. Tạo Response class (extend từ `BaseAnalyticsResponse<TMetadata, TChartData>`)
4. Implement repository method trả về response mới
5. Update service và controller

## Best Practices

1. **Metadata**: Chứa các thông tin tổng quan, không phải array
2. **Chart**: Chứa array các data points, mỗi point có label và các chỉ số
3. **Label**: Luôn có và phải rõ ràng, dễ hiểu
4. **Naming**: Đặt tên rõ ràng, dễ hiểu (ví dụ: `CompletedUsersCount` thay vì `Count`)

