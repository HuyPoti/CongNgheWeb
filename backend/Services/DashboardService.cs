using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public interface IDashboardService
{
	Task<OverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
	Task<List<RevenueChartDto>> GetRevenueChartAsync(int days, CancellationToken cancellationToken = default);
	Task<List<TopProductDto>> GetTopProductsAsync(int limit, CancellationToken cancellationToken = default);
	Task<List<TopCustomerDto>> GetTopCustomersAsync(int limit, CancellationToken cancellationToken = default);
}

public class DashboardService(AppDbContext context) : IDashboardService
{
	public async Task<OverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;
		var startOfMonth = new DateTime(now.Year, now.Month, 1);

		var revenue = await context.Orders
			.Where(x => x.Status != 6 && x.PaymentStatus == 2)
			.SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0m;

		var totalOrders = await context.Orders
			.CountAsync(x => x.CreatedAt >= startOfMonth, cancellationToken);

		var totalCustomers = await context.Users
			.CountAsync(x => x.Role == Models.UserRole.customer, cancellationToken);

		var activeCoupons = await context.Coupons
			.CountAsync(x => x.IsActive && x.StartDate <= now && x.EndDate >= now, cancellationToken);

		var activeFlashSales = await context.FlashSales
			.CountAsync(x => x.IsActive && x.StartTime <= now && x.EndTime >= now, cancellationToken);

		return new OverviewDto
		{
			TotalRevenue = revenue,
			TotalOrders = totalOrders,
			TotalCustomers = totalCustomers,
			ActiveCoupons = activeCoupons,
			ActiveFlashSales = activeFlashSales
		};
	}

	public async Task<List<RevenueChartDto>> GetRevenueChartAsync(
		int days,
		CancellationToken cancellationToken = default)
	{
		days = Math.Clamp(days, 1, 365);
		var fromDate = DateTime.UtcNow.Date.AddDays(-days + 1);

		var grouped = await context.Orders
			.Where(x => x.CreatedAt >= fromDate && x.Status != 6)
			.GroupBy(x => x.CreatedAt.Date)
			.Select(g => new RevenueChartDto
			{
				Date = g.Key,
				Revenue = g.Where(x => x.PaymentStatus == 2).Sum(x => x.TotalAmount),
				OrderCount = g.Count()
			})
			.OrderBy(x => x.Date)
			.ToListAsync(cancellationToken);

		return grouped;
	}

	public async Task<List<TopProductDto>> GetTopProductsAsync(
		int limit,
		CancellationToken cancellationToken = default)
	{
		limit = Math.Clamp(limit, 1, 20);

		var topProducts = await context.OrderItems
			.Include(x => x.Order)
			.Include(x => x.Product)
			.Where(x => x.Order != null && x.Order.Status != 6)
			.GroupBy(x => new { x.ProductId, ProductName = x.Product != null ? x.Product.Name : string.Empty })
			.Select(g => new TopProductDto
			{
				ProductId = g.Key.ProductId,
				ProductName = g.Key.ProductName,
				UnitsSold = g.Sum(x => x.Quantity),
				Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
			})
			.OrderByDescending(x => x.UnitsSold)
			.Take(limit)
			.ToListAsync(cancellationToken);

		return topProducts;
	}

	public async Task<List<TopCustomerDto>> GetTopCustomersAsync(
		int limit,
		CancellationToken cancellationToken = default)
	{
		limit = Math.Clamp(limit, 1, 20);

		var topCustomers = await context.Orders
			.Include(x => x.User)
			.Where(x => x.Status != 6)
			.GroupBy(x => new { x.UserId, Name = x.User.FullName })
			.Select(g => new TopCustomerDto
			{
				UserId = g.Key.UserId,
				FullName = g.Key.Name,
				TotalOrders = g.Count(),
				TotalSpent = g.Where(x => x.PaymentStatus == 2).Sum(x => x.TotalAmount)
			})
			.OrderByDescending(x => x.TotalSpent)
			.Take(limit)
			.ToListAsync(cancellationToken);

		return topCustomers;
	}
}
