// Controller = "Cửa ngõ" nhận HTTP request từ frontend
// Mỗi method tương ứng 1 API endpoint
//
// [Route("api/[controller]")] → URL sẽ là: /api/users
// Vì tên class là UsersController, ASP.NET tự bỏ hậu tố "Controller"

using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]                    // ← Đánh dấu đây là API Controller
[Route("api/[controller]")]        // ← Route: /api/users
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;  // ← Inject UserService
    }

    // GET /api/users → Lấy tất cả users
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _service.GetAllAsync(cancellationToken);
        return Ok(users);  // ← Trả về HTTP 200 + JSON object
    }

    // GET /api/users/5 → Lấy user có id = 5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _service.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });  // ← HTTP 404

        return Ok(user);  // ← HTTP 200
    }

    // POST /api/users → Tạo mới user
    [HttpPost]
    public async Task<ActionResult<UserDto?>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ";

            return BadRequest(new { message = firstError });
        }

        var user = await _service.CreateAsync(dto, cancellationToken);
        if (user == null)
            return Conflict(new { message = "Email này đã được sử dụng bởi một tài khoản khác" });

        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
    }

    // PUT /api/users/5 → Cập nhật user có id = 5
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _service.UpdateAsync(id, dto, cancellationToken);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        return Ok(user);
    }

    // DELETE /api/users/5 → Xóa user có id = 5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        return NoContent();  // ← HTTP 204 (xóa thành công, không trả data)
    }
}
