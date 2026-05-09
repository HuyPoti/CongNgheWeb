# 🧪 MASTER SAMPLE DATA - 23 MODULES (COPY-FRIENDLY)

---

## ⚙️ THIẾT LẬP POSTMAN TRƯỚC KHI TEST

### Bước 1: Tạo Collection Variables

Trong Postman → tạo Collection `TechShop API` → tab **Variables** → thêm 3 biến:

| Variable | Initial Value | Mô tả |
|---|---|---|
| `host` | `http://localhost:5000/api` | Base URL backend |
| `token` | _(để trống)_ | Access Token sau khi login |
| `refreshToken` | _(để trống)_ | Refresh Token sau khi login |

### Bước 2: Tự động lưu Token sau khi Login

Trong request **`POST {{host}}/Auth/login`** → tab **Tests** → dán script:

```javascript
const res = pm.response.json();
pm.collectionVariables.set("token", res.token);
pm.collectionVariables.set("refreshToken", res.refreshToken);
console.log("✅ Token saved:", res.token);
```

> Sau khi gửi request login thành công, token được **tự động lưu** — không cần paste tay.

### Bước 3: Gắn Token vào mọi request có Auth

Với mỗi request yêu cầu Auth → tab **Authorization**:
- **Type**: `Bearer Token`  
- **Token**: `{{token}}`

Hoặc thêm Header thủ công:

```
Authorization: Bearer {{token}}
Content-Type: application/json
```

### Bước 4: Thứ tự test cơ bản (luồng khởi động)

```
1. POST Auth/register     → Tạo tài khoản
2. POST Auth/verify-email → Xác nhận OTP
3. POST Auth/login        → Lấy token (script tự lưu)
4. POST Categories        → Tạo danh mục
5. POST Brands            → Tạo thương hiệu
6. POST Suppliers         → Tạo nhà cung cấp
7. POST Product           → Tạo sản phẩm
8. POST Inventory/receipts          → Tạo phiếu nhập
9. PATCH Inventory/receipts/{id}/complete → Duyệt nhập kho
10. POST Orders           → Đặt hàng
```

---

> **Header mặc định cho mọi request:**  
> `Authorization: Bearer {{token}}`  
> `Content-Type: application/json`

---

## 🔐 MODULE 1: Auth (`api/Auth`)

### POST `api/Auth/register`
```json
{
  "email": "customer@example.com",
  "password": "Password123!",
  "fullName": "Nguyễn Văn A",
  "phone": "0987654321"
}
```

### POST `api/Auth/login`
```json
{
  "email": "admin@gearvn.com",
  "password": "AdminPassword123!"
}
```

### POST `api/Auth/google-login`
```json
{
  "idToken": "GOOGLE_ID_TOKEN_TU_FRONTEND"
}
```

### POST `api/Auth/forgot-password`
```json
{
  "email": "customer@example.com"
}
```

### POST `api/Auth/reset-password`
```json
{
  "email": "customer@example.com",
  "otp": "123456",
  "newPassword": "NewPassword@2024"
}
```

### POST `api/Auth/refresh-token`
```json
{
  "refreshToken": "REFRESH_TOKEN_TU_LOGIN"
}
```

### POST `api/Auth/logout`
```json
{
  "refreshToken": "REFRESH_TOKEN_TU_LOGIN"
}
```

### POST `api/Auth/verify-email`
```json
{
  "email": "customer@example.com",
  "otp": "123456"
}
```

### POST `api/Auth/resend-email`
**Trường hợp 1 - Gửi lại mã xác thực email:**
```json
{
  "email": "customer@example.com",
  "type": "verify"
}
```
**Trường hợp 2 - Gửi lại mã đặt lại mật khẩu:**
```json
{
  "email": "customer@example.com",
  "type": "forgot-password"
}
```

### POST `api/Auth/change-password` _(Auth required)_
```json
{
  "currentPassword": "Password123!",
  "newPassword": "NewPassword@2024"
}
```

---

## 👤 MODULE 2: Users (`api/Users`) _(Admin/Staff)_

### GET `api/Users`
> Không cần body. Trả về danh sách tất cả người dùng.

### GET `api/Users/{id}`
> Không cần body. Thay `{id}` bằng UUID user thực tế.

### POST `api/Users`
```json
{
  "email": "staff@gearvn.com",
  "password": "StaffPass123!",
  "fullName": "Nhân viên B",
  "phone": "0911222333",
  "roleId": 2
}
```

### PUT `api/Users/{id}`
```json
{
  "fullName": "Nhân viên B (Updated)",
  "phone": "0922333444",
  "roleId": 2,
  "status": 1
}
```

### DELETE `api/Users/{id}`
> Không cần body. Thay `{id}` bằng UUID user cần xóa.

---

## 🆔 MODULE 3: Profile (`api/Profile`) _(Auth required)_

### GET `api/Profile`
> Không cần body. Trả về profile của user đang đăng nhập.

### PUT `api/Profile`
```json
{
  "fullName": "Nguyễn Văn A (Updated)",
  "phone": "0988888888",
  "avatarUrl": "https://res.cloudinary.com/demo/image/upload/avatars/user_1.jpg"
}
```

---

## 📦 MODULE 4: Product (`api/Product`)

### GET `api/Product`
> Query params: `?keyword=laptop&categoryId=UUID&minPrice=10000000&maxPrice=50000000&page=1&pageSize=10`

### GET `api/Product/{id}`
> Không cần body.

### GET `api/Product/{id}/full`
> Không cần body. Trả về product + ảnh + specs.

### GET `api/Product/slug/{slug}`
> Ví dụ: `api/Product/slug/asus-rog-strix-g16-2024`

### GET `api/Product/slug/{slug}/full`
> Ví dụ: `api/Product/slug/asus-rog-strix-g16-2024/full`

### GET `api/Product/client`
> Query: `?categorySlug=laptop-gaming&keyword=asus&brandId=UUID&minPrice=20000000&sortBy=price_asc&page=1&pageSize=12`

### POST `api/Product` _(Admin/Staff)_
```json
{
  "categoryId": "UUID_CATEGORY",
  "brandId": "UUID_BRAND",
  "name": "ASUS ROG Strix G16 (2024)",
  "slug": "asus-rog-strix-g16-2024",
  "sku": "ROG-G16-2024-001",
  "regularPrice": 35000000,
  "salePrice": 32990000,
  "stockQuantity": 50,
  "warrantyMonths": 24,
  "specifications": "{\"CPU\": \"Core i9-13980HX\", \"RAM\": \"16GB DDR5\", \"VGA\": \"RTX 4060 8GB\", \"Screen\": \"16 inch 165Hz\"}",
  "status": 2
}
```

### PUT `api/Product/{id}` _(Admin/Staff)_
```json
{
  "name": "ASUS ROG Strix G16 (2024) - Updated",
  "salePrice": 31990000,
  "specifications": "{\"CPU\": \"Core i9-13980HX\", \"RAM\": \"32GB DDR5\"}",
  "status": 2
}
```

### DELETE `api/Product/{id}` _(Admin/Staff)_
> Không cần body.

---

## 📁 MODULE 5: Categories (`api/Categories`)

### GET `api/Categories`
> Không cần body.

### GET `api/Categories/{id}`
> Không cần body.

### POST `api/Categories` _(Admin/Staff)_
```json
{
  "name": "Laptop Gaming",
  "slug": "laptop-gaming",
  "parentId": null,
  "icon": "laptop-icon",
  "sortOrder": 1
}
```

### PUT `api/Categories/{id}` _(Admin/Staff)_
```json
{
  "name": "Laptop Gaming (Updated)",
  "slug": "laptop-gaming-updated",
  "sortOrder": 2
}
```

### DELETE `api/Categories/{id}` _(Admin/Staff)_
> Không cần body.

---

## 🏷️ MODULE 6: Brands (`api/Brands`)

### GET `api/Brands`
> Không cần body.

### GET `api/Brands/{id}`
> Không cần body.

### GET `api/Brands/slug/{slug}`
> Ví dụ: `api/Brands/slug/asus`

### POST `api/Brands` _(Admin/Staff)_
```json
{
  "name": "ASUS",
  "slug": "asus",
  "logoUrl": "https://res.cloudinary.com/demo/image/upload/brands/asus.png",
  "description": "Thương hiệu gaming hàng đầu thế giới"
}
```

### PUT `api/Brands/{id}` _(Admin/Staff)_
```json
{
  "name": "ASUS Republic of Gamers",
  "description": "Thương hiệu gaming cao cấp"
}
```

### DELETE `api/Brands/{id}` _(Admin/Staff)_
> Không cần body.

---

## 🖼️ MODULE 7: Product Images (`api/products/{productId}/images`)

### GET `api/products/{productId}/images`
> Không cần body.

### POST `api/products/{productId}/images` _(Admin/Staff)_
```json
{
  "imageUrl": "https://res.cloudinary.com/demo/image/upload/products/rog-g16-front.jpg",
  "isMain": true,
  "sortOrder": 0
}
```

### DELETE `api/products/{productId}/images/{imageId}` _(Admin/Staff)_
> Không cần body.

---

## 📤 MODULE 8: Upload (`api/uploads`)

### POST `api/uploads/avatar` _(Auth required)_
> **Content-Type**: `multipart/form-data`
> **Form field**: `file` = [chọn file ảnh]

### POST `api/uploads/{folder}` _(Auth required)_
> **folder**: `products` | `banners` | `news` | `reviews` | `returns`
> **Content-Type**: `multipart/form-data`
> **Form field**: `file` = [chọn file ảnh]

### DELETE `api/uploads?publicId={publicId}` _(Admin/Staff)_
> Query: `?publicId=products/rog-g16-front`

---

## 🛒 MODULE 9: Orders (`api/Orders`)

### GET `api/Orders`
> Query: `?status=pending&userId=UUID&page=1&pageSize=10`

### GET `api/Orders/{id}`
> Không cần body.

### GET `api/Orders/{id}/history`
> Không cần body.

### POST `api/Orders` _(Auth required)_
```json
{
  "shippingAddressId": "UUID_ADDRESS",
  "paymentMethod": "COD",
  "couponCode": "GIAM100K",
  "note": "Giao hàng sau 17h",
  "items": [
    { "productId": "UUID_SP_1", "quantity": 1 },
    { "productId": "UUID_SP_2", "quantity": 2 }
  ]
}
```

### POST `api/Orders/{id}/cancel` _(Auth required)_
```json
{
  "reason": "Tôi tìm được nơi khác rẻ hơn"
}
```

### PUT `api/Orders/{id}` _(Admin/Staff)_
```json
{
  "status": 3,
  "note": "Đơn đang được đóng gói"
}
```

---

## 💳 MODULE 10: Payments (`api/Payments`)

### POST `api/Payments`
```json
{
  "orderId": "UUID_ORDER",
  "paymentMethod": "bank_transfer"
}
```

### GET `api/Payments/order/{orderId}`
> Không cần body.

### PATCH `api/Payments/{paymentId}/confirm` _(Admin/Staff)_
> Không cần body. Xác nhận đã nhận tiền chuyển khoản.

### GET `api/Payments/vnpay-return`
> Query params do VNPay callback tự gửi về.

---

## 🚚 MODULE 11: Shipments (`api/Shipments`) _(Admin/Staff)_

### POST `api/Shipments`
```json
{
  "orderId": "UUID_ORDER",
  "carrier": "Giao Hàng Nhanh (GHN)",
  "shippingFee": 25000
}
```

### GET `api/Shipments/order/{orderId}`
> Không cần body.

### PUT `api/Shipments/{id}`
```json
{
  "trackingCode": "GHN-001234567",
  "carrier": "GHN",
  "status": "shipping",
  "estimatedDelivery": "2024-06-05T00:00:00Z"
}
```

### PATCH `api/Shipments/{id}/qc`
```json
{
  "isPassed": true,
  "qcNote": "Sản phẩm nguyên seal, đủ phụ kiện, ngoại quan tốt."
}
```

### PATCH `api/Shipments/{id}/packed`
> Không cần body. Ghi nhận người đóng gói từ JWT.

---

## ↩️ MODULE 12: Return Requests (`api/ReturnRequests`)

### GET `api/ReturnRequests` _(Admin/Staff)_
> Không cần body.

### GET `api/ReturnRequests/{id}`
> Không cần body.

### GET `api/ReturnRequests/order/{orderId}`
> Không cần body.

### POST `api/ReturnRequests` _(Auth required)_
```json
{
  "orderId": "UUID_ORDER",
  "reason": "Sản phẩm bị lỗi kỹ thuật ngay khi mở hộp",
  "items": [
    { "productId": "UUID_SP", "quantity": 1 }
  ],
  "images": [
    "https://res.cloudinary.com/demo/image/upload/returns/loi_1.jpg"
  ]
}
```

### PUT `api/ReturnRequests/{id}` _(Admin/Staff)_
```json
{
  "status": "approved",
  "adminNote": "Đã xác nhận lỗi sản phẩm, sẽ hoàn kho và hoàn tiền trong 3-5 ngày."
}
```

---

## 🏭 MODULE 13: Inventory (`api/Inventory`) _(Admin/Staff)_

### GET `api/Inventory/receipts`
> Không cần body.

### GET `api/Inventory/receipts/{id}`
> Không cần body.

### POST `api/Inventory/receipts`
```json
{
  "supplierId": "UUID_SUPPLIER",
  "note": "Nhập hàng đợt tháng 6/2024",
  "items": [
    { "productId": "UUID_SP_1", "quantity": 100, "unitPrice": 28000000 },
    { "productId": "UUID_SP_2", "quantity": 50, "unitPrice": 5500000 }
  ]
}
```

### PATCH `api/Inventory/receipts/{id}/complete`
> Không cần body. Duyệt phiếu → tăng tồn kho thực tế.

### PATCH `api/Inventory/receipts/{id}/cancel`
```json
{
  "reason": "Hàng không đúng chất lượng yêu cầu"
}
```

### GET `api/Inventory/transactions/{productId}`
> Không cần body.

### GET `api/Inventory/stock-status`
> Không cần body.

### POST `api/Inventory/adjust`
```json
{
  "productId": "UUID_SP",
  "adjustment": -3,
  "reason": "Hàng bị vỡ trong quá trình kiểm kho"
}
```

---

## 🤝 MODULE 14: Suppliers (`api/Suppliers`) _(Admin/Staff)_

### GET `api/Suppliers`
> Không cần body.

### GET `api/Suppliers/{id}`
> Không cần body.

### POST `api/Suppliers`
```json
{
  "name": "Công ty TNHH Phân Phối GearPro",
  "contactName": "Nguyễn Minh Tân",
  "email": "contact@gearpro.vn",
  "phone": "02838123456",
  "address": "123 Lê Lợi, Q1, TP.HCM",
  "taxCode": "0312345678"
}
```

### PUT `api/Suppliers/{id}`
```json
{
  "name": "Công ty TNHH GearPro (Updated)",
  "phone": "02838999999",
  "address": "456 Nguyễn Huệ, Q1, TP.HCM"
}
```

### DELETE `api/Suppliers/{id}`
> Không cần body.

---

## 🎟️ MODULE 15: Coupons (`api/Coupons`) _(Admin)_

### GET `api/Coupons`
> Query: `?page=1&pageSize=10&isActive=true&keyword=HE`

### POST `api/Coupons`
```json
{
  "code": "HE2024",
  "discountType": "percentage",
  "discountValue": 10,
  "maxDiscountAmount": 500000,
  "minOrderAmount": 2000000,
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-08-31T23:59:59Z",
  "usageLimit": 100,
  "isActive": true
}
```

### PUT `api/Coupons/{id}`
```json
{
  "discountValue": 15,
  "endDate": "2024-09-30T23:59:59Z",
  "usageLimit": 200
}
```

### DELETE `api/Coupons/{id}`
> Không cần body. Chuyển coupon sang trạng thái không kích hoạt.

### POST `api/Coupons/validate` _(No auth)_
```json
{
  "code": "HE2024",
  "totalAmount": 3000000,
  "userId": "UUID_USER"
}
```

---

## ⚡ MODULE 16: Flash Sales (`api/FlashSales`)

### GET `api/FlashSales` _(Admin)_
> Query: `?page=1&pageSize=10`

### GET `api/FlashSales/active` _(Public)_
> Không cần body.

### POST `api/FlashSales` _(Admin)_
```json
{
  "name": "Siêu Sale 6.6 - Đại Tiệc Gaming",
  "startDate": "2024-06-06T00:00:00Z",
  "endDate": "2024-06-06T23:59:59Z",
  "isActive": true
}
```

### POST `api/FlashSales/{id}/items` _(Admin)_
```json
{
  "productId": "UUID_SP",
  "flashPrice": 28000000,
  "quantity": 20
}
```

### DELETE `api/FlashSales/{id}/items/{productId}` _(Admin)_
> Không cần body.

---

## 🚩 MODULE 17: Banners (`api/Banners`)

### GET `api/Banners`
> Không cần body.

### GET `api/Banners/public`
> Không cần body. Chỉ trả banner đang active.

### GET `api/Banners/{id}`
> Không cần body.

### POST `api/Banners` _(Admin/Staff)_
```json
{
  "title": "Sale Hè 2024 - Giảm đến 30%",
  "imageUrl": "https://res.cloudinary.com/demo/image/upload/banners/sale_he_2024.jpg",
  "linkUrl": "/flash-sales",
  "isActive": true,
  "sortOrder": 1,
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-08-31T23:59:59Z"
}
```

### PUT `api/Banners/{id}` _(Admin/Staff)_
```json
{
  "title": "Sale Hè 2024 - Cập nhật",
  "isActive": false,
  "sortOrder": 2
}
```

### DELETE `api/Banners/{id}` _(Admin/Staff)_
> Không cần body.

---

## 📰 MODULE 18: News (`api/News`)

### GET `api/News`
> Không cần body.

### GET `api/News/{id}`
> Không cần body.

### POST `api/News` _(Admin/Staff)_
```json
{
  "categoryId": "UUID_NEWS_CATEGORY",
  "title": "Đánh giá RTX 5090: Hiệu năng vượt trội",
  "slug": "danh-gia-rtx-5090-hieu-nang-vuot-troi",
  "summary": "RTX 5090 mang lại hiệu năng vượt bậc với kiến trúc Blackwell...",
  "content": "<h2>Nội dung bài viết chi tiết</h2><p>Lorem ipsum...</p>",
  "thumbnailUrl": "https://res.cloudinary.com/demo/image/upload/news/rtx5090.jpg",
  "status": 1
}
```

### PUT `api/News/{id}` _(Admin/Staff)_
```json
{
  "title": "Đánh giá RTX 5090 (Cập nhật)",
  "status": 2
}
```

### DELETE `api/News/{id}` _(Admin/Staff)_
> Không cần body.

---

## 📑 MODULE 19: News Categories (`api/news-categories`)

### GET `api/news-categories`
> Không cần body.

### GET `api/news-categories/{id}`
> Không cần body.

### POST `api/news-categories` _(Admin/Staff)_
```json
{
  "name": "Đánh giá - Review",
  "slug": "danh-gia-review",
  "description": "Các bài đánh giá sản phẩm công nghệ"
}
```

### PUT `api/news-categories/{id}` _(Admin/Staff)_
```json
{
  "name": "Đánh giá & Review (Updated)",
  "description": "Bài đánh giá chuyên sâu"
}
```

### DELETE `api/news-categories/{id}` _(Admin/Staff)_
> Không cần body.

---

## ⭐ MODULE 20: Reviews (`api/Reviews`)

### GET `api/Reviews` _(Admin/Staff)_
> Không cần body.

### GET `api/Reviews/product/{productId}` _(Public)_
> Không cần body.

### GET `api/Reviews/{id}` _(Public)_
> Không cần body.

### POST `api/Reviews` _(Auth required)_
```json
{
  "productId": "UUID_SP",
  "rating": 5,
  "comment": "Sản phẩm xuất sắc, đóng gói cẩn thận, giao hàng siêu nhanh!",
  "images": [
    "https://res.cloudinary.com/demo/image/upload/reviews/review_1.jpg"
  ]
}
```

### PATCH `api/Reviews/{id}/active` _(Admin/Staff)_
```json
{
  "isActive": true
}
```

### DELETE `api/Reviews/{id}` _(Admin/Staff)_
> Không cần body.

### POST `api/Reviews/{reviewId}/replies` _(Admin/Staff)_
```json
{
  "content": "Cảm ơn bạn đã tin tưởng lựa chọn GearVN! Chúc bạn trải nghiệm vui vẻ."
}
```

### PUT `api/Reviews/replies/{replyId}` _(Admin/Staff)_
```json
{
  "content": "Cảm ơn quý khách đã phản hồi. Chúng tôi sẽ cải thiện dịch vụ!"
}
```

### DELETE `api/Reviews/replies/{replyId}` _(Admin/Staff)_
> Không cần body.

### GET `api/Reviews/{reviewId}/images` _(Public)_
> Không cần body.

### POST `api/Reviews/{reviewId}/images` _(Auth required)_
```json
{
  "imageUrl": "https://res.cloudinary.com/demo/image/upload/reviews/review_img_2.jpg"
}
```

### DELETE `api/Reviews/images/{imageId}` _(Auth required)_
> Không cần body.

### POST `api/Reviews/{reviewId}/votes/toggle` _(Auth required)_
```json
{
  "userId": "UUID_USER"
}
```

### GET `api/Reviews/{reviewId}/votes/count`
> Không cần body. Trả về số lượng vote hữu ích của đánh giá.

### GET `api/Reviews/{reviewId}/votes/check/{userId}`
> Không cần body. Kiểm tra user hiện tại đã vote cho đánh giá này chưa.


---

## ❤️ MODULE 21: Wishlist (`api/Wishlist`) _(Auth required)_

### GET `api/Wishlist`
> Không cần body.

### POST `api/Wishlist/toggle/{productId}`
> Không cần body. Toggle thêm/xóa sản phẩm.

### GET `api/Wishlist/check/{productId}`
> Không cần body. Trả về `{ "isInWishlist": true/false }`.

---

## 📊 MODULE 22: Dashboard (`api/Dashboard`) _(Admin)_

### GET `api/Dashboard/overview`
> Không cần body.

### GET `api/Dashboard/revenue`
> Query: `?days=30`

### GET `api/Dashboard/top-products`
> Query: `?take=10`

### GET `api/Dashboard/top-customers`
> Query: `?take=10`

---

## 📜 MODULE 23: Activity Logs (`api/ActivityLogs`) _(Admin)_

### GET `api/ActivityLogs`
> Query: `?page=1&pageSize=20&userId=UUID&entityType=Order&from=2024-01-01&to=2024-12-31`

---

## 💡 HƯỚNG DẪN KIỂM THỬ THEO LUỒNG NGHIỆP VỤ

### 🚀 Luồng khởi động hệ thống (Bootstrap)
1. `POST auth/register` → Tạo tài khoản
2. `POST auth/verify-email` → Xác nhận email
3. `POST auth/login` → Lấy Access Token
4. `POST api/Categories` → Tạo danh mục
5. `POST api/Brands` → Tạo thương hiệu
6. `POST api/Suppliers` → Tạo nhà cung cấp
7. `POST api/Product` → Tạo sản phẩm
8. `POST api/Inventory/receipts` → Tạo phiếu nhập kho
9. `PATCH api/Inventory/receipts/{id}/complete` → Duyệt nhập kho

### 🛒 Luồng đặt hàng
1. `POST api/Orders` → Tạo đơn hàng
2. `POST api/Payments` → Tạo thanh toán
3. `POST api/Shipments` → Tạo phiếu vận chuyển
4. `PATCH api/Shipments/{id}/qc` → QC hàng
5. `PATCH api/Shipments/{id}/packed` → Đóng gói
6. `PUT api/Shipments/{id}` → Cập nhật tracking code

### 📝 Luồng sau bán hàng
1. `POST api/Reviews` → Tạo đánh giá
2. `POST api/Reviews/{id}/replies` → Staff phản hồi
3. `POST api/ReturnRequests` → Khách gửi đổi trả
4. `PUT api/ReturnRequests/{id}` → Admin xử lý đổi trả
