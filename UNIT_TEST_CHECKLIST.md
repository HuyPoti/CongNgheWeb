
Tài liệu này liệt kê **tất cả** các hàm trong lớp Service cần được kiểm thử.

---

| Class Service | Tên Hàm (Method) | Mức độ ưu tiên | Trạng thái |
| :--- | :--- | :--- | :--- |
| **ActivityLogService** | LogAsync | 🟢 Thấp | [ ] |
| | GetLogsAsync | 🟢 Thấp | [ ] |
| **AuthService** | LoginAsync | 🔴 Cao | [ ] |
| | RegisterAsync | 🔴 Cao | [ ] |
| | GoogleLoginAsync | 🟡 Trung bình | [ ] |
| | ForgotPasswordAsync | 🟡 Trung bình | [ ] |
| | ResetPasswordAsync | 🔴 Cao | [ ] |
| | RefreshTokenAsync | 🔴 Cao | [ ] |
| | LogoutAsync | 🟡 Trung bình | [ ] |
| | VerifyEmailAsync | 🔴 Cao | [ ] |
| | ResendEmailAsync | 🟡 Trung bình | [ ] |
| | ChangePasswordAsync | 🟡 Trung bình | [ ] |
| **BannerService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetPublicAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🟢 Thấp | [ ] |
| | UpdateAsync | 🟢 Thấp | [ ] |
| | DeleteAsync | 🟢 Thấp | [ ] |
| **BrandService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | GetBySlugAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| **CategoryService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | GetBySlugAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| **CloudinaryService** | UploadImageAsync | 🟢 Thấp | [ ] |
| | DeleteImageAsync | 🟢 Thấp | [ ] |
| **CouponService** | CreateAsync | 🔴 Cao | [ ] |
| | GetAllAsync | 🟡 Trung bình | [ ] |
| | ValidateAsync | 🔴 Cao | [ ] |
| | ApplyAsync | 🔴 Cao | [ ] |
| | DeactivateAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🟡 Trung bình | [ ] |
| **DashboardService** | GetOverviewAsync | 🟢 Thấp | [ ] |
| | GetRevenueChartAsync | 🟢 Thấp | [ ] |
| | GetTopProductsAsync | 🟢 Thấp | [ ] |
| | GetTopCustomersAsync | 🟢 Thấp | [ ] |
| **EmailNotificationService**| SendOrderConfirmedEmail | 🟢 Thấp | [ ] |
| | SendOrderShippingEmail | 🟢 Thấp | [ ] |
| | SendOrderDeliveredEmail | 🟢 Thấp | [ ] |
| | SendReturnProcessedEmail | 🟢 Thấp | [ ] |
| **EmailService** | SendEmailAsync | 🟢 Thấp | [ ] |
| **FlashSaleService** | CreateAsync | 🔴 Cao | [ ] |
| | GetAllAsync | 🟡 Trung bình | [ ] |
| | GetActiveAsync | 🔴 Cao | [ ] |
| | GetFlashPriceAsync | 🔴 Cao | [ ] |
| | AddItemAsync | 🔴 Cao | [ ] |
| | RemoveItemAsync | 🟡 Trung bình | [ ] |
| **InventoryService** | CreateReceiptAsync | 🔴 Cao | [ ] |
| | GetReceiptByIdAsync | 🟢 Thấp | [ ] |
| | GetReceiptsAsync | 🟢 Thấp | [ ] |
| | CompleteReceiptAsync | 🔴 Cao | [ ] |
| | CancelReceiptAsync | 🔴 Cao | [ ] |
| | AdjustStockAsync | 🔴 Cao | [ ] |
| | GetTransactionsAsync | 🟡 Trung bình | [ ] |
| | GetStockStatusAsync | 🟡 Trung bình | [ ] |
| **NewsCategoryService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| **NewsService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| **OrderService** | CreateAsync | 🔴 Cao | [ ] |
| | GetAllAsync | 🟡 Trung bình | [ ] |
| | GetByIdAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🔴 Cao | [ ] |
| | CancelAsync | 🔴 Cao | [ ] |
| | GetStatusHistoryAsync | 🟢 Thấp | [ ] |
| **PaymentService** | CreatePaymentAsync | 🔴 Cao | [ ] |
| | GetByOrderIdAsync | 🟢 Thấp | [ ] |
| | ConfirmBankTransferAsync | 🔴 Cao | [ ] |
| | CompleteCodPaymentAsync | 🟡 Trung bình | [ ] |
| **ProductImageService** | GetByProductIdAsync | 🟢 Thấp | [ ] |
| | AddAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| **ProductService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetFullByIdAsync | 🟢 Thấp | [ ] |
| | GetFullBySlugAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | GetBySlugAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🔴 Cao | [ ] |
| | UpdateAsync | 🔴 Cao | [ ] |
| | DeleteAsync | 🔴 Cao | [ ] |
| | GetProductListAsync | 🔴 Cao | [ ] |
| **ProfileService** | GetProfileAsync | 🟢 Thấp | [ ] |
| | UpdateProfileAsync | 🟡 Trung bình | [ ] |
| **ReturnRequestService** | GetAllAsync | 🟡 Trung bình | [ ] |
| | GetByIdAsync | 🟡 Trung bình | [ ] |
| | GetByOrderIdAsync | 🟡 Trung bình | [ ] |
| | CreateAsync | 🔴 Cao | [ ] |
| | ProcessAsync | 🔴 Cao | [ ] |
| **ReviewService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | GetByProductIdAsync | 🟢 Thấp | [ ] |
| | UpdateActiveAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| | CreateReplyAsync | 🟡 Trung bình | [ ] |
| | UpdateReplyAsync | 🟡 Trung bình | [ ] |
| | DeleteReplyAsync | 🟡 Trung bình | [ ] |
| | AddImageAsync | 🟢 Thấp | [ ] |
| | DeleteImageAsync | 🟢 Thấp | [ ] |
| | GetImagesByReviewIdAsync | 🟢 Thấp | [ ] |
| | ToggleVoteAsync | 🔴 Cao | [ ] |
| | GetVoteCountAsync | 🟢 Thấp | [ ] |
| | HasUserVotedAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🔴 Cao | [ ] |
| **ShipmentService** | CreateAsync | 🔴 Cao | [ ] |
| | UpdateAsync | 🔴 Cao | [ ] |
| | GetByOrderIdAsync | 🟡 Trung bình | [ ] |
| | MarkQcPassedAsync | 🔴 Cao | [ ] |
| | MarkPackedAsync | 🔴 Cao | [ ] |
| **SupplierService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🟡 Trung bình | [ ] |
| | UpdateAsync | 🟡 Trung bình | [ ] |
| | DeleteAsync | 🟡 Trung bình | [ ] |
| **UserService** | GetAllAsync | 🟢 Thấp | [ ] |
| | GetByIdAsync | 🟢 Thấp | [ ] |
| | CreateAsync | 🔴 Cao | [ ] |
| | UpdateAsync | 🔴 Cao | [ ] |
| | DeleteAsync | 🔴 Cao | [ ] |
| **WishlistService** | GetByUserAsync | 🟢 Thấp | [ ] |
| | ToggleAsync | 🔴 Cao | [ ] |
| | IsInWishlistAsync | 🟢 Thấp | [ ] |
| | CountAsync | 🟢 Thấp | [ ] |

---

> [!IMPORTANT]
> Danh sách này bao gồm **tất cả** các hàm public trong các lớp Service. Hãy bắt đầu từ các hàm có mức độ ưu tiên **Đỏ (Cao)**.
