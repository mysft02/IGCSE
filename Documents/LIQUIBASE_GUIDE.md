## Hướng dẫn sử dụng Liquibase (MySQL) cho dự án IGCSE

Tài liệu này hướng dẫn bạn cài đặt, cấu hình và chạy Liquibase để quản lý database schema cho dự án.

---

## 1) Cài đặt Liquibase (CLI)

### macOS

```bash
brew install liquibase          # cài qua Homebrew
liquibase --version             # kiểm tra cài đặt
```

### Windows

1. Tải Liquibase (zip) từ trang chủ: `https://www.liquibase.org/download`
2. Giải nén, ví dụ vào `C:\\Tools\\Liquibase`
3. Thêm đường dẫn `C:\\Tools\\Liquibase` vào hệ thống PATH:
   - Start → tìm "Environment Variables" → Edit the system environment variables → Environment Variables…
   - Edit PATH (User hoặc System) → New → dán `C:\\Tools\\Liquibase`
4. Mở Command Prompt (cmd) hoặc PowerShell, kiểm tra:

```powershell
liquibase --version
```

---

## 2) Cấu trúc thư mục Liquibase trong dự án

```bash
../IGCSE/Migration
├── mysql-connector-j-9.4.0.jar     # JDBC driver MySQL (đã có)
├── liquibase.properties            # Cấu hình Liquibase
├── changelog-master.yaml           # Master changelog
├── ChangeLog/
│   └── ticket-8.yaml               # Ví dụ changeset tạo bảng course
└── liquibase-menu.sh               # Script menu thao tác nhanh (tùy chọn)
```

Lưu ý: nếu file changeset nằm trong thư mục `ChangeLog/`, hãy include đúng đường dẫn trong `changelog-master.yaml`.

Ví dụ nội dung `changelog-master.yaml`:

```yaml
databaseChangeLog:
  - include:
      file: ChangeLog/ticket-8.yaml
```

---

## 3) Cấu hình kết nối: `liquibase.properties`

Mặc định file tại: `/IGCSE/Migration/liquibase.properties`

Ví dụ:

```properties
changeLogFile=changelog-master.yaml
url=jdbc:mysql://localhost:3306/IGCSE
username=root
password=rootpassword
driver=com.mysql.cj.jdbc.Driver
classpath=mysql-connector-j-9.4.0.jar
```

- `url`: JDBC URL trỏ tới MySQL của bạn
- `username`/`password`: tài khoản MySQL
- `classpath`: JDBC driver MySQL (đã có sẵn trong thư mục `Migration`)

---

## 4) Cách chạy Liquibase (CLI)

Di chuyển tới thư mục Migration trước khi chạy:

```bash
cd Migration
```

Chạy câu lệnh để mở menu liquibase

```bash
./liquibase-menu.sh

```

---

## 5) Quy trình gợi ý (trước khi chạy ứng dụng)

```bash
# 1) Kiểm tra/điều chỉnh changelog-master.yaml và file changeset trong ChangeLog/
# 2) Kiểm tra kết nối trong liquibase.properties
# 3) Xem trước câu lệnh SQL sẽ chạy
liquibase --defaultsFile=liquibase.properties updateSQL

# 4) Áp dụng thay đổi
liquibase --defaultsFile=liquibase.properties update

# 5) (Tuỳ chọn) Xem trạng thái sau khi chạy
liquibase --defaultsFile=liquibase.properties status
```

Rollback nhanh (ví dụ quay lại 1 changeset):

```bash
liquibase --defaultsFile=liquibase.properties rollbackCount 1
```

Gắn thẻ (tag) và rollback theo tag:

```bash
liquibase --defaultsFile=liquibase.properties tag v1.0.0
liquibase --defaultsFile=liquibase.properties rollback v1.0.0
```

---

## 6) Lỗi thường gặp & cách xử lý

- Không tìm thấy Liquibase CLI:

  - macOS: `brew install liquibase`
  - Windows: tải zip từ trang chủ, thêm vào PATH như mục 1).

- Lỗi driver JDBC / kết nối:

  - Đảm bảo có file `mysql-connector-j-9.4.0.jar` cùng thư mục `liquibase.properties`.
  - Kiểm tra `url`, `username`, `password` đúng.
  - Kiểm tra MySQL đang chạy: `mysql -u root -p -e "SELECT 1;"`

- Include file trong changelog sai đường dẫn:
  - Với thư mục `ChangeLog/`, dùng: `file: ChangeLog/ticket-8.yaml` trong `changelog-master.yaml`.

##
