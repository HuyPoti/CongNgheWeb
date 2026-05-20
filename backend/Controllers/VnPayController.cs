using backend.DTOs;
using backend.Services;
using backend.UnitOfWork;
using backend.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VnPayController : ControllerBase
{
    private readonly IVnPayService _vnPayService;
    private readonly IUnitOfWork _uow;

    public VnPayController(IVnPayService vnPayService, IUnitOfWork uow)
    {
        _vnPayService = vnPayService;
        _uow = uow;
    }

    [HttpPost("create")]
    public IActionResult CreatePayment([FromBody] VnPayRequestDto dto)
    {
        var url = _vnPayService.CreatePaymentUrl(dto, HttpContext);
        return Ok(ApiResponse.Ok(new { paymentUrl = url }));
    }

    [HttpGet("return")]
    public async Task<IActionResult> PaymentReturn(CancellationToken cancellationToken)
    {
        var result = _vnPayService.ProcessCallback(Request.Query);

        if (result.Success && Guid.TryParse(result.OrderId, out var paymentId))
        {
            var payment = await _uow.Payments.Query()
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId, cancellationToken);

            if (payment != null && payment.Status != PaymentStatus.Completed)
            {
                payment.Status = PaymentStatus.Completed; // 2 = success
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = result.TransactionNo ?? string.Empty;
                payment.GatewayResponse = JsonSerializer.Serialize(new { status = "success", message = result.Message });
                payment.UpdatedAt = DateTime.UtcNow;

                var order = await _uow.Orders.Query()
                    .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId, cancellationToken);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.Completed; // 2 = success
                    order.UpdatedAt = DateTime.UtcNow;
                }

                await _uow.SaveAsync(cancellationToken);
            }
        }
        else if (!result.Success && Guid.TryParse(result.OrderId, out var paymentIdFailed))
        {
            var payment = await _uow.Payments.Query()
                .FirstOrDefaultAsync(p => p.PaymentId == paymentIdFailed, cancellationToken);

            if (payment != null && payment.Status != PaymentStatus.Completed)
            {
                payment.Status = PaymentStatus.Failed; // 3 = failed
                payment.GatewayResponse = JsonSerializer.Serialize(new { status = "failed", message = result.Message });
                payment.TransactionId = result.TransactionNo ?? string.Empty;
                payment.UpdatedAt = DateTime.UtcNow;

                var order = await _uow.Orders.Query()
                    .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId, cancellationToken);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.Failed; // 3 = failed
                    order.UpdatedAt = DateTime.UtcNow;
                }

                await _uow.SaveAsync(cancellationToken);
            }
        }

        return Ok(ApiResponse.Ok(result));
    }
}
