using backend.UnitOfWork;
using backend.DTOs;
using backend.Constants;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class DashboardService(IUnitOfWork uow) : IDashboardService
{
	public async Task<OverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;
		var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

		var revenue = await uow.Orders.Query()
			.Where(x => x.Status != OrderStatus.Cancelled && x.PaymentStatus == PaymentStatus.Completed)
			.SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0m;

		var totalOrders = await uow.Orders.Query()
			.CountAsync(x => x.CreatedAt >= startOfMonth, cancellationToken);

		var totalCustomers = await uow.Users.Query()
			.CountAsync(x => x.Role == Models.UserRole.customer, cancellationToken);

		var activeCoupons = await uow.Coupons.Query()
			.CountAsync(x => x.IsActive && x.StartDate <= now && x.EndDate >= now, cancellationToken);

		var activeFlashSales = await uow.FlashSales.Query()
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
		var fromDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-days + 1), DateTimeKind.Utc);

		var grouped = await uow.Orders.Query()
			.Where(x => x.CreatedAt >= fromDate && x.Status != OrderStatus.Cancelled)
			.GroupBy(x => x.CreatedAt.Date)
			.Select(g => new RevenueChartDto
			{
				Date = g.Key,
				Revenue = g.Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.TotalAmount),
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

		var topProducts = await uow.OrderItems.Query()
			.Include(x => x.Order)
			.Include(x => x.Product)
			.Where(x => x.Order != null && x.Order.Status != OrderStatus.Cancelled)
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

		var topCustomers = await uow.Orders.Query()
			.Include(x => x.User)
			.Where(x => x.Status != OrderStatus.Cancelled)
			.GroupBy(x => new { x.UserId, Name = x.User.FullName })
			.Select(g => new TopCustomerDto
			{
				UserId = g.Key.UserId,
				FullName = g.Key.Name,
				TotalOrders = g.Count(),
				TotalSpent = g.Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.TotalAmount)
			})
			.OrderByDescending(x => x.TotalSpent)
			.Take(limit)
			.ToListAsync(cancellationToken);

		return topCustomers;
	}
}
