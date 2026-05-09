# 📚 API DOCUMENTATION - FULL SYSTEM (23 MODULES)

Hệ thống cung cấp RESTful API chuẩn JSON. 
- **Base URL**: `http://localhost:5000/api/` (hoặc cấu hình thực tế).
- **Format phản hồi**: Tất cả response đều bọc trong `ApiResponse<T>` hoặc trả về trực tiếp DTO/PagedResult.
- **Xác thực**: Sử dụng Bearer Token trong Header `Authorization`.

---

## 🔐 1. Auth Module (`api/Auth`)
Quản lý đăng ký, đăng nhập và bảo mật tài khoản.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| POST | `login` | Đăng nhập hệ thống | No |
| POST | `register` | Đăng ký tài khoản mới | No |
| POST | `google-login` | Đăng nhập bằng Google ID Token | No |
| POST | `forgot-password` | Yêu cầu OTP khôi phục mật khẩu | No |
| POST | `reset-password` | Đặt lại mật khẩu mới bằng OTP | No |
| POST | `refresh-token` | Lấy Access Token mới từ Refresh Token | No |
| POST | `logout` | Đăng xuất, vô hiệu hóa Refresh Token | No |
| POST | `verify-email` | Xác thực email bằng mã OTP | No |
| POST | `resend-email` | Gửi lại OTP (xác thực email hoặc quên mật khẩu) | No |
| POST | `change-password` | Thay đổi mật khẩu khi đã đăng nhập | Yes |

---

## 👤 2. Users Module (`api/Users`)
Quản lý danh sách người dùng (Dành cho Admin/Staff).

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Lấy danh sách tất cả người dùng | Admin/Staff |
| GET | `{id}` | Lấy thông tin chi tiết một người dùng | Admin/Staff |
| POST | `/` | Admin tạo người dùng mới | Admin/Staff |
| PUT | `{id}` | Cập nhật thông tin người dùng | Admin/Staff |
| DELETE | `{id}` | Xóa (vô hiệu hóa) người dùng | Admin/Staff |

---

## 🆔 3. Profile Module (`api/Profile`)
Quản lý thông tin cá nhân của người dùng hiện tại.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Lấy thông tin profile hiện tại | Yes |
| PUT | `/` | Cập nhật thông tin cá nhân | Yes |

---

## 📦 4. Product Module (`api/Product`)
Quản lý danh mục sản phẩm và tìm kiếm.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách sản phẩm (Admin query) | No |
| GET | `{id}` | Chi tiết sản phẩm cơ bản | No |
| GET | `{id}/full` | Chi tiết sản phẩm + Ảnh + Specs | No |
| GET | `slug/{slug}` | Lấy sản phẩm theo đường dẫn thân thiện | No |
| GET | `slug/{slug}/full` | Lấy sản phẩm full theo slug | No |
| GET | `client` | API cho khách hàng (Filter, Sort, Paging) | No |
| POST | `/` | Tạo sản phẩm mới | Admin/Staff |
| PUT | `{id}` | Cập nhật sản phẩm | Admin/Staff |
| DELETE | `{id}` | Xóa sản phẩm | Admin/Staff |

---

## 📁 5. Categories Module (`api/Categories`)
Quản lý danh mục hàng hóa.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Lấy cây danh mục | No |
| GET | `{id}` | Lấy chi tiết danh mục | No |
| POST | `/` | Tạo danh mục mới | Admin/Staff |
| PUT | `{id}` | Cập nhật danh mục | Admin/Staff |
| DELETE | `{id}` | Xóa danh mục | Admin/Staff |

---

## 🏷️ 6. Brands Module (`api/Brands`)
Quản lý thương hiệu sản phẩm.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách thương hiệu | No |
| GET | `{id}` | Chi tiết thương hiệu | No |
| GET | `slug/{slug}` | Lấy thương hiệu theo slug | No |
| POST | `/` | Tạo thương hiệu mới | Admin/Staff |
| PUT | `{id}` | Cập nhật thương hiệu | Admin/Staff |
| DELETE | `{id}` | Xóa thương hiệu | Admin/Staff |

---

## 🖼️ 7. Product Images Module (`api/products/{productId}/images`)
Quản lý thư viện ảnh của sản phẩm.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Lấy danh sách ảnh của 1 sản phẩm | No |
| POST | `/` | Thêm ảnh mới cho sản phẩm | Admin/Staff |
| DELETE | `{imageId}` | Xóa ảnh sản phẩm | Admin/Staff |

---

## 📤 8. Upload Module (`api/uploads`)
Xử lý tải tệp tin lên Cloudinary.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| POST | `avatar` | Tải lên và cập nhật ảnh đại diện | Yes |
| POST | `{folder}` | Tải lên ảnh vào thư mục (products, news...) | Yes |
| DELETE | `/` | Xóa ảnh khỏi Cloud theo publicId | Admin/Staff |

---

## 🛒 9. Orders Module (`api/Orders`)
Quản lý đơn hàng và quy trình đặt hàng.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Lấy danh sách đơn (Filter theo status, user) | Yes |
| GET | `{id}` | Chi tiết đơn hàng | Yes |
| GET | `{id}/history` | Lịch sử thay đổi trạng thái đơn | Yes |
| POST | `/` | Tạo đơn hàng mới | Yes |
| POST | `{id}/cancel` | Hủy đơn hàng | Yes |
| PUT | `{id}` | Cập nhật thông tin đơn hàng | Admin/Staff |

---

## 💳 10. Payments Module (`api/Payments`)
Xử lý thanh toán đơn hàng.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| POST | `/` | Tạo yêu cầu thanh toán (Bank transfer/COD) | Yes |
| GET | `order/{orderId}` | Lấy thông tin thanh toán của đơn | Yes |
| PATCH | `{paymentId}/confirm` | Xác nhận đã nhận tiền (Chuyển khoản) | Admin/Staff |
| GET | `vnpay-return` | Callback nhận kết quả từ VnPay | No |

---

## 🚚 11. Shipments Module (`api/Shipments`)
Quản lý vận chuyển, QC và Đóng gói.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| POST | `/` | Tạo phiếu giao hàng (status → Packing) | Admin/Staff |
| GET | `order/{orderId}` | Lấy thông tin vận chuyển của đơn | Admin/Staff |
| PUT | `{id}` | Cập nhật tracking, carrier, status | Admin/Staff |
| PATCH | `{id}/qc` | Đánh dấu kết quả kiểm hàng (QC) | Admin/Staff |
| PATCH | `{id}/packed` | Đánh dấu đã đóng gói xong | Admin/Staff |

---

## ↩️ 12. Return Requests Module (`api/ReturnRequests`)
Quản lý quy trình đổi trả hàng.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách yêu cầu đổi trả | Admin/Staff |
| GET | `{id}` | Chi tiết yêu cầu đổi trả | Yes |
| GET | `order/{orderId}` | Lấy yêu cầu đổi trả theo đơn hàng | Yes |
| POST | `/` | Khách hàng gửi yêu cầu đổi trả | Yes |
| PUT | `{id}` | Admin xử lý (Duyệt/Từ chối) yêu cầu | Admin/Staff |

---

## 🏭 13. Inventory Module (`api/Inventory`)
Quản lý nhập kho và biến động tồn kho.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `receipts` | Danh sách phiếu nhập kho | Admin/Staff |
| GET | `receipts/{id}` | Chi tiết phiếu nhập | Admin/Staff |
| POST | `receipts` | Tạo phiếu nhập (Draft) | Admin/Staff |
| PATCH | `receipts/{id}/complete` | Duyệt nhập kho (Tăng tồn thực tế) | Admin/Staff |
| PATCH | `receipts/{id}/cancel` | Hủy phiếu nhập kho | Admin/Staff |
| GET | `transactions/{productId}` | Lịch sử biến động kho của SP | Admin/Staff |
| GET | `stock-status` | Tổng quan tồn kho toàn hệ thống | Admin/Staff |
| POST | `adjust` | Điều chỉnh tồn kho thủ công | Admin/Staff |

---

## 🤝 14. Suppliers Module (`api/Suppliers`)
Quản lý nhà cung cấp hàng hóa.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách nhà cung cấp | Admin/Staff |
| GET | `{id}` | Chi tiết nhà cung cấp | Admin/Staff |
| POST | `/` | Thêm nhà cung cấp mới | Admin/Staff |
| PUT | `{id}` | Cập nhật thông tin nhà cung cấp | Admin/Staff |
| DELETE | `{id}` | Xóa nhà cung cấp | Admin/Staff |

---

## 🎟️ 15. Coupons Module (`api/Coupons`)
Quản lý mã giảm giá.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách Coupon (Admin) | Admin |
| POST | `/` | Tạo Coupon mới | Admin |
| PUT | `{id}` | Cập nhật Coupon | Admin |
| DELETE | `{id}` | Ngừng kích hoạt Coupon | Admin |
| POST | `validate` | Kiểm tra mã giảm giá hợp lệ | No |

---

## ⚡ 16. FlashSales Module (`api/FlashSales`)
Quản lý chương trình khuyến mãi giờ vàng.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách các đợt Flash Sale | Admin |
| GET | `active` | Lấy đợt Flash Sale đang diễn ra | No |
| POST | `/` | Tạo đợt Flash Sale mới | Admin |
| POST | `{id}/items` | Thêm sản phẩm vào Flash Sale | Admin |
| DELETE | `{id}/items/{productId}` | Xóa sản phẩm khỏi Flash Sale | Admin |

---

## 🚩 17. Banners Module (`api/Banners`)
Quản lý ảnh bìa quảng cáo trang chủ.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách Banner (Tất cả) | No |
| GET | `public` | Danh sách Banner đang hiển thị | No |
| GET | `{id}` | Chi tiết Banner | No |
| POST | `/` | Tạo Banner mới | Admin/Staff |
| PUT | `{id}` | Cập nhật Banner | Admin/Staff |
| DELETE | `{id}` | Xóa Banner | Admin/Staff |

---

## 📰 18. News Module (`api/News`)
Quản lý bài viết tin tức công nghệ.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách tin tức | No |
| GET | `{id}` | Chi tiết bài viết | No |
| POST | `/` | Tạo bài viết mới | Admin/Staff |
| PUT | `{id}` | Cập nhật bài viết | Admin/Staff |
| DELETE | `{id}` | Xóa bài viết | Admin/Staff |

---

## 📑 19. News Categories Module (`api/news-categories`)
Quản lý chuyên mục tin tức.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách chuyên mục tin | No |
| GET | `{id}` | Chi tiết chuyên mục | No |
| POST | `/` | Tạo chuyên mục mới | Admin/Staff |
| PUT | `{id}` | Cập nhật chuyên mục | Admin/Staff |
| DELETE | `{id}` | Xóa chuyên mục | Admin/Staff |

---

## ⭐ 20. Reviews Module (`api/Reviews`)
Quản lý đánh giá và bình luận sản phẩm.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Danh sách đánh giá (Admin) | Admin/Staff |
| GET | `product/{productId}` | Lấy đánh giá của một sản phẩm | No |
| GET | `{id}` | Chi tiết 1 đánh giá | No |
| POST | `/` | Tạo đánh giá mới | Yes |
| PATCH | `{id}/active` | Duyệt/Ẩn đánh giá | Admin/Staff |
| DELETE | `{id}` | Xóa đánh giá | Admin/Staff |
| POST | `{reviewId}/replies` | Phản hồi đánh giá | Admin/Staff |
| PUT | `replies/{replyId}` | Cập nhật phản hồi | Admin/Staff |
| DELETE | `replies/{replyId}` | Xóa phản hồi | Admin/Staff |
| GET | `{reviewId}/images` | Lấy ảnh của đánh giá | No |
| POST | `{reviewId}/images` | Thêm ảnh vào đánh giá | Yes |
| DELETE | `images/{imageId}` | Xóa ảnh đánh giá | Yes |
| POST | `{reviewId}/votes/toggle` | Bình chọn "Hữu ích" | Yes |
| GET | `{reviewId}/votes/count` | Lấy số lượng vote hữu ích | No |
| GET | `{reviewId}/votes/check/{userId}` | Kiểm tra người dùng đã vote chưa | Yes |

---

## ❤️ 21. Wishlist Module (`api/Wishlist`)
Quản lý danh sách sản phẩm yêu thích của người dùng.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Lấy danh sách yêu thích của tôi | Yes |
| POST | `toggle/{productId}` | Thêm/Xóa sản phẩm khỏi yêu thích | Yes |
| GET | `check/{productId}` | Kiểm tra SP có trong yêu thích không | Yes |

---

## 📊 22. Dashboard Module (`api/Dashboard`)
Thống kê báo cáo cho quản trị viên.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `overview` | Số liệu tổng quan (Doanh thu, Đơn hàng, User) | Admin |
| GET | `revenue` | Dữ liệu biểu đồ doanh thu theo ngày | Admin |
| GET | `top-products` | Top sản phẩm bán chạy nhất | Admin |
| GET | `top-customers` | Top khách hàng chi tiêu nhiều nhất | Admin |

---

## 📜 23. Activity Logs Module (`api/ActivityLogs`)
Nhật ký hệ thống và hoạt động người dùng.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/` | Xem toàn bộ nhật ký (Filter: user, date, entity) | Admin |
