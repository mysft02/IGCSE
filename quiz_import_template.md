# 📋 Hướng dẫn Import Quiz từ Excel

## 🎯 Format file Excel mẫu

File Excel phải có **2 cột** với format như sau:

| A | B |
|---|---|
| **Question Content** | **Correct Answer** |
| Câu hỏi 1? | Đáp án A |
| Câu hỏi 2? | Đáp án B |
| ... | ... |

## 📝 Ví dụ cụ thể:

### Sheet 1: Questions
| A | B |
|---|---|
| **Question Content** | **Correct Answer** |
| Thủ đô của Việt Nam là gì? | Hà Nội |
| Số nguyên tố nhỏ nhất là? | 2 |
| Kim tự tháp Giza nằm ở đâu? | Ai Cập |
| Tác giả của "Những người khốn khổ" là ai? | Victor Hugo |
| 2 + 2 = ? | 4 |

## 🔧 Cách sử dụng API:

### 1. Request Format:
```
POST /api/quiz/import-from-excel
Content-Type: multipart/form-data

CourseId: 1
QuizTitle: "Quiz Toán học cơ bản"
QuizDescription: "Quiz về kiến thức toán học cơ bản"
ExcelFile: [file Excel]
```

### 2. Yêu cầu:
- **File Excel**: Chỉ chấp nhận .xlsx hoặc .xls
- **Cột A**: Nội dung câu hỏi (bắt buộc)
- **Cột B**: Đáp án đúng (bắt buộc)
- **Row 1**: Header (sẽ bị bỏ qua)
- **Row 2 trở đi**: Dữ liệu câu hỏi
```

## ⚠️ Lưu ý quan trọng:

1. **Row đầu tiên** phải là header (sẽ bị bỏ qua)
2. **Không được để trống** nội dung câu hỏi hoặc đáp án
3. **File phải có ít nhất 2 rows** (1 header + 1 data)
4. **Chỉ sử dụng cột A và B** - các cột khác sẽ bị bỏ qua
5. **Encoding**: Sử dụng UTF-8 để tránh lỗi tiếng Việt

## 📊 Ví dụ file Excel hoàn chỉnh:

```
| A                                          | B                    |
|--------------------------------------------|----------------------|
| Question Content                           | Correct Answer       |
| Thủ đô của Việt Nam là gì?                 | Hà Nội               |
| Số nguyên tố nhỏ nhất là?                  | 2                    |
| Kim tự tháp Giza nằm ở đâu?                | Ai Cập               |
| Tác giả của "Những người khốn khổ" là ai?  | Victor Hugo          |
| 2 + 2 = ?                                  | 4                    |
| Màu của lá cây thường là gì?               | Xanh lá              |
| Con vật nào được gọi là "chúa sơn lâm"?    | Sư tử                |
```

