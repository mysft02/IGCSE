# Tài liệu API Quản lý Nội dung Khóa học (Course Content Management)

Tài liệu này mô tả các API endpoint để quản lý nội dung khóa học bao gồm: **Course Sections**, **Lessons**, và **Lesson Items**. Tất cả các API này chỉ dành cho role **Teacher** và có các business rules nghiêm ngặt để đảm bảo tính toàn vẹn dữ liệu.

---

## Mục lục

1. [Tổng quan](#1-tổng-quan)
2. [Business Rules chung](#2-business-rules-chung)
3. [API Course Section](#3-api-course-section)
4. [API Lesson](#4-api-lesson)
5. [API Lesson Item](#5-api-lesson-item)
6. [Ví dụ sử dụng](#6-ví-dụ-sử-dụng)
7. [Xử lý lỗi](#7-xử-lý-lỗi)

---

## 1. Tổng quan

### 1.1. Cấu trúc phân cấp

```
Course
  └── Course Section (Section)
      └── Lesson
          └── Lesson Item (Video, PDF, Image, Text)
```

### 1.2. Base URL

```
/api/course
```

### 1.3. Authentication

Tất cả các API yêu cầu:

- **JWT Token** trong header: `Authorization: Bearer <token>`
- **Role**: `Teacher` (chỉ giáo viên mới có quyền sử dụng)

### 1.4. Response Format

Tất cả các API trả về format chuẩn:

```json
{
  "message": "Success message",
  "statusCode": 200,
  "data": { ... }
}
```

---

## 2. Business Rules chung

### 2.1. Quyền truy cập

- ✅ Chỉ có thể **tạo, cập nhật, xóa** các items thuộc khóa học do **chính mình tạo**
- ❌ Không thể thao tác với khóa học của giáo viên khác

### 2.2. Trạng thái khóa học (Course Status)

Các thao tác **UPDATE** và **DELETE** chỉ được phép khi khóa học ở các trạng thái:

| Status     | Mô tả          | Cho phép Update/Delete? |
| ---------- | -------------- | ----------------------- |
| `Draft`    | Nháp           | ✅ Có                   |
| `Pending`  | Đang chờ duyệt | ✅ Có                   |
| `Rejected` | Bị từ chối     | ✅ Có                   |
| `Open`     | Đã được duyệt  | ❌ **KHÔNG**            |

**Lý do**: Khi khóa học đã được duyệt (`Open`), nội dung đã được công bố cho học viên, việc chỉnh sửa hoặc xóa có thể ảnh hưởng đến trải nghiệm học tập.

### 2.3. Cascade Delete (Xóa dây chuyền)

Khi xóa một item, tất cả các items con sẽ bị xóa tự động:

- **Xóa Section** → Xóa tất cả **Lessons** và **Lesson Items** bên trong
- **Xóa Lesson** → Xóa tất cả **Lesson Items** bên trong
- **Xóa Lesson Item** → Xóa **file đính kèm** (nếu có) trên server

### 2.4. File Management

- Khi **update** Lesson Item với file mới → File cũ sẽ **tự động bị xóa**
- Khi **delete** Lesson Item → File đính kèm sẽ **tự động bị xóa** khỏi server

---

## 3. API Course Section

### 3.1. Tạo Section

**Endpoint:** `POST /api/course/{courseId}/sections`

**Request:**

```json
{
  "name": "Section 1: Giới thiệu",
  "description": "Mô tả section",
  "order": 1,
  "isActive": 1
}
```

**Business Rules:**

- ✅ Section thuộc khóa học do chính mình tạo
- ✅ Có thể tạo ở mọi trạng thái khóa học

---

### 3.2. Cập nhật Section

**Endpoint:** `PUT /api/course/sections/{sectionId}`

**Request:**

```json
{
  "name": "Section 1 đã cập nhật",
  "description": "Mô tả section đã cập nhật",
  "order": 1,
  "isActive": 1
}
```

**Business Rules:**

- ✅ Section thuộc khóa học do chính mình tạo
- ✅ Khóa học phải ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`
- ❌ **KHÔNG THỂ** cập nhật khi khóa học đã được duyệt (`Open`)

**Response:**

```json
{
  "message": "Course section updated successfully",
  "statusCode": 200,
  "data": {
    "courseId": 1,
    "courseSectionId": 1,
    "name": "Section 1 đã cập nhật",
    "description": "Mô tả section đã cập nhật",
    "order": 1,
    "isActive": true
  }
}
```

---

### 3.3. Xóa Section

**Endpoint:** `DELETE /api/course/sections/{sectionId}`

**Business Rules:**

- ✅ Section thuộc khóa học do chính mình tạo
- ✅ Khóa học phải ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`
- ❌ **KHÔNG THỂ** xóa khi khóa học đã được duyệt (`Open`)
- ⚠️ **CẢNH BÁO**: Xóa section sẽ xóa **TẤT CẢ** lessons và lesson items bên trong (không thể hoàn tác)

**Response:**

```json
{
  "message": "Course section deleted successfully",
  "statusCode": 200,
  "data": "Section đã được xóa thành công."
}
```

---

## 4. API Lesson

### 4.1. Tạo Lesson

**Endpoint:** `POST /api/course/sections/{sectionId}/lessons`

**Request:**

```json
{
  "name": "Lesson 1: Tổng quan",
  "description": "Mô tả lesson",
  "order": 1,
  "isActive": 1
}
```

**Business Rules:**

- ✅ Lesson thuộc section của khóa học do chính mình tạo
- ✅ Có thể tạo ở mọi trạng thái khóa học

---

### 4.2. Cập nhật Lesson

**Endpoint:** `PUT /api/course/lessons/{lessonId}`

**Request:**

```json
{
  "name": "Lesson 1 đã cập nhật",
  "description": "Mô tả lesson đã cập nhật",
  "order": 1,
  "isActive": 1
}
```

**Business Rules:**

- ✅ Lesson thuộc khóa học do chính mình tạo
- ✅ Khóa học phải ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`
- ❌ **KHÔNG THỂ** cập nhật khi khóa học đã được duyệt (`Open`)

**Response:**

```json
{
  "message": "Lesson updated successfully",
  "statusCode": 200,
  "data": {
    "lessonId": 1,
    "courseSectionId": 1,
    "name": "Lesson 1 đã cập nhật",
    "description": "Mô tả lesson đã cập nhật",
    "order": 1,
    "isActive": true
  }
}
```

---

### 4.3. Xóa Lesson

**Endpoint:** `DELETE /api/course/lessons/{lessonId}`

**Business Rules:**

- ✅ Lesson thuộc khóa học do chính mình tạo
- ✅ Khóa học phải ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`
- ❌ **KHÔNG THỂ** xóa khi khóa học đã được duyệt (`Open`)
- ⚠️ **CẢNH BÁO**: Xóa lesson sẽ xóa **TẤT CẢ** lesson items bên trong (không thể hoàn tác)

**Response:**

```json
{
  "message": "Lesson deleted successfully",
  "statusCode": 200,
  "data": "Lesson đã được xóa thành công."
}
```

---

## 5. API Lesson Item

### 5.1. Tạo Lesson Item

**Endpoint:** `POST /api/course/lessons/{lessonId}/items`

**Request (multipart/form-data):**

```
name: "Video bài giảng"
description: "Mô tả video"
content: "" (để null nếu upload file)
itemType: "" (tự động detect từ file)
order: 1
file: [file upload] (video/pdf/image)
```

**Business Rules:**

- ✅ Lesson Item thuộc lesson của khóa học do chính mình tạo
- ✅ Có thể tạo ở mọi trạng thái khóa học
- ✅ Hỗ trợ upload: **video** (mp4, webm, ogg), **PDF**, **image** (jpg, png, gif, webp)
- ✅ Hệ thống tự động detect loại file và set `itemType`

---

### 5.2. Cập nhật Lesson Item

**Endpoint:** `PUT /api/course/lesson-items/{lessonItemId}`

**Request (multipart/form-data):**

```
name: "Video bài giảng đã cập nhật"
description: "Mô tả đã cập nhật"
content: "" (để null nếu upload file mới)
itemType: "video" (optional)
order: 1
file: [file mới] (optional - nếu có, file cũ sẽ bị xóa)
```

**Business Rules:**

- ✅ Lesson Item thuộc khóa học do chính mình tạo
- ✅ Khóa học phải ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`
- ❌ **KHÔNG THỂ** cập nhật khi khóa học đã được duyệt (`Open`)
- 🔄 **Logic tự động**: Nếu upload file mới → File cũ sẽ **tự động bị xóa**
- ✅ Tự động detect loại file và set `itemType`

**Response:**

```json
{
  "message": "Lesson item updated successfully",
  "statusCode": 200,
  "data": {
    "lessonItemId": 1,
    "lessonId": 1,
    "name": "Video bài giảng đã cập nhật",
    "description": "Mô tả đã cập nhật",
    "content": "/uploads/lessons/video.mp4",
    "itemType": "video",
    "order": 1
  }
}
```

---

### 5.3. Xóa Lesson Item

**Endpoint:** `DELETE /api/course/lesson-items/{lessonItemId}`

**Business Rules:**

- ✅ Lesson Item thuộc khóa học do chính mình tạo
- ✅ Khóa học phải ở trạng thái: `Draft`, `Pending`, hoặc `Rejected`
- ❌ **KHÔNG THỂ** xóa khi khóa học đã được duyệt (`Open`)
- 🗑️ **Logic tự động**: File đính kèm sẽ **tự động bị xóa** khỏi server

**Response:**

```json
{
  "message": "Lesson item deleted successfully",
  "statusCode": 200,
  "data": "Lesson item đã được xóa thành công."
}
```

---

## 6. Ví dụ sử dụng

### 6.1. Quy trình tạo nội dung khóa học

```bash
# 1. Tạo Section
POST /api/course/1/sections
{
  "name": "Section 1: Cơ bản",
  "order": 1,
  "isActive": 1
}
# Response: sectionId = 10

# 2. Tạo Lesson trong Section
POST /api/course/sections/10/lessons
{
  "name": "Lesson 1: Giới thiệu",
  "order": 1,
  "isActive": 1
}
# Response: lessonId = 20

# 3. Tạo Lesson Item (upload video)
POST /api/course/lessons/20/items
multipart/form-data:
  name: "Video bài giảng"
  order: 1
  file: [video.mp4]
# Response: lessonItemId = 30
```

### 6.2. Quy trình cập nhật nội dung

```bash
# 1. Kiểm tra trạng thái khóa học
GET /api/course/1
# Response: status = "Pending" ✅ (có thể update)

# 2. Cập nhật Section
PUT /api/course/sections/10
{
  "name": "Section 1: Cơ bản (đã cập nhật)",
  "order": 1,
  "isActive": 1
}

# 3. Cập nhật Lesson Item (thay file mới)
PUT /api/course/lesson-items/30
multipart/form-data:
  name: "Video bài giảng mới"
  file: [new-video.mp4]
# File cũ sẽ tự động bị xóa
```

### 6.3. Quy trình xóa nội dung

```bash
# 1. Kiểm tra trạng thái khóa học
GET /api/course/1
# Response: status = "Draft" ✅ (có thể delete)

# 2. Xóa Lesson Item
DELETE /api/course/lesson-items/30
# File đính kèm sẽ tự động bị xóa

# 3. Xóa Lesson (sẽ xóa tất cả lesson items bên trong)
DELETE /api/course/lessons/20
# Tất cả lesson items trong lesson này sẽ bị xóa

# 4. Xóa Section (sẽ xóa tất cả lessons và lesson items)
DELETE /api/course/sections/10
# Tất cả lessons và lesson items trong section này sẽ bị xóa
```

---

## 7. Xử lý lỗi

### 7.1. Các mã lỗi thường gặp

| Status Code | Lỗi                                  | Mô tả                         | Giải pháp                                 |
| ----------- | ------------------------------------ | ----------------------------- | ----------------------------------------- |
| `401`       | Unauthorized                         | Không xác định được tài khoản | Kiểm tra JWT token                        |
| `403`       | Forbidden                            | Không có quyền truy cập       | Đảm bảo role là `Teacher`                 |
| `400`       | Bad Request                          | Lỗi validation                | Kiểm tra request body                     |
| `400`       | Course section not found             | Section không tồn tại         | Kiểm tra `sectionId`                      |
| `400`       | Lesson not found                     | Lesson không tồn tại          | Kiểm tra `lessonId`                       |
| `400`       | Lesson item not found                | Lesson Item không tồn tại     | Kiểm tra `lessonItemId`                   |
| `400`       | Bạn không có quyền...                | Không phải người tạo          | Chỉ có thể thao tác với khóa học của mình |
| `400`       | Khóa học đã được duyệt, không thể... | Status = `Open`               | Không thể update/delete khi đã duyệt      |

### 7.2. Ví dụ Response lỗi

```json
{
  "message": "Khóa học đã được duyệt, không thể chỉnh sửa section.",
  "statusCode": 400,
  "data": null
}
```

```json
{
  "message": "Bạn không có quyền cập nhật section này.",
  "statusCode": 400,
  "data": null
}
```

```json
{
  "message": "Course section not found",
  "statusCode": 400,
  "data": null
}
```

### 7.3. Best Practices

1. **Luôn kiểm tra trạng thái khóa học trước khi update/delete:**

   ```bash
   GET /api/course/{courseId}
   # Kiểm tra status != "Open"
   ```

2. **Backup dữ liệu quan trọng trước khi xóa:**

   - Xóa section/lesson sẽ xóa tất cả dữ liệu con
   - Hành động xóa là **vĩnh viễn** và không thể hoàn tác

3. **Xử lý file upload:**

   - Chỉ upload file khi thực sự cần thay đổi
   - File cũ sẽ tự động bị xóa khi upload file mới

4. **Validation request:**
   - Đảm bảo `order` là số nguyên dương
   - `isActive` phải là `0` hoặc `1`
   - `name` là bắt buộc và không được rỗng

---

## 8. Tóm tắt Business Rules

### 8.1. Bảng quyền truy cập

| Thao tác                       | Draft | Pending | Rejected | Open |
| ------------------------------ | ----- | ------- | -------- | ---- |
| **CREATE** Section/Lesson/Item | ✅    | ✅      | ✅       | ✅   |
| **UPDATE** Section/Lesson/Item | ✅    | ✅      | ✅       | ❌   |
| **DELETE** Section/Lesson/Item | ✅    | ✅      | ✅       | ❌   |

### 8.2. Cascade Delete

| Xóa         | Sẽ xóa                        |
| ----------- | ----------------------------- |
| Section     | Tất cả Lessons + Lesson Items |
| Lesson      | Tất cả Lesson Items           |
| Lesson Item | File đính kèm (nếu có)        |

### 8.3. File Management

| Thao tác            | Hành động                 |
| ------------------- | ------------------------- |
| Update với file mới | Xóa file cũ tự động       |
| Delete Lesson Item  | Xóa file đính kèm tự động |

---

**Lưu ý quan trọng:**

- Tất cả các API yêu cầu **JWT Token** và role **Teacher**
- User ID được lấy tự động từ JWT token
- Hành động xóa là **vĩnh viễn** và không thể hoàn tác
- Luôn kiểm tra trạng thái khóa học trước khi thực hiện update/delete
