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
- **Lưu ý về Specs**: Hệ thống sử dụng cột `specifications` (JSONB) trong bảng `Products` thay vì bảng riêng lẻ để tăng tính linh hoạt.

#### 🛠️ Service Details

- `ShipmentService`: Quản lý vận chuyển đơn hàng theo quy trình QC + Đóng gói.
  - `CreateAsync` — Tạo phiếu giao hàng, tự động chuyển đơn sang Processing (3).
  - `UpdateAsync` — Cập nhật tracking code; nếu có tracking + QC pass → tự động chuyển đơn sang Shipping (4).
  - `MarkQcPassedAsync` — Đánh dấu QC pass/fail kèm ghi chú kiểm tra.
  - `MarkPackedAsync` — Ghi nhận nhân viên đóng gói (`packed_by`) và thời điểm.
  - `GetByOrderIdAsync` — Lấy thông tin vận chuyển theo đơn hàng.
  - **Luồng trạng thái**: `packing` → `qc_passed` → `packed` → `shipping` → `delivered`.

- `InventoryService`: Quản lý nhập kho và biến động tồn kho.
  - `CreateReceiptAsync` — Tạo phiếu nhập kho (Draft). Stock chưa tăng ở bước này.
  - `CompleteReceiptAsync` — Duyệt phiếu → tăng `stock_quantity`, tạo `InventoryTransaction` type=1.
  - `CancelReceiptAsync` — Hủy phiếu nhập kèm lý do (không ảnh hưởng stock).
  - `AdjustStockAsync` — Điều chỉnh tồn kho thủ công (kiểm kê, hàng hỏng).
  - `GetTransactionsAsync` — Lịch sử biến động kho theo sản phẩm.
  - `GetStockStatusAsync` — Tổng quan tồn kho toàn bộ sản phẩm.

- `ReviewService`: Triển khai interface `IReviewService` xử lý nghiệp vụ đánh giá.
  - **Reviews**: `GetAllAsync`, `GetByIdAsync`, `UpdateActiveAsync`, `DeleteAsync`.
  - **Replies**: `CreateReplyAsync`, `UpdateReplyAsync`, `DeleteReplyAsync`.
  - **Images**: `AddImageAsync`, `DeleteImageAsync`, `GetImagesByReviewIdAsync`.
  - **Votes**: `ToggleVoteAsync` (POST /toggle), `GetVoteCountAsync` (GET /count), `HasUserVotedAsync` (GET /check/{userId}).

- `AuthService`: Xử lý xác thực và quản lý tài khoản.
  - **Auth**: `LoginAsync`, `RegisterAsync`, `GoogleLoginAsync`, `RefreshTokenAsync`.
  - **Password Recovery**: `ForgotPasswordAsync`, `ResetPasswordAsync` (OTP Email).

- `ProfileService`: Quản lý thông tin cá nhân và sổ địa chỉ của người dùng.

- `WishlistService`: Quản lý sản phẩm yêu thích.
  - **Logic**: Tối đa 50 sản phẩm/user, `ToggleAsync`, `GetByUserAsync`.

- `ReturnRequestService`: Xử lý đổi trả hàng.
  - **Logic**: Kiểm tra thời hạn 7 ngày, trạng thái `Delivered`, tự động hoàn kho khi Admin duyệt.

- `EmailNotificationService`: Hệ thống thông báo tự động.
  - **Emails**: Xác nhận đơn hàng, cập nhật vận chuyển, kết quả đổi trả, OTP khôi phục mật khẩu.

- `ProductService`: Quản lý danh mục sản phẩm và tìm kiếm nâng cao.
  - **CRUD**: `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` (Soft delete).
  - **Lookup**: `GetBySlugAsync`, `GetFullDetailsAsync`.
  - **Client API**: `GetPagedListAsync` với bộ lọc phức tạp (Price, Category, Brand, Sort).

### ⚙️ Quy trình xử lý (Data Flow)

`Request` → `Controller` → `Service` → `UnitOfWork` → `Repository` → `Database`

---

## 3. Chi tiết Frontend (Angular)

### 📂 Cơ cấu thư mục chi tiết

- `src/app/core/`: Chứa các thành phần cốt lõi dùng chung (Guards, Interceptors, Pipes, Services toàn cục như Auth, Theme).
- `src/app/shared/`: Chứa các UI Components dùng lại ở nhiều nơi:
  - **Common**: Navbar, Toast, Loading, Modals, Flash sale countdown badge.
  - **After-Sales**: `WishlistToggleComponent`, `VerifiedBadgeComponent`.
- `src/app/features/`: Chia theo từng phân hệ chức năng:
  - `admin/`: Dashboard, Products, **Inventory Receipts**, **Packing Slip**, **Supplier Management**, News, Banners, Coupons, Flash Sales, HRM, Activity Logs, Return Requests.
  - `employee/`: Quản lý đơn hàng, kho sản phẩm, phản hồi đánh giá và tra cứu khách hàng.
  - `customer/`: Màn hình trang chủ, danh sách sản phẩm, tin tức cho khách hàng.
  - `product/`: Chi tiết sản phẩm, thư viện hình ảnh, cấu hình kỹ thuật (dùng JSONB) và đánh giá khách hàng.
  - `user/`: Profile, sổ địa chỉ, lịch sử đơn, **order tracking** (xem trạng thái + vận chuyển), đổi trả.
  - `auth/`: Đăng nhập, đăng ký, quên mật khẩu.
  - `cart/`, `build-pc/`, `comparison/`, `tech-news/`.
- `src/app/layouts/`: Các bộ khung layout khác nhau (`AdminLayoutComponent`, `EmployeeLayoutComponent`, `MainLayoutComponent`).

### 🎨 Styling & UI

- **Tailwind CSS 4**: Sử dụng engine mới nhất của Tailwind để tối ưu tốc độ và kích thước CSS.
- **Responsive Design**: Hỗ trợ đầy đủ Mobile, Tablet và Desktop.

#### 🛠️ Frontend Service Details (Core)

- `InventoryService`: CRUD phiếu nhập kho, complete, cancel, adjust stock, stock-status.
- `ShipmentService`: Tạo shipment, cập nhật, mark QC, mark packed, get by orderId.
- `SupplierService`: CRUD nhà cung cấp.
- `OrderService`: Tạo đơn, lấy lịch sử, hủy đơn, cập nhật trạng thái.
- `ReviewService`: Lấy đánh giá, tạo, phản hồi, vote.
- `AuthService`: Đăng nhập, đăng ký, refresh token, quên mật khẩu.
- `CartService`: Quản lý giỏ hàng (RxJS BehaviorSubject).
- `WishlistService`: Sản phẩm yêu thích (Angular Signals).

---

## 4. Cơ sở dữ liệu (Database)

Hệ thống sử dụng **PostgreSQL** với **34 bảng** được tổ chức thành 10 nhóm:

- **Identity (5)**: `users`, `addresses`, `refresh_tokens`, `password_reset_tokens`.
- **Catalog (4)**: `products` (JSONB specs), `categories` (hierarchical), `brands`, `product_images`.
- **Sales (4)**: `orders`, `order_items`, `order_status_history`, `payments`.
- **Logistics (1)**: `shipments` (status: packing/qc_passed/packed/shipping/delivered, ghi nhận QC + người đóng gói).
- **Inventory (4)**: `suppliers`, `inventory_receipts`, `inventory_receipt_items`, `inventory_transactions`.
- **After-Sales (3)**: `return_requests`, `return_request_items`, `return_request_images`.
- **Marketing (5)**: `coupons`, `coupon_usages`, `flash_sales`, `flash_sale_items`, `banners`.
- **CMS (2)**: `news`, `news_categories`.
- **Engagement (5)**: `reviews`, `review_images`, `review_replies`, `review_helpful_votes`, `wishlists`.
- **Audit (1)**: `activity_logs`.

> **Lưu ý**: Sử dụng extension `pgcrypto` để hỗ trợ `gen_random_uuid()`.

---

## 5. Danh sách Services & DTOs Key

### 🛠️ Backend Services (Key)
- `ShipmentService`, `IShipmentService`: Quản lý vận chuyển (QC + Packing + Tracking).
- `InventoryService`, `IInventoryService`: Nhập kho, điều chỉnh tồn kho, tổng quan stock.
- `OrderService`, `IOrderService`: Vòng đời đơn hàng.
- `PaymentService`, `VnPayService`: Xử lý thanh toán.
- `CouponService`: Mã giảm giá.
- `FlashSaleService`: Khuyến mãi Flash Sale.
- `ActivityLogService`: Ghi log hoạt động.
- `DashboardService`: Dữ liệu tổng hợp.
- `ReturnRequestService`: Đổi trả hàng.
- `EmailNotificationService`: Thông báo email.

### 📦 DTOs Key
- `ShipmentDto`, `CreateShipmentDto`, `UpdateShipmentDto`, `MarkQcDto`.
- `InventoryReceiptDto`, `CreateInventoryReceiptDto`, `AdjustStockDto`, `StockStatusDto`.
- `CreateOrderDto`, `OrderDetailDto`, `UpdateOrderDto`.
- `PaymentDto`, `OrderStatusHistoryDto`, `ReturnRequestDto`.

---

## 6. Shared Frontend Foundation

### 📦 Models (`core/models/`)
- `order.model.ts`: `OrderDto`, `OrderDetailDto`, `PaymentDto`, `ShipmentDto`, `OrderStatusHistoryDto`, `ReturnRequestDto`.
- `inventory.model.ts`: `InventoryReceipt`, `InventoryTransaction`, `StockStatus`, `AdjustStockDto`.
- `supplier.model.ts`: `Supplier`, `CreateSupplierDto`, `UpdateSupplierDto`.
- `product.model.ts`, `user.model.ts`, `review.model.ts`, etc.

### 🛣️ Routes
- `/admin/inventory-receipts`: Quản lý phiếu nhập kho.
- `/admin/packing-slip`: In phiếu đóng gói và QC.
- `/admin/supplier-management`: Quản lý nhà cung cấp.
- `/user/order-tracking`: Theo dõi trạng thái và vận chuyển đơn hàng.
- `/payment/vnpay-return`: Xử lý phản hồi từ VnPay.
- `/admin/coupons`, `/admin/flash-sales`, `/admin/activity-logs`: Routes quản trị mở rộng.

---

## 🛠️ Quy chuẩn Kỹ thuật (Technical Standards)

### Backend

- **Dependency Injection**: Luôn đăng ký Service trong `Program.cs`.
- **Async/Await**: Sử dụng lập trình bất đồng bộ cho mọi tác vụ I/O.
- **Error Handling**: Sử dụng `ApiResponse<T>` chuẩn để trả về dữ liệu. Exception Middleware xử lý tập trung.
- **Authorization**: `[Authorize(Roles = "admin,staff")]` cho các API quản trị.

### Frontend

- **Type-safe**: Tuyệt đối không sử dụng `any`, luôn định nghĩa Interface/Type.
- **Strict Mode**: Tuân thủ Angular Strict Mode để hạn chế lỗi runtime.
- **Shared Components**: Các thành phần giao diện lặp lại phải được đưa vào `shared/`.
- **Signals**: Ưu tiên Angular Signals cho state management thay vì RxJS Subject khi có thể.

---

## 🧪 7. Testing Resources

- **API Documentation**: Tài liệu chi tiết các endpoint và phương thức (GET, POST, PUT, PATCH, DELETE) có tại [.idea/md/API_DOCUMENTATION.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/md/API_DOCUMENTATION.md).
- **API CSV**: Bản xuất CSV của tài liệu API tại [api_documentation.csv](file:///d:/Workspace/Cong_Nghe_Web/api_documentation.csv).
- **Sample Data**: Chi tiết các mẫu dữ liệu JSON để test API có tại [.idea/md/SAMPLE_DATA.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/md/SAMPLE_DATA.md).
- **Test Plan**: Kế hoạch và các kịch bản kiểm thử chi tiết có tại [.idea/md/TEST_PLAN.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/md/TEST_PLAN.md).
- **Unit Test Guide**: Hướng dẫn viết và chạy Unit Test cho Backend (.NET) tại [.idea/md/UNIT_TEST_GUIDE.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/md/UNIT_TEST_GUIDE.md).
- **Unit Test Checklist**: Danh sách các hàm cần viết test tại [.idea/md/UNIT_TEST_CHECKLIST.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/md/UNIT_TEST_CHECKLIST.md) (hoặc bản CSV tại [unit_test_checklist.csv](file:///d:/Workspace/Cong_Nghe_Web/unit_test_checklist.csv)).
