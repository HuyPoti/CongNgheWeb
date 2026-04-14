# Project Architecture & Technical Documentation

Tài liệu này mô tả chi tiết kiến trúc hệ thống, quy trình xử lý dữ liệu và các thành phần kỹ thuật của dự án.

## 1. Tổng Quan Kiến Trúc (System Overview)

Dự án sử dụng mô hình **N-Layer Architecture** (kiến trúc đa tầng) giúp tách biệt các lớp trách nhiệm, dễ dàng bảo trì và mở rộng.

- **Client Side (Frontend)**: Angular 21 Single Page Application (SPA).
- **Server Side (Backend)**: ASP.NET Core 8.0 Web API.
- **Database**: PostgreSQL (quản lý qua Docker hoặc Supabase).

---

## 2. Chi tiết Backend (.NET Core)

### 📂 Cơ cấu thư mục chi tiết
- `Controllers/`: Tiếp nhận HTTP Request, điều hướng xử lý qua Services.
- `Services/`: Chứa logic nghiệp vụ (Business Logic). Tách biệt Interface (`IService`) và Implementation.
- `UnitOfWork/`: Implement Pattern **Unit of Work** và **Generic Repository** để quản lý giao dịch database.
- `Models/`: Các Entity ánh xạ trực tiếp với bảng trong Database (Entity Framework Core).
- `DTOs/`: Data Transfer Objects - dùng để truyền tải dữ liệu giữa Client và Server (tránh lộ Entity).
- `Data/`: Chứa `AppDbContext` và cấu hình Seed Data.
- `MapperProfiles/`: Cấu hình **AutoMapper** để chuyển đổi giữa Models và DTOs.
- `Middleware/`: Chứa `ExceptionMiddleware` xử lý lỗi tập trung toàn hệ thống.

#### 🛠️ Service Details (Review)
- `ReviewService`: Triển khai interface `IReviewService` xử lý nghiệp vụ đánh giá.
  - **Reviews**: `GetAllAsync`, `GetByIdAsync`, `UpdateActiveAsync`, `DeleteAsync`.
  - **Replies**: `CreateReplyAsync`, `UpdateReplyAsync`, `DeleteReplyAsync`.
  - **Images**: `AddImageAsync`, `DeleteImageAsync`, `GetImagesByReviewIdAsync`.
  - **Votes**: `ToggleVoteAsync`, `GetVoteCountAsync`, `HasUserVotedAsync`.


### ⚙️ Quy trình xử lý (Data Flow)
`Request` -> `Controller` -> `Service` -> `UnitOfWork` -> `Repository` -> `Database`

---

## 3. Chi tiết Frontend (Angular)

### 📂 Cơ cấu thư mục chi tiết
- `src/app/core/`: Chứa các thành phần cốt lõi dùng chung (Guards, Interceptors, Pipes, Services toàn cục như Auth, Theme).
- `src/app/shared/`: Chứa các UI Components dùng lại ở nhiều nơi (Navbar, Toast, Loading, Modals).
- `src/app/features/`: Chia theo từng phân hệ chức năng:
  - `admin/`: Quản lý Dashboard, Products, News, Banners.
  - `customer/`: Màn hình trang chủ, danh sách sản phẩm, tin tức cho khách hàng.
  - `auth/`: Đăng nhập, đăng ký.
- `src/app/layouts/`: Các bộ khung layout khác nhau (`AdminLayoutComponent`, `MainLayoutComponent`).

### 🎨 Styling & UI
- **Tailwind CSS 4**: Sử dụng engine mới nhất của Tailwind để tối ưu tốc độ và kích thước CSS.
- **Responsive Design**: Hỗ trợ đầy đủ Mobile, Tablet và Desktop.

#### 🛠️ Service Details (Review)
- `ReviewService`: Chứa logic xử lý đánh giá sản phẩm.
  - **Reviews**: `getAll`, `getById`, `updateActive`, `delete`.
  - **Replies**: `createReply`, `updateReply`, `deleteReply`.
  - **Images**: `addImage`, `deleteImage`, `getImages`.
  - **Votes**: `toggleVote`, `getVoteCount`, `checkUserVoted`.


---

## 4. Cơ sở dữ liệu (Database)

Hệ thống sử dụng **PostgreSQL** với các bảng chính:
- `Users`: Quản trị viên và khách hàng.
- `Products`: Thông tin sản phẩm.
- `Categories`: Danh mục sản phẩm.
- `Brands`: Thương hiệu sản phẩm.
- `News` & `NewsCategories`: Quản lý tin tức.
- `Banners`: Hình ảnh quảng cáo.
- `Reviews`: Đánh giá và bình luận sản phẩm.
- `ReviewImages`: Hình ảnh đính kèm trong đánh giá.
- `ReviewReplies`: Phản hồi từ quản trị viên đối với đánh giá.

---

## 🛠️ Quy chuẩn Kỹ thuật (Technical Standards)

### Backend
- **Dependency Injection**: Luôn đăng ký Service trong `Program.cs`.
- **Async/Await**: Sử dụng lập trình bất đồng bộ cho mọi tác vụ I/O.
- **Error Handling**: Sử dụng `ApiResponse<T>` chuẩn để trả về dữ liệu.

### Frontend
- **Type-safe**: Tuyệt đối không sử dụng `any`, luôn định nghĩa Interface/Type.
- **Strict Mode**: Tuân thủ Angular Strict Mode để hạn chế lỗi runtime.
- **Shared Components**: Các thành phần giao diện lặp lại phải được đưa vào `shared/`.
