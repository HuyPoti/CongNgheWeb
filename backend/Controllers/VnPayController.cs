using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VnPayController : ControllerBase
{
    private readonly IVnPayService _vnPayService;

    public VnPayController(IVnPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    [HttpPost("create")]
    public IActionResult CreatePayment([FromBody] VnPayRequestDto dto)
    {
        var url = _vnPayService.CreatePaymentUrl(dto, HttpContext);
        return Ok(ApiResponse.Ok(new { paymentUrl = url }));
    }

    [HttpGet("return")]
    public IActionResult PaymentReturn()
    {
        var result = _vnPayService.ProcessCallback(Request.Query);
        return Ok(ApiResponse.Ok(result));
    }
}
