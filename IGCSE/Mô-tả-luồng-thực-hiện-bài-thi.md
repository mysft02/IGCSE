# Luồng Thực Hiện Bài Kiểm Tra (Mock Test)

Tài liệu này mô tả chi tiết luồng thực hiện bài kiểm tra từ đầu đến cuối trong hệ thống IGCSE.

## Tổng Quan Luồng

Luồng thực hiện bài kiểm tra bao gồm 6 bước chính:

1. **Lấy danh sách package** - Người dùng xem các gói bài thi có sẵn
2. **Mua package** - Người dùng thanh toán để mở khóa bài thi
3. **Xem danh sách bài thi** - Người dùng xem các bài thi mock test với trạng thái
4. **Thực hiện bài thi** - Người dùng làm bài thi mock test
5. **Chấm bài thi** - Hệ thống chấm điểm và lưu kết quả
6. **Xem kết quả** - Người dùng xem lịch sử kết quả các bài thi đã làm

---

## Bước 1: Lấy Danh Sách Package

### Endpoint
```
GET /api/package/get-all-package
```

### Mô Tả
Người dùng xem toàn bộ các package (gói bài thi) có trong hệ thống. Endpoint này không yêu cầu xác thực, cho phép người dùng xem các gói bài thi trước khi quyết định mua.

### Tham Số
- `PackageQueryRequest`: Query parameters cho phân trang (page, pageSize, ...)

### Response
- `BaseResponse<PaginatedResponse<Package>>`: Danh sách package có phân trang
- Mỗi package chứa thông tin về gói bài thi, giá, mô tả, v.v.

### Ghi Chú
- Không cần xác thực (không có `[Authorize]`)
- Hỗ trợ phân trang
- Người dùng có thể xem tất cả package trước khi đăng nhập

---

## Bước 2: Mua Package

### 2.1. Lấy URL Thanh Toán PayOS

#### Endpoint
```
GET /api/payment/get-payos-payment-url
```

#### Mô Tả
Người dùng đã chọn package và muốn mua. Hệ thống tạo URL thanh toán PayOS để người dùng thanh toán.

#### Tham Số
- `PayOSPaymentRequest`: Thông tin thanh toán(không thể nhập courseId và packageId cùng 1 lúc):
   + Amount: số tiền thanh toán
   + CourseId: id của khóa học muốn thanh toán
   + PackageId: id của gói muốn thanh toán

- Yêu cầu xác thực: **Có** (`[Authorize]`)

#### Response
- `BaseResponse<PayOSApiResponse>`: Chứa URL thanh toán PayOS
- Người dùng sẽ được redirect đến URL này để thanh toán

#### Quy Trình
1. Hệ thống lấy `userId` từ JWT token
2. Tạo yêu cầu thanh toán với PayOS
3. Trả về URL thanh toán: `checkoutUrl`
4. Người dùng được chuyển hướng đến trang thanh toán PayOS

---

### 2.2. Xử Lý Callback Thanh Toán

#### Endpoint
```
POST /api/payment/payment-callback
```

#### Mô Tả
Sau khi thanh toán thành công trên PayOS, PayOS sẽ gọi callback này để thông báo kết quả thanh toán. Hệ thống sẽ xử lý trên frontend, gửi dữ liệu thông tin thanh toán sau khi return và cập nhật trạng thái mua package cho người dùng.

#### Tham Số
- `queryParams`: Dictionary chứa thông tin thanh toán từ PayOS bao gồm:
   + `code`: mã lỗi (`00` là thành công, `01` là invalid params)
   + `id`: id giao dịch 
   + `cancel`: trạng thái hủy(`true` là đã hủy, `false` là chờ thanh toán hoặc đã thanh toán)
   + `status`: trạng thái thanh toán(`PAID` là đã thanh toán, `PENDING` là chờ thanh toán, 
                                    `PROCESSING` là đang xử lí, `CANCELLED` là đã hủy)
   + `orderCode`: mã đơn hàng
- Yêu cầu xác thực: **Có** (`[Authorize]`)

#### Response
- `BaseResponse<PayOSPaymentReturnResponse>`: Kết quả xử lý thanh toán
- Nếu thành công, package sẽ được kích hoạt cho người dùng

#### Quy Trình
1. PayOS gửi thông tin thanh toán về hệ thống
2. Hệ thống xác thực thông tin thanh toán
3. Cập nhật trạng thái mua package cho người dùng
4. Mở khóa các bài thi mock test trong package đã mua

#### Kết Quả
- Sau khi thanh toán thành công, người dùng có quyền truy cập vào các bài thi mock test trong package đã mua
- Các bài thi sẽ chuyển từ trạng thái `Locked` (3) sang `Open` (2)

---

## Bước 3: Xem Danh Sách Bài Thi Mock Test

### Endpoint
```
GET /api/mocktest/get-all-mocktest
```

### Mô Tả
Sau khi đã mua package, người dùng có thể xem danh sách tất cả các bài thi mock test. Hệ thống sẽ hiển thị trạng thái của từng bài thi dựa trên việc người dùng đã mua package hay chưa, và đã hoàn thành bài thi hay chưa.

### Tham Số
- `MockTestQueryRequest`: Query parameters cho phân trang và lọc
- Yêu cầu xác thực: **Có** (`[Authorize]`)

### Response
- `BaseResponse<PaginatedResponse<MockTestResponse>>`: Danh sách bài thi mock test có phân trang
- Mỗi bài thi có trạng thái:
  - **1 - Completed**: Đã hoàn thành bài thi trước đó
  - **2 - Open**: Đã mua gói nhưng chưa hoàn thành bài thi trước đó
  - **3 - Locked**: Chưa mua gói bài thi

### Quy Trình
1. Hệ thống lấy `userId` từ JWT token
2. Kiểm tra các package mà người dùng đã mua
3. Xác định trạng thái của từng bài thi:
   - Nếu đã mua package và đã làm bài → `Completed` (1)
   - Nếu đã mua package nhưng chưa làm bài → `Open` (2)
   - Nếu chưa mua package → `Locked` (3)
4. Trả về danh sách bài thi với trạng thái tương ứng

### Ghi Chú
- Chỉ người dùng đã đăng nhập mới có thể xem danh sách
- Trạng thái `Open` (2) cho phép người dùng bắt đầu làm bài thi
- Trạng thái `Locked` (3) yêu cầu người dùng mua package trước

---

## Bước 4: Thực Hiện Bài Thi Mock Test

### 4.1. Lấy Thông Tin Bài Thi

#### Endpoint
```
GET /api/mocktest/get-mocktest-by-id
```

#### Mô Tả
Người dùng chọn một bài thi mock test và yêu cầu lấy thông tin chi tiết để bắt đầu làm bài. Hệ thống sẽ trả về các câu hỏi và thông tin bài thi.

#### Tham Số
- `id`: ID của bài thi mock test
- Yêu cầu xác thực: **Có** (`[Authorize]`)

#### Response
- `BaseResponse<MockTestResponse>`: Thông tin chi tiết bài thi bao gồm:
  - Thông tin bài thi (tên, mô tả, thời gian, v.v.)
  - Danh sách câu hỏi
  - Các lựa chọn trả lời (không bao gồm đáp án đúng)

#### Quy Trình
1. Hệ thống lấy `userId` từ JWT token
2. Kiểm tra quyền truy cập: người dùng phải đã mua package chứa bài thi này
3. Lấy thông tin bài thi và câu hỏi
4. Ẩn đáp án đúng (chỉ hiển thị các lựa chọn)
5. Trả về thông tin bài thi cho người dùng

#### Validation
- Người dùng phải đã mua package chứa bài thi này
- Nếu chưa mua, hệ thống sẽ từ chối yêu cầu

---

### 4.2. Chấm Bài Thi

#### Endpoint
```
POST /api/mocktest/mark-mocktest
```

#### Mô Tả
Sau khi người dùng hoàn thành bài thi và nộp bài, hệ thống sẽ chấm điểm tự động. Kết quả sẽ được lưu vào database.

#### Tham Số
- `MockTestMarkRequest`: Chứa:
  - `mockTestId`: ID của bài thi
  - `answers`: Danh sách câu trả lời của người dùng. mỗi phần tử gồm:
     + `questionId`: id của câu hỏi mà người dùng làm
     + `answer`: câu trả lời của người dùng cho câu hỏi đó
- Yêu cầu xác thực: **Có** (`[Authorize]`)

#### Response
- `BaseResponse<List<MockTestMarkResponse>>`: Kết quả chấm bài là danh sách bao gồm:
  - `question`: nội dung câu hỏi
  - `answer`: nội dung câu trả lời của người dùng
  - `rightAnswer`: nội dung câu trả lời đúng cho câu hỏi này
  - `score`: điểm số của người dùng
  - `isCorrect`: câu trả lời của người dùng đúng hay sai
  - `comment`: câu trả lời của AI về câu hỏi bao gồm đúng hay sai và cách làm chính xác để cho ra được đáp án đúng

#### Quy Trình
1. Hệ thống lấy `userId` từ JWT token
2. Nhận danh sách câu trả lời từ người dùng
3. So sánh với đáp án đúng trong database
4. Tính điểm số
5. Lưu kết quả vào database:
   - Điểm số
   - Thời gian làm bài
   - Chi tiết từng câu trả lời
6. Trả về kết quả chấm bài cho người dùng

#### Tính Năng
- Chấm điểm tự động
- Lưu lịch sử làm bài
- Hiển thị đáp án đúng và giải thích (nếu có)
- Cập nhật trạng thái bài thi

---

## Bước 5: Xem Kết Quả Bài Thi

### Endpoint
```
GET /api/mocktest/get-mocktest-result
```

### Mô Tả
Người dùng có thể xem lịch sử kết quả tất cả các bài thi mock test mà họ đã hoàn thành. Danh sách được hiển thị với phân trang.

### Tham Số
- `MockTestResultQueryRequest`: Query parameters cho phân trang và lọc
- Yêu cầu xác thực: **Có** (`[Authorize]`)

### Response
- `BaseResponse<PaginatedResponse<MockTestResultQueryResponse>>`: Danh sách kết quả bài thi có phân trang
- Mỗi kết quả bao gồm:
  - `mockTest`: thông tin của bài thi thử đó
  - `score`: điểm số của người dùng sau lần thi thử đó
  - `dateTaken`: thời gian người dùng hoàn thành bài thi

### Quy Trình
1. Hệ thống lấy `userId` từ JWT token
2. Gán `userId` vào request để lọc kết quả
3. Truy vấn database lấy tất cả kết quả bài thi của người dùng
4. Áp dụng phân trang và sắp xếp
5. Trả về danh sách kết quả

### Tính Năng
- Xem lịch sử tất cả bài thi đã làm
- Phân trang kết quả
- Lọc và sắp xếp kết quả
- Chỉ hiển thị kết quả của người dùng hiện tại

---

## Sơ Đồ Luồng

```
┌─────────────────────────────────────────────────────────────────┐
│                    LUỒNG THỰC HIỆN BÀI KIỂM TRA                 │
└─────────────────────────────────────────────────────────────────┘

1. LẤY DANH SÁCH PACKAGE
   GET /api/package/get-all-package
   ↓
   [Người dùng xem các gói bài thi có sẵn]
   ↓

2. MUA PACKAGE
   ├─ GET /api/payment/get-payos-payment-url
   │  [Lấy URL thanh toán PayOS]
   │  ↓
   │  [Người dùng thanh toán trên PayOS]
   │  ↓
   └─ POST /api/payment/payment-callback
      [Xử lý kết quả thanh toán và mở khóa bài thi]
      ↓

3. XEM DANH SÁCH BÀI THI
   GET /api/mocktest/get-all-mocktest
   ↓
   [Hiển thị bài thi với trạng thái:
    - Completed (1): Đã hoàn thành
    - Open (2): Đã mua, chưa làm
    - Locked (3): Chưa mua]
   ↓

4. THỰC HIỆN BÀI THI
   ├─ GET /api/mocktest/get-mocktest-by-id
   │  [Lấy thông tin bài thi và câu hỏi]
   │  ↓
   │  [Người dùng làm bài thi]
   │  ↓
   └─ POST /api/mocktest/mark-mocktest
      [Chấm bài và lưu kết quả]
      ↓

5. XEM KẾT QUẢ
   GET /api/mocktest/get-mocktest-result
   ↓
   [Hiển thị lịch sử kết quả các bài thi đã làm]
```

---

## Lưu Ý Quan Trọng

### Xác Thực
- Hầu hết các endpoint yêu cầu xác thực (`[Authorize]`)
- Chỉ endpoint `GET /api/package/get-all-package` không yêu cầu xác thực
- `userId` được lấy từ JWT token qua `HttpContext.User.FindFirst("AccountID")?.Value`

### Trạng Thái Bài Thi
- **Locked (3)**: Người dùng chưa mua package → Cần mua package trước
- **Open (2)**: Người dùng đã mua package → Có thể bắt đầu làm bài
- **Completed (1)**: Người dùng đã hoàn thành bài thi → Có thể xem lại kết quả

### Quy Trình Thanh Toán
- Sử dụng PayOS làm cổng thanh toán
- Thanh toán được xử lý bất đồng bộ qua callback
- Sau khi thanh toán thành công, package sẽ được kích hoạt tự động

### Bảo Mật
- Tất cả endpoint đều kiểm tra `userId` từ token
- Người dùng chỉ có thể xem và làm các bài thi trong package đã mua
- Kết quả bài thi chỉ hiển thị cho chính người dùng đó

---

## Tài Liệu Tham Khảo

- **PackageController.cs**: Quản lý package và gói bài thi
- **PaymentController.cs**: Xử lý thanh toán qua PayOS
- **MockTestController.cs**: Quản lý bài thi mock test và kết quả

---

*Tài liệu được tạo tự động dựa trên mã nguồn hệ thống IGCSE*

