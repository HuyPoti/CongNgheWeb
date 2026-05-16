# Project Architecture & Technical Documentation

Tài liệu này mô tả chi tiết kiến trúc hệ thống, quy trình xử lý dữ liệu và các thành phần kỹ thuật của dự án.

## 1. Tổng Quan Kiến Trúc (System Overview)

Dự án sử dụng mô hình **N-Layer Architecture** (kiến trúc đa tầng) giúp tách biệt các lớp trách nhiệm, dễ dàng bảo trì và mở rộng.

- **Client Side (Frontend)**: Angular 21 (v21.1.0) Single Page Application (SPA) với SSR support.
- **Server Side (Backend)**: ASP.NET Core 10.0 Web API (net10.0).
- **Database**: PostgreSQL 16 (quản lý qua Docker hoặc Supabase).
- **Containerization**: Docker Compose cho PostgreSQL.

---

## 2. Chi tiết Backend (.NET Core)

### 📂 Cơ cấu thư mục chi tiết

- `Controllers/` (23 files): Tiếp nhận HTTP Request, điều hướng xử lý qua Services.
- `Services/` (46 files): Chứa logic nghiệp vụ (Business Logic). Tách biệt Interface (`IService`) và Implementation.
- `UnitOfWork/` (4 files): Implement Pattern **Unit of Work** và **Generic Repository** để quản lý giao dịch database.
- `Models/` (34 files): Các Entity ánh xạ trực tiếp với bảng trong Database (Entity Framework Core) + enum `UserRole`.
- `DTOs/` (63 files): Data Transfer Objects - dùng để truyền tải dữ liệu giữa Client và Server (tránh lộ Entity).
- `Data/`: Chứa `AppDbContext` và cấu hình Entity mappings.
- `MapperProfiles/` (13 files): Cấu hình **AutoMapper** để chuyển đổi giữa Models và DTOs.
- `Middleware/`: Chứa `ExceptionMiddleware` xử lý lỗi tập trung toàn hệ thống.
- `Exceptions/`: Custom exceptions — `BadRequestException`, `NotFoundException`, `UnauthorizedException`.
- `Constants/`: Chứa `StatusConstants.cs` định nghĩa tập trung các hằng số trạng thái (Order, Payment, Inventory, Product).
- `Extensions/`: Chứa các phương thức mở rộng chung — `ActivityLogExtensions`, `QueryableExtensions` (Pagination).
- `Migrations/`: EF Core migration `InitialCreate` (2026-05-08).
- **Lưu ý về Specs**: Hệ thống sử dụng cột `specifications` (JSONB) trong bảng `Products` thay vì bảng riêng lẻ để tăng tính linh hoạt.

### 📋 Controllers (23)

`ActivityLogsController`, `AuthController`, `BannersController`, `BrandsController`, `CategoriesController`, `CouponsController`, `DashboardController`, `FlashSalesController`, `InventoryController`, `NewsCategoriesController`, `NewsController`, `OrdersController`, `PaymentsController`, `ProductController`, `ProductImagesController`, `ProfileController`, `ReturnRequestsController`, `ReviewsController`, `ShipmentsController`, `SuppliersController`, `UploadController`, `UsersController`, `WishlistController`.

#### 🛠️ Service Details

- `ShipmentService` / `IShipmentService`: Quản lý vận chuyển đơn hàng theo quy trình QC + Đóng gói.
  - `CreateAsync` — Tạo phiếu giao hàng, tự động chuyển đơn sang Processing (3).
  - `UpdateAsync` — Cập nhật tracking code; nếu có tracking + QC pass → tự động chuyển đơn sang Shipping (4).
  - `MarkQcPassedAsync` — Đánh dấu QC pass/fail kèm ghi chú kiểm tra.
  - `MarkPackedAsync` — Ghi nhận nhân viên đóng gói (`packed_by`) và thời điểm.
  - `GetByOrderIdAsync` — Lấy thông tin vận chuyển theo đơn hàng.
  - **Luồng trạng thái**: `pending` → `qc_passed` → `packed` → `shipping` → `delivered`.

- `InventoryService` / `IInventoryService`: Quản lý nhập kho và biến động tồn kho.
  - `CreateReceiptAsync` — Tạo phiếu nhập kho (Draft). Stock chưa tăng ở bước này.
  - `CompleteReceiptAsync` — Duyệt phiếu → tăng `stock_quantity`, tạo `InventoryTransaction` type=1.
  - `CancelReceiptAsync` — Hủy phiếu nhập kèm lý do (không ảnh hưởng stock).
  - `AdjustStockAsync` — Điều chỉnh tồn kho thủ công (kiểm kê, hàng hỏng).
  - `GetTransactionsAsync` — Lịch sử biến động kho theo sản phẩm.
  - `GetStockStatusAsync` — Tổng quan tồn kho toàn bộ sản phẩm.

- `OrderService` / `IOrderService`: Vòng đời đơn hàng.
  - `CreateAsync` — Tạo đơn hàng từ giỏ hàng.
  - `GetAllAsync` — Danh sách đơn hàng (phân trang + lọc theo status/userId).
  - `GetByIdAsync` — Chi tiết đơn hàng.
  - `UpdateAsync` — Cập nhật trạng thái/thanh toán (ghi nhận `changedByUserId` để lưu vào lịch sử trạng thái).
  - `CancelAsync` — Hủy đơn hàng.
  - `GetStatusHistoryAsync` — Lịch sử thay đổi trạng thái.

- `PaymentService` / `IPaymentService`: Xử lý thanh toán.
  - `CreatePaymentAsync` — Tạo payment (COD, bank_transfer, VnPay).
  - `GetByOrderIdAsync` — Lấy thông tin thanh toán theo đơn.
  - `ConfirmBankTransferAsync` — Admin xác nhận chuyển khoản ngân hàng.
  - `CompleteCodPaymentAsync` — Hoàn tất thanh toán COD khi giao hàng.

- `VnPayService` / `IVnPayService`: Stub interface cho tích hợp VnPay (chưa implement).

- `ReviewService` / `IReviewService`: Xử lý nghiệp vụ đánh giá.
  - **Reviews**: `GetAllAsync`, `GetByIdAsync`, `UpdateActiveAsync`, `DeleteAsync`.
  - **Replies**: `CreateReplyAsync`, `UpdateReplyAsync`, `DeleteReplyAsync`.
  - **Images**: `AddImageAsync`, `DeleteImageAsync`, `GetImagesByReviewIdAsync`.
  - **Votes**: `ToggleVoteAsync`, `GetVoteCountAsync`, `HasUserVotedAsync`.

- `AuthService` / `IAuthService`: Xử lý xác thực và quản lý tài khoản. Sử dụng `IEmailTemplateService` để render nội dung email.
  - **Auth**: `LoginAsync`, `RegisterAsync`, `GoogleLoginAsync`, `RefreshTokenAsync`.
  - **Password Recovery**: `ForgotPasswordAsync`, `ResetPasswordAsync` (OTP Email qua template).

- `ProfileService` / `IProfileService`: Quản lý thông tin cá nhân và sổ địa chỉ.

- `UserService` / `IUserService`: Quản lý tài khoản người dùng (Admin CRUD).

- `ProductService` / `IProductService`: Quản lý danh mục sản phẩm và tìm kiếm nâng cao.
  - **CRUD**: `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` (Soft delete).
  - **Lookup**: `GetBySlugAsync`, `GetFullDetailsAsync`.
  - **Client API**: `GetPagedListAsync` với bộ lọc phức tạp (Price, Category, Brand, Sort).

- `ProductImageService` / `IProductImageService`: Upload và quản lý ảnh sản phẩm.

- `WishlistService` / `IWishlistService`: Sản phẩm yêu thích (tối đa 50/user, `ToggleAsync`, `GetByUserAsync`).

- `ReturnRequestService` / `IReturnRequestService`: Đổi trả hàng (kiểm tra 7 ngày, trạng thái Delivered, tự động hoàn kho khi Admin duyệt).

- `CouponService` / `ICouponService`: Quản lý mã giảm giá.

- `FlashSaleService` / `IFlashSaleService`: Khuyến mãi Flash Sale.

- `EmailNotificationService` / `IEmailNotificationService`: Thông báo email tự động (xác nhận đơn, cập nhật vận chuyển, kết quả đổi trả, OTP).

- `EmailTemplateService` / `IEmailTemplateService`: Quản lý và render HTML templates từ thư mục `Templates/`.

- `EmailService` / `IEmailService`: Gửi email cơ bản (SMTP).

- `CloudinaryService` / `ICloudinaryService`: Upload ảnh lên Cloudinary.

- `CategoryService` / `ICategoryService`: CRUD danh mục sản phẩm (hierarchical).

- `BrandService` / `IBrandService`: CRUD thương hiệu.

- `BannerService` / `IBannerService`: CRUD banner quảng cáo.

- `NewsService` / `INewsService`: CRUD tin tức.

- `NewsCategoryService` / `INewsCategoryService`: CRUD danh mục tin tức.

- `SupplierService` / `ISupplierService`: CRUD nhà cung cấp.

- `ActivityLogService` / `IActivityLogService`: Ghi log hoạt động hệ thống.

- `DashboardService` / `IDashboardService`: Dữ liệu tổng hợp cho dashboard admin.

### 🗂️ MapperProfiles (14)

`AfterSalesProfile`, `BannerProfile`, `BrandProfile`, `CategoryProfile`, `InventoryProfile`, `MarketingProfile`, `NewsCategoryProfile`, `NewsProfile`, `OrderProfile`, `ProductImageProfile`, `ProductProfile`, `ReviewProfile`, `SupplierProfile`, `UserProfile`.

### ⚙️ Quy trình xử lý (Data Flow)

`Request` → `Controller` → `Service` → `UnitOfWork` → `Repository` → `Database`
- **Lưu ý**: Tất cả các services phải sử dụng `IUnitOfWork` thay vì `AppDbContext` trực tiếp để đảm bảo tính nhất quán và quản lý transaction hiệu quả.

---

## 3. Chi tiết Frontend (Angular)

### 📂 Cơ cấu thư mục chi tiết

- `src/app/core/`: Chứa các thành phần cốt lõi dùng chung:
  - `constants/`: `translations.ts` — Bản dịch đa ngôn ngữ.
  - `guards/`: `auth-guard.ts` (xác thực đăng nhập), `role.guard.ts` (phân quyền admin/staff).
  - `interceptors/`: `auth-interceptor.ts`, `jwt.interceptor.ts` (tự động gắn token).
  - `mocks/`: `product.mock.ts` — Dữ liệu mock cho development.
  - `models/` (11 files): TypeScript interfaces/types cho toàn bộ entities.
  - `pipes/`: `translate.pipe.ts` — Pipe đa ngôn ngữ.
  - `services/` (25 files): Tất cả HTTP services gọi API backend.
  - `utils/`: (3 files) Chứa các helper và utilities: `auth.util.ts`, `theme.util.ts`, `comparison.service.ts` (helper service).

- `src/app/shared/`: Chứa các UI Components dùng lại ở nhiều nơi:
  - **Components**: `navbar/`, `footer/`, `mega-menu/`, `search-overlay/`, `toast/`, `loading/`, `review-modal/`, `wishlist-toggle/`, `verified-badge/`, `coupon-input.ts`, `flash-sale-badge.ts`, `flash-sale-section.ts`.
  - **Services**: `modal.service.ts`.
  - **Directives**: (trống — dự phòng).
  - **Pipes**: (trống — dự phòng).

- `src/app/features/`: Chia theo từng phân hệ chức năng (11 modules):
  - `admin/` (18 sub-modules): Dashboard, Manage Product, Manage Order, Inventory, Category Hierarchy, Customer CRM, Employee Management, CMS Banner, Brand Management, CMS News, Reviews, Coupons, Flash Sales, Activity Logs, Return Requests, Supplier Management, Inventory Receipts (list/form/detail), Packing Slip (component).
  - `employee/` (5 sub-modules): Emp Orders, Emp Products, Emp Reviews, Emp Customers, Packing Slip.
  - `home/`: Trang chủ với banner, sản phẩm nổi bật, flash sale.
  - `product/` (4 sub-modules): Product List, Product Detail, Category, Reviews.
  - `user/` (7 sub-modules): Profile, Orders, Order Tracking, Return Request, Wishlist, Settings, User Layout.
  - `auth/` (5 sub-modules): Login, Register, Forgot Password, Verify Email, Internal Login (portal).
  - `cart/` (4 sub-modules): Cart Page, Checkout, Payment, VnPay Return.
  - `build-pc/`: Cấu hình PC tương thích (Socket, PSU).
  - `comparison/`: So sánh sản phẩm side-by-side.
  - `tech-news/`: Tin tức công nghệ + News Detail.
  - `terms/`: Trang điều khoản sử dụng.

- `src/app/layouts/` (3 layouts): `AdminLayout`, `EmployeeLayout`, `MainLayout`.

### 🎨 Styling & UI

- **Tailwind CSS 4** (v4.1.12): Sử dụng engine mới nhất với PostCSS plugin.
- **Responsive Design**: Hỗ trợ đầy đủ Mobile, Tablet và Desktop.
- **SSR**: Hỗ trợ Server-Side Rendering qua `@angular/ssr` + Express.

#### 🛠️ Frontend Service Details (Core — 27 files)

- `AuthService`: Đăng nhập, đăng ký, Google login, refresh token, quên mật khẩu.
- `OrderService`: Tạo đơn, lấy lịch sử, hủy đơn, cập nhật trạng thái.
- `CartService`: Quản lý giỏ hàng (RxJS BehaviorSubject).
- `WishlistService`: Sản phẩm yêu thích (Angular Signals).
- `ProductService`: Danh sách, chi tiết, tìm kiếm, lọc sản phẩm.
- `ReviewService`: Lấy đánh giá, tạo, phản hồi, vote.
- `PaymentService`: Tạo thanh toán, xác nhận chuyển khoản.
- `InventoryService`: CRUD phiếu nhập kho, complete, cancel, adjust stock, stock-status.
- `ShipmentService`: Tạo shipment, cập nhật, mark QC, mark packed, get by orderId.
- `SupplierService`: CRUD nhà cung cấp.
- `CouponService`: CRUD coupon, validate, apply.
- `FlashSaleService`: CRUD flash sale campaigns.
- `DashboardService`: Thống kê dashboard admin.
- `ActivityLogService`: Xem log hoạt động.
- `ReturnRequestService`: Gửi/duyệt yêu cầu đổi trả.
- `BannerService`: CRUD banner.
- `BrandService`: CRUD thương hiệu.
- `CategoryService`: CRUD danh mục (hierarchical).
- `NewsService`: CRUD tin tức + danh mục tin tức.
- `CloudinaryService`: Upload ảnh lên Cloudinary.
- `UserService`: Quản lý người dùng (Admin).
- `ShopStateService`: State management cho shop (filters, pagination).
- `LanguageService`: Quản lý ngôn ngữ giao diện.
- `ThemeService` (`core/utils/theme.util.ts`): Chế độ sáng/tối.
- `ToastService`: Hiển thị thông báo toast.
- `ComparisonService` (`core/services/comparison.service.ts`): So sánh sản phẩm.
- `ModalService` (`shared/services/modal.service.ts`): Quản lý modal dialogs.
- `AuthUtils` (`core/utils/auth.util.ts`): Quản lý token và login state.

---

## 4. Cơ sở dữ liệu (Database)

Hệ thống sử dụng **PostgreSQL 16** với **34 bảng** được tổ chức thành 11 nhóm:

- **Identity (4)**: `users`, `addresses`, `refresh_tokens`, `password_reset_tokens`.
- **Catalog (4)**: `products` (JSONB specs), `categories` (hierarchical), `brands`, `product_images`.
- **Shopping (1)**: `cart_items`.
- **Sales (4)**: `orders`, `order_items`, `order_status_history`, `payments`.
- **Logistics (1)**: `shipments` (status: pending/packed/shipping/delivered/failed, ghi nhận QC + người đóng gói).
- **Inventory (4)**: `suppliers`, `inventory_receipts`, `inventory_receipt_items`, `inventory_transactions`.
- **After-Sales (3)**: `return_requests`, `return_request_items`, `return_request_images`.
- **Marketing (5)**: `coupons`, `coupon_usages`, `flash_sales`, `flash_sale_items`, `banners`.
- **CMS (2)**: `news`, `news_categories`.
- **Engagement (5)**: `reviews`, `review_images`, `review_replies`, `review_helpful_votes`, `wishlists`.
- **Audit (1)**: `activity_logs`.

> **Lưu ý**:
> - Sử dụng extension `pgcrypto` để hỗ trợ `gen_random_uuid()`.
> - Cột `order_code` trong bảng `orders` có ràng buộc `UNIQUE` để hỗ trợ quy trình seed dữ liệu và tránh trùng lặp mã đơn hàng.

---

## 5. Danh sách DTOs Key

### 📦 DTOs (63 files)

- **Order**: `CreateOrderDto`, `OrderDto`, `OrderDetailDto`, `OrderItemDto`, `UpdateOrderDto`, `OrderStatusHistoryDto`.
- **Payment**: `CreatePaymentDto`, `PaymentDto`, `PaymentConfig`, `VnPayCallbackDto`.
- **Shipment**: `ShipmentDto`, `CreateShipmentDto`, `UpdateShipmentDto`, `MarkQcDto`.
- **Inventory**: `InventoryDto` (Receipt + Transaction + StockStatus + AdjustStock).
- **Product**: `CreateProductDto`, `ProductDto`, `ProductFullDto`, `ProductListItemDto`, `UpdateProductDto`.
- **Product Images**: `CreateProductImageDto`, `ProductImageDto`.
- **Auth**: `AuthDTOs` (Login, Register, Token, Google login, Forgot/Reset password).
- **User**: `CreateUserDto`, `UserDto`, `UpdateUserDto`, `UpdateProfileDto`, `AddressDto`.
- **Review**: `ReviewDto`, `ReviewImageDto`, `ReviewReplyDto`, `ReviewHelpfulVoteDto`, `CreateReviewImageDto`, `CreateReviewReplyDto`, `UpdateReviewReplyDto`, `ToggleVoteDto`.
- **Return**: `CreateReturnRequestDto`, `ReturnRequestDto`, `UpdateReturnRequestDto`.
- **Category**: `CategoryDto`, `CreateCategoryDto`, `UpdateCategoryDto`.
- **Brand**: `BrandDto`, `CreateBrandDto`, `UpdateBrandDto`.
- **Banner**: `BannerDto`, `CreateBannerDto`, `UpdateBannerDto`.
- **News**: `NewsDto`, `CreateNewsDto`, `UpdateNewsDto`, `NewsCategoryDto`, `CreateNewsCategoryDto`, `UpdateNewsCategoryDto`.
- **Coupon**: `CouponDto`, `CreateCouponDto`, `UpdateCouponDto`, `CouponValidationDto`.
- **Flash Sale**: `FlashSaleDto`, `CreateFlashSaleDto`, `UpdateFlashSaleDto`.
- **Supplier**: `SupplierDto`, `CreateSupplierDto`, `UpdateSupplierDto` (trong `SupplierDto.cs`).
- **Dashboard**: `DashboardDto`.
- **Activity Log**: `ActivityLogDto`.
- **Wishlist**: `WishlistItemDto`.
- **Pagination**: `PageResultDto`.

---

## 6. Shared Frontend Foundation

### 📦 Models (`core/models/` — 11 files)

- `auth.models.ts`: Login, Register, Token DTOs.
- `order.model.ts`: `OrderDto`, `OrderDetailDto`, `PaymentDto`, `ShipmentDto`, `OrderStatusHistoryDto`, `ReturnRequestDto`.
- `inventory.model.ts`: `InventoryReceipt`, `InventoryTransaction`, `StockStatus`, `AdjustStockDto`.
- `supplier.model.ts`: `Supplier`, `CreateSupplierDto`, `UpdateSupplierDto`.
- `product.model.ts`: Product, ProductImage, ProductListItem interfaces.
- `user.model.ts`: User interface.
- `review.model.ts`: Review, ReviewReply, ReviewImage, Vote interfaces.
- `banner.model.ts`: Banner interfaces.
- `brand.model.ts`: Brand interfaces.
- `category.model.ts`: Category interfaces.
- `news.model.ts`: News, NewsCategory interfaces.

### 🛣️ Routes

#### Main Layout (`/`)

- `/`: Trang chủ (HomeComponent).
- `/tech-news`: Danh sách tin tức công nghệ.
- `/tech-news/:id`: Chi tiết tin tức.
- `/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/verify-email`: Xác thực.
- `/terms`: Điều khoản sử dụng.
- `/product/list`: Danh sách sản phẩm.
- `/product/category/:id`: Sản phẩm theo danh mục.
- `/product/:slug`: Chi tiết sản phẩm.
- `/build-pc`: Cấu hình PC tương thích.
- `/comparison`: So sánh sản phẩm.
- `/cart`: Giỏ hàng.
- `/cart/checkout`: Thanh toán (authGuard).
- `/cart/payment`: Xử lý thanh toán (authGuard).
- `/cart/vnpay-return`: Callback VnPay.
- `/payment/vnpay-return`: Callback VnPay (alias route).
- `/user/profile`, `/user/orders`, `/user/order-tracking/:id`, `/user/return-request`, `/user/return-request/:id`, `/user/wishlist`, `/user/settings`: Trang người dùng (trong UserLayout — authGuard).

#### Admin Layout (`/admin` — roleGuard: admin)

- `/admin/dashboard`: Dashboard tổng quan.
- `/admin/manage-product`: Quản lý sản phẩm.
- `/admin/manage-order`: Quản lý đơn hàng.
- `/admin/inventory`: Tổng quan tồn kho.
- `/admin/category-hierarchy`: Quản lý danh mục phân cấp.
- `/admin/customer-crm`: Quản lý khách hàng.
- `/admin/employee-management`: Quản lý nhân viên.
- `/admin/cms-banner`: Quản lý banner.
- `/admin/brand-management`: Quản lý thương hiệu.
- `/admin/cms-news`: Quản lý tin tức.
- `/admin/reviews`: Quản lý đánh giá.
- `/admin/coupons`: Quản lý mã giảm giá.
- `/admin/flash-sales`: Quản lý Flash Sale.
- `/admin/activity-logs`: Log hoạt động.
- `/admin/return-requests`: Quản lý đổi trả.
- `/admin/suppliers`: Quản lý nhà cung cấp.
- `/admin/inventory-receipts`: Danh sách phiếu nhập kho.
- `/admin/inventory-receipts/new`: Tạo phiếu nhập kho.
- `/admin/inventory-receipts/:id`: Chi tiết phiếu nhập kho.

#### Employee Layout (`/employee` — roleGuard: admin, staff)

- `/employee/orders`: Xử lý đơn hàng.
- `/employee/products`: Xem kho sản phẩm.
- `/employee/reviews`: Phản hồi đánh giá.
- `/employee/customers`: Tra cứu khách hàng.
- `/employee/packing-slip`: In phiếu đóng gói + QC.

#### Portal

- `/portal`: Internal login cho admin/staff.

---

## 7. Quy chuẩn Kỹ thuật (Technical Standards)

### Backend

- **Dependency Injection**: Luôn đăng ký Service trong `Program.cs`.
- **Async/Await**: Sử dụng lập trình bất đồng bộ cho mọi tác vụ I/O.
- **Error Handling**: Sử dụng `ApiResponse<T>` wrapper chuẩn (ApiResponse.Ok / ApiResponse.Fail) cho tất cả controller. Exception Middleware xử lý tập trung. Custom exceptions: `BadRequestException`, `NotFoundException`, `UnauthorizedException`.
- **Status Management**: Tuyệt đối không dùng magic numbers. Tất cả trạng thái phải dùng hằng số định nghĩa trong `backend.Constants.StatusConstants`.
- **Namespace Imports**: Luôn import `backend.Constants` khi sử dụng các hằng số trạng thái để đảm bảo code được build thành công.

- **Authorization**: `[Authorize(Roles = "admin,staff")]` cho các API quản trị.
- **Authentication**: JWT Bearer + Refresh Token.
- **API Docs**: Scalar API Reference (development mode).
- **NuGet Packages**: AutoMapper, BCrypt.Net-Next, CloudinaryDotNet, Google.Apis.Auth, JwtBearer, Npgsql, Scalar.AspNetCore.

### Frontend

- **Type-safe**: Tuyệt đối không sử dụng `any`, luôn định nghĩa Interface/Type.
- **Strict Mode**: Tuân thủ Angular Strict Mode để hạn chế lỗi runtime.
- **Shared Components**: Các thành phần giao diện lặp lại phải được đưa vào `shared/`.
- **Signals**: Ưu tiên Angular Signals cho state management thay vì RxJS Subject khi có thể.
- **Lazy Loading**: Tất cả feature modules sử dụng `loadComponent`/`loadChildren`.
- **SSR**: Server-Side Rendering qua `@angular/ssr` + Express server.
- **Linting**: ESLint + Prettier + angular-eslint.
- **Testing**: Vitest cho unit tests.
- **Libraries**: jsPDF + jspdf-autotable (xuất PDF), @abacritt/angularx-social-login (Google Login).

---

## 8. Testing Resources

### Backend Tests (`backend.Tests/`)

- **Framework**: xUnit (hoặc tương đương .NET test framework).
- **Files**: `EfCoreTests.cs`, `InspectTests.cs`.
- **Service Tests** (24 files): `ActivityLogServiceTests`, `AuthServiceTests`, `BannerServiceTests`, `BrandServiceTests`, `CategoryServiceTests`, `CloudinaryServiceTests`, `CouponServiceTests`, `DashboardServiceTests`, `EmailNotificationServiceTests`, `FlashSaleServiceTests`, `InventoryServiceTests`, `NewsCategoryServiceTests`, `NewsServiceTests`, `OrderServiceTests`, `PaymentServiceTests`, `ProductImageServiceTests`, `ProductServiceTests`, `ProfileServiceTests`, `ReturnRequestServiceTests`, `ReviewServiceTests`, `ShipmentServiceTests`, `SupplierServiceTests`, `UserServiceTests`, `WishlistServiceTests`.

### Documentation

- **API Documentation**: [API_DOCUMENTATION.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/API_DOCUMENTATION.md).
- **Sample Data**: [SAMPLE_DATA.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/SAMPLE_DATA.md).
- **Test Plan**: [TEST_PLAN.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/md/TEST_PLAN.md).
- **Unit Test Guide**: [UNIT_TEST_GUIDE.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/UNIT_TEST_GUIDE.md).
- **Unit Test Checklist**: [UNIT_TEST_CHECKLIST.md](file:///d:/Workspace/Cong_Nghe_Web/.idea/UNIT_TEST_CHECKLIST.md) (CSV: [unit_test_checklist.csv](file:///d:/Workspace/Cong_Nghe_Web/.idea/unit_test_checklist.csv)).
- **Unit Test Results**: [UNIT_TEST_RESULTS.md](file:///d:/Workspace/Cong_Nghe_Web/idea/UNIT_TEST_RESULTS.md).
- **Class Diagram Data**: [class_diagram.md](file:///d:/Workspace/Cong_Nghe_Web/class_diagram.md).
- **Additional Docs** (`.idea/md/`): BUSINESS_FLOW, DATABASE_ANALYSIS, ERD, FEATURE_LIST, PROJECT_AUDIT, REFACTOR_PLAN, ROADMAP, SKILL, SYSTEM_ARCHITECTURE, TECHNICAL_DEBT, TEST_CASES, BUG_RISK_ANALYSIS.
- **Diagrams** (`.idea/md/`): `business-flow.drawio`, `erd-diagram.drawio`, `usecase-diagram.drawio`.
- **Diagrams** (`.idea/`): `ecommerce-linhkien-flow.drawio`.

---

## 9. Quy chuẩn thiết kế Class Diagram (UML)

Hệ thống tuân thủ các quy chuẩn thiết kế Class Diagram chuyên nghiệp để đảm bảo tính rõ ràng và khả năng bảo trì.

### 9.1. Ký hiệu Phạm vi truy cập (Access Modifiers)
- `+` (**Public**): Mọi đối tượng đều có thể truy cập. Áp dụng cho các Public Methods trong Services và Controllers.
- `-` (**Private**): Chỉ đối tượng trong class mới có thể truy cập. Áp dụng cho các fields ẩn trong implementation.
- `#` (**Protected**): Class hiện tại và các class kế thừa có thể truy cập.
- `~` (**Internal/Package**): Truy cập trong cùng một assembly/package.

### 9.2. Các loại Quan hệ (Relationships)
- **Inheritance** (Kế thừa): Thể hiện qua mũi tên rỗng, dùng cho các thực thể kế thừa từ lớp cơ sở.
- **Association** (Liên kết): Đường thẳng đơn thuần, thể hiện mối quan hệ giữa các đối tượng độc lập (ví dụ: `User` và `Order`).
- **Aggregation** (Thu gom - Quan hệ độc lập): Hình thoi rỗng, đối tượng thành phần có thể tồn tại độc lập với đối tượng chứa.
- **Composition** (Chứa đựng - Quan hệ sống còn): Hình thoi đặc, đối tượng thành phần không thể tồn tại nếu đối tượng chứa bị xóa (ví dụ: `Order` và `OrderItem`).

### 9.3. Đa dạng số lượng (Multiplicity)
- `1`: Quan hệ duy nhất.
- `0...1`: Không có hoặc có 1.
- `*` hoặc `0...*`: Không có hoặc có nhiều.
- `1...*`: Ít nhất phải có 1.

> Chi tiết cấu trúc các lớp theo chuẩn này được trình bày tại [class_diagram.md](file:///d:/Workspace/Cong_Nghe_Web/class_diagram.md).
