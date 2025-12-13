# Rule-Based Pre-Filtering Pattern

## Tổng quan

**Rule-Based Pre-Filtering Pattern** là một pattern được sử dụng để giảm tải việc gọi API OpenAI bằng cách lọc và chấm điểm các câu trả lời đơn giản, rõ ràng trước khi gửi đến AI. Pattern này giúp:

- **Giảm 30-50% số lần gọi OpenAI API**: Các câu trả lời trùng khớp hoặc tương tự cao được xử lý ngay lập tức
- **Tăng tốc độ phản hồi**: Rule-based check chỉ mất < 1ms so với 500-2000ms cho API call
- **Tiết kiệm chi phí**: Không phải trả tiền cho các câu hỏi đơn giản
- **Đảm bảo độ chính xác**: Exact match và fuzzy match với threshold cao luôn chính xác

## Vấn đề cần giải quyết

### Vấn đề ban đầu

Hệ thống chấm điểm quiz hiện tại gọi OpenAI API cho **tất cả** câu trả lời, kể cả những câu:

- Trả lời trùng khớp hoàn toàn với đáp án đúng
- Trả lời chứa đáp án đúng (ví dụ: "Đáp án là A" vs "A")
- Trả lời tương tự > 90% với đáp án đúng

Điều này dẫn đến:

- **Lãng phí chi phí**: Trả tiền cho những câu hỏi có thể chấm bằng rule đơn giản
- **Chậm**: Phải chờ API response cho tất cả câu hỏi
- **Không tối ưu**: Không tận dụng được các pattern matching cơ bản

### Giải pháp

Rule-Based Pre-Filtering Pattern giải quyết bằng cách:

1. **Phân loại câu hỏi**: Chia thành 2 nhóm - có thể chấm bằng rule và cần AI
2. **Xử lý song song**: Rule-based cho nhóm đơn giản, AI cho nhóm phức tạp
3. **Kết hợp kết quả**: Merge kết quả từ cả 2 nhóm

## Kiến trúc Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                    User Submit Quiz                          │
└───────────────────────────────┬─────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│         Rule-Based Pre-Filtering Layer                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 1. Exact Match Check                                  │   │
│  │    - Case-insensitive comparison                     │   │
│  │    - Trim whitespace                                 │   │
│  │    → Return immediately if match                     │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 2. Contains Match Check                               │   │
│  │    - User answer contains correct answer?            │   │
│  │    - Correct answer contains user answer?             │   │
│  │    → Return immediately if match                      │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 3. Fuzzy Match Check (Levenshtein Distance)           │   │
│  │    - Calculate string similarity                      │   │
│  │    - Threshold: 90% (0.9)                            │   │
│  │    → Return immediately if similarity ≥ 90%          │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 4. Decision Point                                     │   │
│  │    - Can use rule-based? → Rule-Based Scoring        │   │
│  │    - Need AI? → Add to AI queue                      │   │
│  └──────────────────────────────────────────────────────┘   │
└───────────────────────────┬──────────────────────────────────┘
                            │
            ┌───────────────┴───────────────┐
            │                               │
            ▼                               ▼
┌───────────────────────┐      ┌──────────────────────────┐
│  Rule-Based Scoring   │      │   AI Scoring (OpenAI)    │
│  - Instant response   │      │   - Complex answers      │
│  - No API call        │      │   - Semantic analysis    │
│  - 100% accurate      │      │   - Contextual feedback   │
└───────────┬───────────┘      └──────────────┬───────────┘
            │                                 │
            └───────────────┬─────────────────┘
                            │
                            ▼
            ┌───────────────────────────────┐
            │   Merge Results               │
            │   - Combine rule-based + AI   │
            │   - Calculate final score     │
            │   - Return to user            │
            └───────────────────────────────┘
```

## Fuzzy Matching (Levenshtein Distance)

### Fuzzy Matching là gì?

**Fuzzy Matching** (so khớp mờ) là kỹ thuật so sánh hai chuỗi văn bản để xác định mức độ tương đồng giữa chúng, ngay cả khi chúng không hoàn toàn giống nhau.

### Vấn đề Fuzzy Matching giải quyết

Trong thực tế, học viên có thể trả lời:

- **Có lỗi chính tả nhỏ**: "Hà Nội" vs "Ha Noi"
- **Thêm/bớt khoảng trắng**: "Đáp án A" vs "Đáp án A"
- **Viết tắt**: "HCM" vs "Hồ Chí Minh"
- **Thứ tự từ khác**: "A và B" vs "B và A"

Fuzzy Matching giúp nhận diện những trường hợp này vẫn là **đúng** hoặc **gần đúng**.

### Levenshtein Distance

**Levenshtein Distance** (còn gọi là Edit Distance) là số lần thao tác tối thiểu cần thiết để chuyển đổi một chuỗi này sang chuỗi kia.

#### Các thao tác:

1. **Insert** (Chèn): Thêm một ký tự
2. **Delete** (Xóa): Xóa một ký tự
3. **Substitute** (Thay thế): Thay một ký tự bằng ký tự khác

#### Ví dụ:

```
Chuỗi 1: "Hà Nội"
Chuỗi 2: "Ha Noi"

Levenshtein Distance = 2
- Thay "à" → "a" (1 operation)
- Thay "ộ" → "o" (1 operation)
```

```
Chuỗi 1: "Đáp án A"
Chuỗi 2: "Đáp án  A"  (có 2 khoảng trắng)

Levenshtein Distance = 1
- Insert thêm 1 khoảng trắng (1 operation)
```

#### Công thức tính Similarity:

```
Similarity = 1 - (Levenshtein Distance / Max Length)
```

**Ví dụ:**

- Chuỗi 1: "Hà Nội" (6 ký tự)
- Chuỗi 2: "Ha Noi" (6 ký tự)
- Levenshtein Distance = 2
- Max Length = 6
- **Similarity = 1 - (2/6) = 0.67 (67%)**

### Ngưỡng Threshold

Trong pattern này, chúng ta sử dụng **threshold = 0.9 (90%)**:

- **Similarity ≥ 90%**: Coi là đúng, sử dụng rule-based scoring
- **Similarity < 90%**: Cần AI để đánh giá chính xác hơn

**Lý do chọn 90%:**

- Đủ cao để đảm bảo độ chính xác
- Đủ thấp để bắt được các trường hợp có lỗi chính tả nhỏ
- Cân bằng giữa false positive và false negative

## Các bước kiểm tra trong Pattern

### Bước 1: Exact Match (Trùng khớp hoàn toàn)

**Mục đích**: Kiểm tra xem câu trả lời có trùng khớp hoàn toàn với đáp án đúng không.

**Cách thực hiện**:

1. Normalize: Trim whitespace, lowercase
2. So sánh case-insensitive
3. Nếu match → Return `(CanUseRuleBased: true, IsCorrect: true, Similarity: 1.0)`

**Ví dụ**:

```
User Answer: "Hà Nội"
Correct Answer: "Hà Nội"
→ Exact Match → IsCorrect: true
```

### Bước 2: Contains Match (Chứa đựng)

**Mục đích**: Kiểm tra xem một chuỗi có chứa chuỗi kia không.

**Cách thực hiện**:

1. **User answer chứa correct answer**:

   - Ví dụ: "Đáp án là Hà Nội" chứa "Hà Nội"
   - → Return `(CanUseRuleBased: true, IsCorrect: true, Similarity: 0.95)`

2. **Correct answer chứa user answer**:
   - Ví dụ: "Hà Nội" chứa "Hà" (nhưng chỉ coi đúng nếu user answer ≥ 50% độ dài)
   - → Return `(CanUseRuleBased: true, IsCorrect: true, Similarity: 0.9)`

**Ví dụ**:

```
User Answer: "Đáp án là A"
Correct Answer: "A"
→ Contains Match → IsCorrect: true
```

### Bước 3: Fuzzy Match (Levenshtein Similarity)

**Mục đích**: Tính độ tương đồng giữa hai chuỗi dựa trên Levenshtein Distance.

**Cách thực hiện**:

1. Normalize: Trim, lowercase
2. Tính Levenshtein Distance
3. Tính Similarity = 1 - (Distance / Max Length)
4. Nếu Similarity ≥ 0.9 → Return `(CanUseRuleBased: true, IsCorrect: true, Similarity: calculated)`

**Ví dụ**:

```
User Answer: "Ha Noi"
Correct Answer: "Hà Nội"
Levenshtein Distance = 2
Max Length = 6
Similarity = 1 - (2/6) = 0.67 (67%)
→ Similarity < 90% → Need AI
```

```
User Answer: "Hà Nội"
Correct Answer: "Hà Nội "
Levenshtein Distance = 1
Max Length = 7
Similarity = 1 - (1/7) = 0.86 (86%)
→ Similarity < 90% → Need AI (nhưng gần threshold)
```

### Bước 4: Decision Point

**Mục đích**: Quyết định xem có thể sử dụng rule-based scoring hay cần AI.

**Logic**:

- Nếu bất kỳ bước nào (1, 2, hoặc 3) trả về `CanUseRuleBased: true` → Sử dụng rule-based
- Nếu tất cả đều trả về `CanUseRuleBased: false` → Cần AI

## Implementation trong IGCSE

### Utility Methods

Pattern được implement trong `CommonUtils.cs`:

```csharp
// Tính Levenshtein Distance
public static int LevenshteinDistance(string s1, string s2)

// Tính Similarity Ratio
public static decimal CalculateStringSimilarity(string s1, string s2)

// Kiểm tra có thể dùng Rule-Based Scoring không
public static (bool CanUseRuleBased, bool IsCorrect, decimal Similarity)
    CanUseRuleBasedScoring(string userAnswer, string correctAnswer, decimal similarityThreshold = 0.9m)
```

### Sử dụng trong QuizService

```csharp
// Rule-Based Pre-Filtering
var (canUseRuleBased, isCorrect, similarity) = CommonUtils.CanUseRuleBasedScoring(
    userAnswerText,
    correctAnswer,
    similarityThreshold: 0.9m);

if (canUseRuleBased)
{
    // Sử dụng Rule-Based Scoring (không cần gọi OpenAI)
    var comment = isCorrect
        ? "Câu trả lời chính xác."
        : $"Câu trả lời chưa chính xác. Đáp án đúng: {correctAnswer}";

    ruleBasedResults.Add((questionId, questionText, answer, correctAnswer, isCorrect, similarity, comment));
}
else
{
    // Cần AI để chấm điểm
    questionsNeedingAI.Add((questionId, questionRequest));
}
```

## Lợi ích và Hiệu quả

### Giảm tải API Calls

**Ước tính giảm tải**:

- **Exact Match**: 20-30% câu hỏi (giảm 20-30% calls)
- **Contains Match**: 10-15% câu hỏi (giảm thêm 10-15% calls)
- **Fuzzy Match (≥90%)**: 5-10% câu hỏi (giảm thêm 5-10% calls)

**Tổng cộng**: Giảm **30-50%** số lần gọi OpenAI API

### Tăng tốc độ phản hồi

- **Rule-Based Check**: < 1ms
- **OpenAI API Call**: 500-2000ms

**Cải thiện**: Giảm thời gian phản hồi trung bình từ 1000ms xuống **500-700ms**

### Tiết kiệm chi phí

- **Chi phí OpenAI**: ~$0.001 per request (gpt-4o-mini)
- **Giảm 30-50% calls**: Tiết kiệm **30-50% chi phí**

**Ví dụ**:

- 1000 quiz submissions/ngày
- 10 câu hỏi/quiz
- 30% giảm tải
- **Tiết kiệm**: 1000 × 10 × 0.3 × $0.001 = **$3/ngày** = **$90/tháng**

## Best Practices

### 1. Chọn Threshold phù hợp

- **Quá cao (95%)**: Bỏ sót nhiều câu đúng, vẫn phải gọi AI
- **Quá thấp (80%)**: Có thể chấm sai, giảm độ tin cậy
- **Khuyến nghị**: **90%** là điểm cân bằng tốt

### 2. Normalize trước khi so sánh

- Trim whitespace
- Lowercase (hoặc case-insensitive comparison)
- Loại bỏ ký tự đặc biệt không cần thiết

### 3. Logging và Monitoring

- Log số lượng câu hỏi được xử lý bằng rule-based vs AI
- Monitor tỷ lệ false positive/negative
- Điều chỉnh threshold dựa trên dữ liệu thực tế

### 4. Fallback Strategy

- Nếu rule-based không chắc chắn → Luôn gọi AI
- Ưu tiên độ chính xác hơn tốc độ

## So sánh với các Pattern khác

| Pattern                      | Giảm tải | Tốc độ               | Độ chính xác               | Chi phí    |
| ---------------------------- | -------- | -------------------- | -------------------------- | ---------- |
| **Rule-Based Pre-Filtering** | 30-50%   | Rất nhanh (<1ms)     | Cao (100% cho exact match) | Thấp       |
| **Cache Pattern**            | 60-80%   | Nhanh (10-50ms)      | Cao                        | Trung bình |
| **Batch Processing**         | 70-90%   | Chậm hơn (chờ batch) | Cao                        | Thấp       |
| **Hybrid (Tất cả)**          | 80-95%   | Nhanh                | Rất cao                    | Thấp       |

## Kết luận

Rule-Based Pre-Filtering Pattern là một pattern hiệu quả để giảm tải API calls bằng cách:

- **Lọc các câu hỏi đơn giản** trước khi gửi đến AI
- **Sử dụng Fuzzy Matching** để nhận diện các trường hợp tương tự
- **Kết hợp với các pattern khác** (Cache, Batch) để tối ưu tối đa

Pattern này đặc biệt phù hợp cho:

- Hệ thống chấm điểm quiz/kiểm tra
- Hệ thống tìm kiếm và so khớp văn bản
- Hệ thống validation và verification
