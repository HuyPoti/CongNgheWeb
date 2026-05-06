using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<OverviewDto>> Overview(CancellationToken cancellationToken)
    {
        var res = await _dashboardService.GetOverviewAsync(cancellationToken);
        return Ok(res);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<IEnumerable<RevenueChartDto>>> Revenue([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var res = await _dashboardService.GetRevenueChartAsync(days, cancellationToken);
        return Ok(res);
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> TopProducts([FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var res = await _dashboardService.GetTopProductsAsync(take, cancellationToken);
        return Ok(res);
    }

    [HttpGet("top-customers")]
    public async Task<ActionResult<IEnumerable<TopCustomerDto>>> TopCustomers([FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var res = await _dashboardService.GetTopCustomersAsync(take, cancellationToken);
        return Ok(res);
    }
}
