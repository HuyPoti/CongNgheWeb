# Project Name: Cong Nghe Web

Dự án này là một ứng dụng web full-stack bao gồm Backend (.NET Core) và Frontend (Angular).

## 🚀 Hướng dẫn khởi chạy

### 1. Yêu cầu hệ thống
*   [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
*   [Node.js 20+](https://nodejs.org/)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (cho database local)

---

### 2. Cài đặt Database (DB)
Dự án sử dụng PostgreSQL. Bạn có thể chạy database local qua Docker hoặc sử dụng Supabase.

**Chạy với Docker:**
1. Mở terminal tại thư mục gốc.
2. Chạy lệnh:
   ```bash
   docker-compose up -d
   ```
3. Database sẽ chạy tại `localhost:5433`.

**Import dữ liệu:**
Sử dụng file `.db.sql` trong thư mục gốc để import cấu trúc bảng ban đầu nếu cần thiết.

---

### 3. Khởi chạy Backend
1. Di chuyển vào thư mục `backend`:
   ```bash
   cd backend
   ```
2. Restore các package:
   ```bash
   dotnet restore
   ```
3. Chạy ứng dụng:
   ```bash
   dotnet run
   ```
   *Backend mặc định chạy tại `https://localhost:7xxx` hoặc `http://localhost:5xxx`.*

---

### 4. Khởi chạy Frontend
1. Di chuyển vào thư mục `frontend`:
   ```bash
   cd frontend
   ```
2. Cài đặt dependencies:
   ```bash
   npm install
   ```
3. Chạy ứng dụng:
   ```bash
   npm start
   ```
   *Frontend mặc định chạy tại `http://localhost:4200`.*

---

## 🛠️ Quy chuẩn Git & Commit

### 1. Cấu trúc Commit chuẩn (Conventional Commits)
Chúng ta tuân thủ quy tắc [Conventional Commits](https://www.conventionalcommits.org/):

**Định dạng:** `<type>(<scope>): <subject>`

**Các loại (type) phổ biến:**
*   `feat`: Tính năng mới.
*   `fix`: Sửa lỗi.
*   `docs`: Thay đổi tài liệu.
*   `style`: Thay đổi format code (không ảnh hưởng logic).
*   `refactor`: Cơ cấu lại code (không sửa lỗi cũng không thêm tính năng).
*   `perf`: Tối ưu hiệu năng.
*   `chore`: Các thay đổi nhỏ về build tool, dependency...

### 2. Commit với Jira Key
Mọi commit **BẮT BUỘC** phải đính kèm Jira ID (ví dụ: `WEB-123`) ở đầu câu hoặc trong nội dung commit.

**Ví dụ:**
```text
feat(product): [WEB-456] add filter by category
```

### 3. Quy tắc đặt tên Nhánh (Branching)
Tên nhánh nên bao gồm loại công việc và Jira Key.

**Định dạng:** `<jira-key>`
**Ví dụ:**
```bash
git checkout -b WEB-123
git checkout -b WEB-456
```

---

## 📁 Cấu trúc .gitignore (Ignore)
Dự án có các file `.gitignore` ở root và từng thư mục con để loại bỏ các file rác:
*   `node_modules/`: Thư viện npm.
*   `bin/`, `obj/`: File build của .NET.
*   `.angular/`: Cache của Angular.
*   `dist/`: Sản phẩm sau khi build.
*   `.env`, `appsettings.Development.json`: Chứa các thông tin nhạy cảm.

---

## 📑 Tài liệu kiến trúc
Xem chi tiết tại [architecture.md](./architecture.md) để biết thêm về cấu trúc thư mục, các hàm và logic xử lý của ứng dụng.
