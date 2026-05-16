using backend.DTOs;

namespace backend.Services;

public interface IDashboardService
{
	Task<OverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
	Task<List<RevenueChartDto>> GetRevenueChartAsync(int days, CancellationToken cancellationToken = default);
	Task<List<TopProductDto>> GetTopProductsAsync(int limit, CancellationToken cancellationToken = default);
	Task<List<TopCustomerDto>> GetTopCustomersAsync(int limit, CancellationToken cancellationToken = default);
}
