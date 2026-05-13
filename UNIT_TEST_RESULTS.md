# 📊 BÁO CÁO KẾT QUẢ KIỂM THỬ (UNIT TEST RESULTS) - CẬP NHẬT CHÍNH XÁC

*Ngày báo cáo: 00:21:47 13/5/2026*

> [!NOTE]
> Báo cáo này được tổng hợp dựa trên các hàm kiểm thử thực tế tìm thấy trong mã nguồn (`backend.Tests/Services`).

## 📈 Tổng quan

| Chỉ số | Giá trị |
| :--- | :--- |
| **Tổng số hàm cần test** | 134 |
| **Đã hoàn thành (Done)** | 132 |
| **Chưa thực hiện (Pending)** | 2 |
| **Tỷ lệ hoàn thành** | 98.51% |

## 📑 Chi tiết theo Service


### 📦 ActivityLogService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `LogAsync` | ✅ Done | 3 |
| `GetLogsAsync` | ✅ Done | 3 |

### 📦 AuthService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `LoginAsync` | ✅ Done | 6 |
| `RegisterAsync` | ✅ Done | 2 |
| `GoogleLoginAsync` | ✅ Done | 1 |
| `ForgotPasswordAsync` | ✅ Done | 3 |
| `ResetPasswordAsync` | ✅ Done | 3 |
| `RefreshTokenAsync` | ✅ Done | 3 |
| `LogoutAsync` | ✅ Done | 2 |
| `VerifyEmailAsync` | ✅ Done | 5 |
| `ResendEmailAsync` | ✅ Done | 2 |
| `ChangePasswordAsync` | ✅ Done | 5 |

### 📦 BannerService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetPublicAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 2 |
| `UpdateAsync` | ✅ Done | 2 |
| `DeleteAsync` | ✅ Done | 2 |

### 📦 BrandService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `GetBySlugAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 2 |
| `UpdateAsync` | ✅ Done | 3 |
| `DeleteAsync` | ✅ Done | 2 |

### 📦 CategoryService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 2 |
| `GetByIdAsync` | ✅ Done | 1 |
| `GetBySlugAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 3 |
| `UpdateAsync` | ✅ Done | 3 |
| `DeleteAsync` | ✅ Done | 3 |

### 📦 CloudinaryService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `UploadImageAsync` | ✅ Done | 3 |
| `DeleteImageAsync` | ⏳ Pending | 0 |

### 📦 CouponService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `CreateAsync` | ✅ Done | 6 |
| `GetAllAsync` | ✅ Done | 1 |
| `ValidateAsync` | ✅ Done | 6 |
| `ApplyAsync` | ✅ Done | 2 |
| `DeactivateAsync` | ✅ Done | 2 |
| `UpdateAsync` | ✅ Done | 2 |

### 📦 DashboardService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetOverviewAsync` | ✅ Done | 1 |
| `GetRevenueChartAsync` | ✅ Done | 1 |
| `GetTopProductsAsync` | ✅ Done | 1 |
| `GetTopCustomersAsync` | ✅ Done | 1 |

### 📦 EmailNotificationService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `SendOrderConfirmedEmail` | ✅ Done | 1 |
| `SendOrderShippingEmail` | ✅ Done | 1 |
| `SendOrderDeliveredEmail` | ✅ Done | 1 |
| `SendReturnProcessedEmail` | ✅ Done | 1 |

### 📦 EmailService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `SendEmailAsync` | ⏳ Pending | 0 |

### 📦 FlashSaleService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `CreateAsync` | ✅ Done | 4 |
| `GetAllAsync` | ✅ Done | 1 |
| `GetActiveAsync` | ✅ Done | 2 |
| `GetFlashPriceAsync` | ✅ Done | 2 |
| `AddItemAsync` | ✅ Done | 3 |
| `RemoveItemAsync` | ✅ Done | 2 |

### 📦 InventoryService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `CreateReceiptAsync` | ✅ Done | 1 |
| `GetReceiptByIdAsync` | ✅ Done | 1 |
| `GetReceiptsAsync` | ✅ Done | 1 |
| `CompleteReceiptAsync` | ✅ Done | 1 |
| `CancelReceiptAsync` | ✅ Done | 1 |
| `AdjustStockAsync` | ✅ Done | 1 |
| `GetTransactionsAsync` | ✅ Done | 1 |
| `GetStockStatusAsync` | ✅ Done | 1 |

### 📦 NewsCategoryService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 1 |
| `UpdateAsync` | ✅ Done | 1 |
| `DeleteAsync` | ✅ Done | 1 |

### 📦 NewsService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 1 |
| `UpdateAsync` | ✅ Done | 1 |
| `DeleteAsync` | ✅ Done | 1 |

### 📦 OrderService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `CreateAsync` | ✅ Done | 2 |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `UpdateAsync` | ✅ Done | 2 |
| `CancelAsync` | ✅ Done | 3 |
| `GetStatusHistoryAsync` | ✅ Done | 1 |

### 📦 PaymentService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `CreatePaymentAsync` | ✅ Done | 4 |
| `GetByOrderIdAsync` | ✅ Done | 2 |
| `ConfirmBankTransferAsync` | ✅ Done | 4 |
| `CompleteCodPaymentAsync` | ✅ Done | 2 |

### 📦 ProductImageService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetByProductIdAsync` | ✅ Done | 2 |
| `AddAsync` | ✅ Done | 2 |
| `DeleteAsync` | ✅ Done | 2 |

### 📦 ProductService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetFullByIdAsync` | ✅ Done | 1 |
| `GetFullBySlugAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `GetBySlugAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 1 |
| `UpdateAsync` | ✅ Done | 1 |
| `DeleteAsync` | ✅ Done | 1 |
| `GetProductListAsync` | ✅ Done | 1 |

### 📦 ProfileService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetProfileAsync` | ✅ Done | 2 |
| `UpdateProfileAsync` | ✅ Done | 2 |

### 📦 ReturnRequestService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `GetByOrderIdAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 1 |
| `ProcessAsync` | ✅ Done | 1 |

### 📦 ReviewService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `GetByProductIdAsync` | ✅ Done | 1 |
| `UpdateActiveAsync` | ✅ Done | 1 |
| `DeleteAsync` | ✅ Done | 2 |
| `CreateReplyAsync` | ✅ Done | 1 |
| `UpdateReplyAsync` | ✅ Done | 1 |
| `DeleteReplyAsync` | ✅ Done | 1 |
| `AddImageAsync` | ✅ Done | 1 |
| `DeleteImageAsync` | ✅ Done | 1 |
| `GetImagesByReviewIdAsync` | ✅ Done | 1 |
| `ToggleVoteAsync` | ✅ Done | 3 |
| `GetVoteCountAsync` | ✅ Done | 1 |
| `HasUserVotedAsync` | ✅ Done | 2 |
| `CreateAsync` | ✅ Done | 4 |

### 📦 ShipmentService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `CreateAsync` | ✅ Done | 4 |
| `UpdateAsync` | ✅ Done | 1 |
| `GetByOrderIdAsync` | ✅ Done | 1 |
| `MarkQcPassedAsync` | ✅ Done | 2 |
| `MarkPackedAsync` | ✅ Done | 2 |

### 📦 SupplierService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 1 |
| `UpdateAsync` | ✅ Done | 1 |
| `DeleteAsync` | ✅ Done | 1 |

### 📦 UserService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetAllAsync` | ✅ Done | 1 |
| `GetByIdAsync` | ✅ Done | 1 |
| `CreateAsync` | ✅ Done | 1 |
| `UpdateAsync` | ✅ Done | 1 |
| `DeleteAsync` | ✅ Done | 1 |

### 📦 WishlistService

| Hàm (Method) | Trạng thái | Số lượng Test Case |
| :--- | :--- | :--- |
| `GetByUserAsync` | ✅ Done | 1 |
| `ToggleAsync` | ✅ Done | 2 |
| `IsInWishlistAsync` | ✅ Done | 1 |
| `CountAsync` | ✅ Done | 1 |

## 🧪 Chi tiết các Test Case đã viết

### 🔹 ActivityLogService.LogAsync
- `LogAsync_EmptyUserId_ThrowsBadRequestException`
- `LogAsync_EmptyAction_ThrowsBadRequestException`
- `LogAsync_ValidInput_CreatesLogAndReturnsDto`

### 🔹 ActivityLogService.GetLogsAsync
- `GetLogsAsync_NoFilters_ReturnsAllLogsPaginated`
- `GetLogsAsync_FilterByUserId_ReturnsOnlyUserLogs`
- `GetLogsAsync_FilterByDateRange_ReturnsCorrectLogs`

### 🔹 AuthService.LoginAsync
- `LoginAsync_UserNotFound_ThrowsException`
- `LoginAsync_GoogleAccount_ThrowsException`
- `LoginAsync_WrongPassword_ThrowsException`
- `LoginAsync_InactiveAccount_ThrowsException`
- `LoginAsync_UnverifiedEmail_ThrowsException`
- `LoginAsync_ValidCredentials_ReturnsAuthResponse`

### 🔹 AuthService.RegisterAsync
- `RegisterAsync_DuplicateEmail_ThrowsException`
- `RegisterAsync_ValidInput_CreatesUserAndSendsEmail`

### 🔹 AuthService.GoogleLoginAsync
- `GoogleLoginAsync_InvalidToken_ThrowsException`

### 🔹 AuthService.ForgotPasswordAsync
- `ForgotPasswordAsync_UserNotFound_ThrowsException`
- `ForgotPasswordAsync_GoogleAccount_ThrowsException`
- `ForgotPasswordAsync_ValidUser_SendsOtpEmail`

### 🔹 AuthService.ResetPasswordAsync
- `ResetPasswordAsync_UserNotFound_ThrowsException`
- `ResetPasswordAsync_InvalidOtp_ThrowsException`
- `ResetPasswordAsync_ValidOtp_ResetsPassword`

### 🔹 AuthService.RefreshTokenAsync
- `RefreshTokenAsync_InvalidToken_ThrowsException`
- `RefreshTokenAsync_ExpiredToken_ThrowsException`
- `RefreshTokenAsync_ValidToken_ReturnsNewTokens`

### 🔹 AuthService.LogoutAsync
- `LogoutAsync_ValidToken_RevokesToken`
- `LogoutAsync_InvalidToken_DoesNotThrow`

### 🔹 AuthService.VerifyEmailAsync
- `VerifyEmailAsync_UserNotFound_ThrowsException`
- `VerifyEmailAsync_AlreadyVerified_ThrowsException`
- `VerifyEmailAsync_WrongOtp_ThrowsException`
- `VerifyEmailAsync_ExpiredOtp_ThrowsException`
- `VerifyEmailAsync_ValidOtp_VerifiesEmail`

### 🔹 AuthService.ResendEmailAsync
- `ResendEmailAsync_UserNotFound_ThrowsException`
- `ResendEmailAsync_VerifyType_SendsNewOtp`

### 🔹 AuthService.ChangePasswordAsync
- `ChangePasswordAsync_UserNotFound_ThrowsException`
- `ChangePasswordAsync_GoogleAccount_ThrowsException`
- `ChangePasswordAsync_WrongCurrentPassword_ThrowsException`
- `ChangePasswordAsync_SamePassword_ThrowsException`
- `ChangePasswordAsync_ValidInput_ChangesPassword`

### 🔹 BannerService.GetAllAsync
- `GetAllAsync_ReturnsAllBanners`

### 🔹 BannerService.GetPublicAsync
- `GetPublicAsync_ValidBanners_ReturnsActiveAndInDateRange`

### 🔹 BannerService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsBanner`

### 🔹 BannerService.CreateAsync
- `CreateAsync_InvalidDates_ReturnsNull`
- `CreateAsync_ValidInput_ReturnsCreatedBanner`

### 🔹 BannerService.UpdateAsync
- `UpdateAsync_NotFound_ReturnsNull`
- `UpdateAsync_ValidInput_UpdatesAndReturnsBanner`

### 🔹 BannerService.DeleteAsync
- `DeleteAsync_NotFound_ReturnsFalse`
- `DeleteAsync_Found_MarksInactiveAndReturnsTrue`

### 🔹 BrandService.GetAllAsync
- `GetAllAsync_ReturnsAllBrands`

### 🔹 BrandService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsBrand`

### 🔹 BrandService.GetBySlugAsync
- `GetBySlugAsync_Found_ReturnsBrand`

### 🔹 BrandService.CreateAsync
- `CreateAsync_DuplicateSlug_ReturnsNull`
- `CreateAsync_ValidInput_ReturnsCreatedBrand`

### 🔹 BrandService.UpdateAsync
- `UpdateAsync_NotFound_ReturnsNull`
- `UpdateAsync_DuplicateSlug_ReturnsNull`
- `UpdateAsync_ValidInput_UpdatesAndReturnsBrand`

### 🔹 BrandService.DeleteAsync
- `DeleteAsync_NotFound_ReturnsFalse`
- `DeleteAsync_Found_MarksInactiveAndReturnsTrue`

### 🔹 CategoryService.GetAllAsync
- `GetAllAsync_ReturnsActiveCategories`
- `GetAllAsync_ReturnsAll`

### 🔹 CategoryService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsCategory`

### 🔹 CategoryService.GetBySlugAsync
- `GetBySlugAsync_Found_ReturnsCategory`

### 🔹 CategoryService.CreateAsync
- `CreateAsync_DuplicateSlug_ReturnsNull`
- `CreateAsync_ValidInput_ReturnsCreatedCategory`
- `CreateAsync_ValidInput_ReturnsCreated`

### 🔹 CategoryService.UpdateAsync
- `UpdateAsync_NotFound_ReturnsNull`
- `UpdateAsync_ValidInput_UpdatesAndReturnsCategory`
- `UpdateAsync_ValidInput_UpdatesAndReturns`

### 🔹 CategoryService.DeleteAsync
- `DeleteAsync_NotFound_ReturnsFalse`
- `DeleteAsync_Found_MarksInactiveAndReturnsTrue`
- `DeleteAsync_Found_MarksInactive`

### 🔹 CloudinaryService.UploadImageAsync
- `UploadImageAsync_NullFile_ThrowsException`
- `UploadImageAsync_FileTooLarge_ThrowsException`
- `UploadImageAsync_InvalidExtension_ThrowsException`

### 🔹 CouponService.CreateAsync
- `CreateAsync_EmptyCode_ThrowsBadRequest`
- `CreateAsync_DuplicateCode_ThrowsBadRequest`
- `CreateAsync_InvalidDiscountType_ThrowsBadRequest`
- `CreateAsync_PercentageOver100_ThrowsBadRequest`
- `CreateAsync_EndBeforeStart_ThrowsBadRequest`
- `CreateAsync_ValidInput_CreatesCoupon`

### 🔹 CouponService.GetAllAsync
- `GetAllAsync_ReturnsPaginatedResults`

### 🔹 CouponService.ValidateAsync
- `ValidateAsync_EmptyCode_ReturnsInvalid`
- `ValidateAsync_CouponNotFound_ReturnsInvalid`
- `ValidateAsync_InactiveCoupon_ReturnsInvalid`
- `ValidateAsync_BelowMinAmount_ReturnsInvalid`
- `ValidateAsync_ValidCoupon_ReturnsValid`
- `ValidateAsync_PercentageWithMaxDiscount_CapsDiscount`

### 🔹 CouponService.ApplyAsync
- `ApplyAsync_CouponNotFound_ThrowsNotFound`
- `ApplyAsync_OrderAlreadyHasCoupon_ThrowsBadRequest`

### 🔹 CouponService.DeactivateAsync
- `DeactivateAsync_NotFound_ThrowsNotFound`
- `DeactivateAsync_ActiveCoupon_DeactivatesIt`

### 🔹 CouponService.UpdateAsync
- `UpdateAsync_NotFound_ThrowsNotFound`
- `UpdateAsync_ValidInput_UpdatesCoupon`

### 🔹 DashboardService.GetOverviewAsync
- `GetOverviewAsync_ReturnsCorrectStatistics`

### 🔹 DashboardService.GetRevenueChartAsync
- `GetRevenueChartAsync_ReturnsGroupedRevenue`

### 🔹 DashboardService.GetTopProductsAsync
- `GetTopProductsAsync_ReturnsSortedProducts`

### 🔹 DashboardService.GetTopCustomersAsync
- `GetTopCustomersAsync_ReturnsSortedCustomers`

### 🔹 EmailNotificationService.SendOrderConfirmedEmail
- `SendOrderConfirmedEmail_ValidOrder_SendsEmail`

### 🔹 EmailNotificationService.SendOrderShippingEmail
- `SendOrderShippingEmail_ValidOrder_SendsEmail`

### 🔹 EmailNotificationService.SendOrderDeliveredEmail
- `SendOrderDeliveredEmail_ValidOrder_SendsEmail`

### 🔹 EmailNotificationService.SendReturnProcessedEmail
- `SendReturnProcessedEmail_ApprovedRequest_SendsEmail`

### 🔹 FlashSaleService.CreateAsync
- `CreateAsync_EmptyTitle_ThrowsBadRequest`
- `CreateAsync_EndBeforeStart_ThrowsBadRequest`
- `CreateAsync_OverlappingActive_ThrowsBadRequest`
- `CreateAsync_ValidInput_CreatesFlashSale`

### 🔹 FlashSaleService.GetAllAsync
- `GetAllAsync_ReturnsPaginatedResults`

### 🔹 FlashSaleService.GetActiveAsync
- `GetActiveAsync_NoActive_ReturnsNull`
- `GetActiveAsync_HasActive_ReturnsIt`

### 🔹 FlashSaleService.GetFlashPriceAsync
- `GetFlashPriceAsync_NoActiveFlashSale_ReturnsNull`
- `GetFlashPriceAsync_HasActiveItem_ReturnsPrice`

### 🔹 FlashSaleService.AddItemAsync
- `AddItemAsync_FlashSaleNotFound_ThrowsNotFound`
- `AddItemAsync_ProductNotFound_ThrowsNotFound`
- `AddItemAsync_ZeroStockLimit_ThrowsBadRequest`

### 🔹 FlashSaleService.RemoveItemAsync
- `RemoveItemAsync_NotFound_ThrowsNotFound`
- `RemoveItemAsync_Found_RemovesItem`

### 🔹 InventoryService.CreateReceiptAsync
- `CreateReceiptAsync_Success_ReturnsReceipt`

### 🔹 InventoryService.GetReceiptByIdAsync
- `GetReceiptByIdAsync_Found_ReturnsReceipt`

### 🔹 InventoryService.GetReceiptsAsync
- `GetReceiptsAsync_ReturnsAll`

### 🔹 InventoryService.CompleteReceiptAsync
- `CompleteReceiptAsync_ValidDraft_UpdatesStockAndCreatesTransactions`

### 🔹 InventoryService.CancelReceiptAsync
- `CancelReceiptAsync_CompletedReceipt_RollbacksStock`

### 🔹 InventoryService.AdjustStockAsync
- `AdjustStockAsync_IncrementsStock`

### 🔹 InventoryService.GetTransactionsAsync
- `GetTransactionsAsync_ReturnsProductTransactions`

### 🔹 InventoryService.GetStockStatusAsync
- `GetStockStatusAsync_ReturnsAllProducts`

### 🔹 NewsCategoryService.GetAllAsync
- `GetAllAsync_ReturnsAll`

### 🔹 NewsCategoryService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsCategory`

### 🔹 NewsCategoryService.CreateAsync
- `CreateAsync_ValidInput_ReturnsCreated`

### 🔹 NewsCategoryService.UpdateAsync
- `UpdateAsync_ValidInput_UpdatesAndReturns`

### 🔹 NewsCategoryService.DeleteAsync
- `DeleteAsync_Found_MarksInactive`

### 🔹 NewsService.GetAllAsync
- `GetAllAsync_ReturnsAll`

### 🔹 NewsService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsNews`

### 🔹 NewsService.CreateAsync
- `CreateAsync_ValidInput_ReturnsCreated`

### 🔹 NewsService.UpdateAsync
- `UpdateAsync_ValidInput_UpdatesAndReturns`

### 🔹 NewsService.DeleteAsync
- `DeleteAsync_Found_MarksInactive`

### 🔹 OrderService.CreateAsync
- `CreateAsync_EmptyItems_ThrowsBadRequest`
- `CreateAsync_UserNotFound_ThrowsNotFound`

### 🔹 OrderService.GetAllAsync
- `GetAllAsync_NoFilters_ReturnsAll`

### 🔹 OrderService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsOrderDetail`

### 🔹 OrderService.UpdateAsync
- `UpdateAsync_OrderNotFound_ThrowsNotFound`
- `UpdateAsync_ValidStatusChange_UpdatesOrderAndCreatesHistory`

### 🔹 OrderService.CancelAsync
- `CancelAsync_OrderNotFound_ThrowsNotFound`
- `CancelAsync_AlreadyDelivered_ThrowsBadRequest`
- `CancelAsync_ValidOrder_CancelsAndRestoresStock`

### 🔹 OrderService.GetStatusHistoryAsync
- `GetStatusHistoryAsync_Found_ReturnsHistory`

### 🔹 PaymentService.CreatePaymentAsync
- `CreatePaymentAsync_OrderNotFound_ThrowsNotFound`
- `CreatePaymentAsync_AlreadyPaid_ThrowsBadRequest`
- `CreatePaymentAsync_BankTransfer_ReturnsBankInfo`
- `CreatePaymentAsync_Cod_NoBankInfo`

### 🔹 PaymentService.GetByOrderIdAsync
- `GetByOrderIdAsync_NotFound_ReturnsNull`
- `GetByOrderIdAsync_Found_ReturnsPayment`

### 🔹 PaymentService.ConfirmBankTransferAsync
- `ConfirmBankTransferAsync_NotFound_ThrowsNotFound`
- `ConfirmBankTransferAsync_NotBankTransfer_ThrowsBadRequest`
- `ConfirmBankTransferAsync_AlreadyConfirmed_ThrowsBadRequest`
- `ConfirmBankTransferAsync_Valid_ConfirmsPayment`

### 🔹 PaymentService.CompleteCodPaymentAsync
- `CompleteCodPaymentAsync_NoPayment_DoesNotThrow`
- `CompleteCodPaymentAsync_Valid_CompletesPayment`

### 🔹 ProductImageService.GetByProductIdAsync
- `GetByProductIdAsync_ProductNotFound_ThrowsNotFound`
- `GetByProductIdAsync_ValidProduct_ReturnsImages`

### 🔹 ProductImageService.AddAsync
- `AddAsync_ValidInput_AddsImage`
- `AddAsync_NewPrimary_ResetsOldPrimary`

### 🔹 ProductImageService.DeleteAsync
- `DeleteAsync_ExistingImage_DeletesIt`
- `DeleteAsync_NotFound_ThrowsNotFound`

### 🔹 ProductService.GetAllAsync
- `GetAllAsync_NoFilters_ReturnsAll`

### 🔹 ProductService.GetFullByIdAsync
- `GetFullByIdAsync_ValidId_ReturnsFullProduct`

### 🔹 ProductService.GetFullBySlugAsync
- `GetFullBySlugAsync_ValidSlug_ReturnsFullProduct`

### 🔹 ProductService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsProduct`

### 🔹 ProductService.GetBySlugAsync
- `GetBySlugAsync_ValidSlug_ReturnsProduct`

### 🔹 ProductService.CreateAsync
- `CreateAsync_ValidInput_CreatesProduct`

### 🔹 ProductService.UpdateAsync
- `UpdateAsync_ValidInput_UpdatesProduct`

### 🔹 ProductService.DeleteAsync
- `DeleteAsync_ExistingProduct_MarksAsDeleted`

### 🔹 ProductService.GetProductListAsync
- `GetProductListAsync_NoFilters_ReturnsPublishedProducts`

### 🔹 ProfileService.GetProfileAsync
- `GetProfileAsync_UserExists_ReturnsUserDto`
- `GetProfileAsync_UserNotFound_ReturnsNull`

### 🔹 ProfileService.UpdateProfileAsync
- `UpdateProfileAsync_UserExists_UpdatesAndReturnsDto`
- `UpdateProfileAsync_UserNotFound_ReturnsNull`

### 🔹 ReturnRequestService.GetAllAsync
- `GetAllAsync_ReturnsAll`

### 🔹 ReturnRequestService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsDto`

### 🔹 ReturnRequestService.GetByOrderIdAsync
- `GetByOrderIdAsync_Found_ReturnsDto`

### 🔹 ReturnRequestService.CreateAsync
- `CreateAsync_ValidInput_CreatesRequest`

### 🔹 ReturnRequestService.ProcessAsync
- `ProcessAsync_Approve_UpdatesStatusAndRestoresStock`

### 🔹 ReviewService.GetAllAsync
- `GetAllAsync_ReturnsAll`

### 🔹 ReviewService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsDto`

### 🔹 ReviewService.GetByProductIdAsync
- `GetByProductIdAsync_Found_ReturnsList`

### 🔹 ReviewService.UpdateActiveAsync
- `UpdateActiveAsync_ValidInput_UpdatesAndReturnsDto`

### 🔹 ReviewService.DeleteAsync
- `DeleteAsync_NotFound_ReturnsFalse`
- `DeleteAsync_Found_DeletesAndReturnsTrue`

### 🔹 ReviewService.CreateReplyAsync
- `CreateReplyAsync_ReviewNotFound_ReturnsNull`

### 🔹 ReviewService.UpdateReplyAsync
- `UpdateReplyAsync_ValidInput_UpdatesAndReturnsDto`

### 🔹 ReviewService.DeleteReplyAsync
- `DeleteReplyAsync_NotFound_ReturnsFalse`

### 🔹 ReviewService.AddImageAsync
- `AddImageAsync_ReviewNotFound_ReturnsNull`

### 🔹 ReviewService.DeleteImageAsync
- `DeleteImageAsync_NotFound_ReturnsFalse`

### 🔹 ReviewService.GetImagesByReviewIdAsync
- `GetImagesByReviewIdAsync_ReturnsList`

### 🔹 ReviewService.ToggleVoteAsync
- `ToggleVoteAsync_ReviewNotFound_ReturnsFalse`
- `ToggleVoteAsync_NoExistingVote_AddsVote`
- `ToggleVoteAsync_ExistingVote_RemovesVote`

### 🔹 ReviewService.GetVoteCountAsync
- `GetVoteCountAsync_ReturnsCorrectCount`

### 🔹 ReviewService.HasUserVotedAsync
- `HasUserVotedAsync_HasVoted_ReturnsTrue`
- `HasUserVotedAsync_HasNotVoted_ReturnsFalse`

### 🔹 ReviewService.CreateAsync
- `CreateAsync_InvalidProductId_ThrowsBadRequest`
- `CreateAsync_ProductNotFound_ThrowsNotFound`
- `CreateAsync_UserNotFound_ThrowsNotFound`
- `CreateAsync_ValidInput_CreatesReview`

### 🔹 ShipmentService.CreateAsync
- `CreateAsync_OrderNotFound_ThrowsNotFound`
- `CreateAsync_OrderNotConfirmed_ThrowsBadRequest`
- `CreateAsync_ShipmentAlreadyExists_ThrowsBadRequest`
- `CreateAsync_Valid_CreatesShipmentAndMovesToProcessing`

### 🔹 ShipmentService.UpdateAsync
- `UpdateAsync_NotFound_ThrowsNotFound`

### 🔹 ShipmentService.GetByOrderIdAsync
- `GetByOrderIdAsync_NotFound_ReturnsNull`

### 🔹 ShipmentService.MarkQcPassedAsync
- `MarkQcPassedAsync_NotFound_ThrowsNotFound`
- `MarkQcPassedAsync_Valid_SetsQcPassed`

### 🔹 ShipmentService.MarkPackedAsync
- `MarkPackedAsync_NotFound_ThrowsNotFound`
- `MarkPackedAsync_Valid_SetsPackedStatus`

### 🔹 SupplierService.GetAllAsync
- `GetAllAsync_ReturnsAll`

### 🔹 SupplierService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsSupplier`

### 🔹 SupplierService.CreateAsync
- `CreateAsync_ValidInput_ReturnsCreatedSupplier`

### 🔹 SupplierService.UpdateAsync
- `UpdateAsync_ValidInput_UpdatesSupplier`

### 🔹 SupplierService.DeleteAsync
- `DeleteAsync_Found_MarksInactive`

### 🔹 UserService.GetAllAsync
- `GetAllAsync_ReturnsActiveUsersOnly`

### 🔹 UserService.GetByIdAsync
- `GetByIdAsync_Found_ReturnsUser`

### 🔹 UserService.CreateAsync
- `CreateAsync_ValidInput_CreatesUser`

### 🔹 UserService.UpdateAsync
- `UpdateAsync_ValidInput_UpdatesUser`

### 🔹 UserService.DeleteAsync
- `DeleteAsync_ExistingUser_MarksAsInactive`

### 🔹 WishlistService.GetByUserAsync
- `GetByUserAsync_ReturnsUserWishlistItems`

### 🔹 WishlistService.ToggleAsync
- `ToggleAsync_NewItem_AddsToWishlist`
- `ToggleAsync_ExistingItem_RemovesFromWishlist`

### 🔹 WishlistService.IsInWishlistAsync
- `IsInWishlistAsync_Exists_ReturnsTrue`

### 🔹 WishlistService.CountAsync
- `CountAsync_ReturnsCorrectCount`

