# 🧪 HƯỚNG DẪN VIẾT UNIT TEST (BACKEND)

Tài liệu này cung cấp quy chuẩn và hướng dẫn cách viết Unit Test cho Backend (.NET) trong dự án.

---

## 1. Nguyên tắc chung (General Principles)

- **Độc lập (Independent)**: Các test case không phụ thuộc vào nhau. Mỗi test nên tự chuẩn bị và dọn dẹp dữ liệu của chính nó.
- **AAA Pattern**: 
  - **Arrange**: Chuẩn bị dữ liệu mẫu, thiết lập mock cho các phụ thuộc (Dependencies).
  - **Act**: Thực hiện gọi phương thức hoặc hành động cần kiểm thử.
  - **Assert**: Kiểm tra kết quả trả về, trạng thái đối tượng hoặc hành vi mong đợi.
- **Naming Convention**: `[MethodName]_[Scenario]_[ExpectedResult]`
  - Ví dụ: `LoginAsync_InvalidCredentials_ReturnsErrorResponse`
- **Tốc độ**: Unit test phải chạy cực nhanh, tuyệt đối không gọi đến Database thật, File System hay External API. Sử dụng Mocking để giả lập các thành phần này.

---

## 2. Công cụ sử dụng

- **Framework**: `xUnit` (Phổ biến, linh hoạt, hỗ trợ chạy song song).
- **Mocking**: `Moq` (Dùng để giả lập Repository, UnitOfWork, Service khác).
- **Assertions**: `FluentAssertions` (Giúp code assert dễ đọc, gần gũi với ngôn ngữ tự nhiên).

---

## 3. Cấu trúc dự án Test

Khuyến nghị tạo một dự án riêng biệt để chứa các bản kiểm thử:
- Tên dự án: `backend.Tests`
- Thư mục tương ứng: `backend.Tests/Services`, `backend.Tests/Controllers`, etc.

---

## 4. Ví dụ Test cho Service

Giả sử chúng ta kiểm thử logic đăng nhập trong `AuthService`:

```csharp
public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        // Khởi tạo service với các đối tượng mock
        _service = new AuthService(_mockUow.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsError()
    {
        // 1. Arrange (Chuẩn bị)
        var loginDto = new LoginDto { Email = "notfound@example.com", Password = "any" };
        
        // Giả lập Repository trả về null khi tìm user
        _mockUow.Setup(u => u.UserRepository.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync((User)null);

        // 2. Act (Thực hiện)
        var result = await _service.LoginAsync(loginDto);

        // 3. Assert (Kiểm tra)
        result.Status.Should().Be("error");
        result.Message.Should().Be("User not found");
        
        // Kiểm tra xem phương thức GetByEmailAsync có thực sự được gọi không
        _mockUow.Verify(u => u.UserRepository.GetByEmailAsync(loginDto.Email), Times.Once);
    }
}
```

---

## 5. Quy trình chạy Test

Sử dụng terminal trong thư mục `backend`:

```bash
# Chạy tất cả các test
dotnet test

# Chạy test và hiển thị chi tiết
dotnet test --logger "console;verbosity=detailed"
```

---

## 6. Danh mục các thành phần cần viết Test

| Thành phần | Mức độ ưu tiên | Mục tiêu kiểm thử |
| :--- | :--- | :--- |
| **Services** | Cao nhất | Logic nghiệp vụ, tính toán, xử lý dữ liệu phức tạp. |
| **Validators** | Cao | Các quy tắc kiểm tra đầu vào (FluentValidation). |
| **Mapper Profiles** | Trung bình | Đảm bảo chuyển đổi Model <-> DTO chính xác. |
| **Utils/Helpers** | Trung bình | Các hàm tiện ích dùng chung (Format, Token generation). |
| **Controllers** | Thấp | Chỉ test nếu có logic xử lý Status Code hoặc phân quyền đặc biệt. |
