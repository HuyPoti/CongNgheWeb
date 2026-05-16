using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend.Models;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReturnRequestsController : ControllerBase
{
    private readonly IReturnRequestService _service;
    private readonly IEmailNotificationService _emailNotification;

    public ReturnRequestsController(IReturnRequestService service, IEmailNotificationService emailNotification)
    {
        _service = service;
        _emailNotification = emailNotification;
    }

    private Guid GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(idStr!);
    }



    [HttpGet]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<List<ReturnRequestDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReturnRequestDto>>> GetById(Guid id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound(ApiResponse.Fail("Return request not found"));

        // Security: Khách hàng chỉ xem được yêu cầu của chính mình
        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff");
        if (!isAdminOrStaff && request.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return Ok(ApiResponse.Ok(request));
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ApiResponse<ReturnRequestDto>>> GetByOrderId(Guid orderId)
    {
        var request = await _service.GetByOrderIdAsync(orderId);
        if (request == null) return NotFound(ApiResponse.Fail("Return request not found"));

        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff");
        if (!isAdminOrStaff && request.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return Ok(ApiResponse.Ok(request));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReturnRequestDto>>> Create([FromBody] CreateReturnRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _service.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.ReturnId }, ApiResponse.Ok(result));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ReturnRequestDto>>> Process(Guid id, [FromBody] UpdateReturnRequestDto dto)
    {
        var adminId = GetCurrentUserId();
        var result = await _service.ProcessAsync(adminId, id, dto);
        
        // Gửi thông báo email sau khi xử lý
        await _emailNotification.SendReturnProcessedEmail(id);

        return Ok(ApiResponse.Ok(result));
    }
}
