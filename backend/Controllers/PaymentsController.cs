using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// Tạo payment mới - trả về bank info nếu là bank_transfer
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreatePaymentResponseDto>>> CreatePayment(
        [FromBody] CreatePaymentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreatePaymentAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    /// Lấy payment theo order ID
    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null)
            return NotFound(ApiResponse.Fail("Payment not found"));
        return Ok(ApiResponse.Ok(payment));
    }

    /// Xác nhận thanh toán chuyển khoản (staff/admin only)
    [HttpPatch("{paymentId}/confirm")]
    [Authorize(Roles = "staff,admin")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ConfirmBankTransfer(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid user");
        }

        var payment = await _paymentService.ConfirmBankTransferAsync(paymentId, userId, cancellationToken);
        return Ok(ApiResponse.Ok(payment));
    }

    /// Xử lý callback từ VNPay (sẽ implement sau)
    [HttpGet("vnpay-return")]
    public async Task<ActionResult<ApiResponse<VnPayResultDto>>> VnPayReturn(
        [FromQuery] VnPayCallbackDto callback,
        CancellationToken cancellationToken)
    {
        return BadRequest(ApiResponse.Fail("VNPay integration not yet implemented"));
    }
}
