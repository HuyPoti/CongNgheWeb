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
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        // Security: Khách hàng chỉ xem được yêu cầu của chính mình
        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff");
        if (!isAdminOrStaff && request.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return Ok(request);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        var request = await _service.GetByOrderIdAsync(orderId);
        if (request == null) return NotFound();

        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff");
        if (!isAdminOrStaff && request.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ReturnId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Process(Guid id, [FromBody] UpdateReturnRequestDto dto)
    {

        try
        {
            var adminId = GetCurrentUserId();
            var result = await _service.ProcessAsync(adminId, id, dto);
            
            // Gửi thông báo email sau khi xử lý
            await _emailNotification.SendReturnProcessedEmail(id);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
