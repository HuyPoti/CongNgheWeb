using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Tests.Services;

public class CouponServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IActivityLogService> _mockActivityLog;
    private readonly CouponService _service;

    public CouponServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _mockMapper = new Mock<IMapper>();
        _mockActivityLog = new Mock<IActivityLogService>();

        _mockMapper.Setup(m => m.Map<CouponDto>(It.IsAny<Coupon>()))
            .Returns((Coupon c) => new CouponDto
            {
                CouponId = c.CouponId,
                Code = c.Code,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                IsActive = c.IsActive
            });
        _mockMapper.Setup(m => m.Map<List<CouponDto>>(It.IsAny<List<Coupon>>()))
            .Returns((List<Coupon> list) => list.Select(c => new CouponDto
            {
                CouponId = c.CouponId,
                Code = c.Code,
                IsActive = c.IsActive
            }).ToList());
        _mockMapper.Setup(m => m.Map<CouponUsageDto>(It.IsAny<CouponUsage>()))
            .Returns((CouponUsage u) => new CouponUsageDto { Id = u.Id, CouponId = u.CouponId });

        _mockActivityLog.Setup(a => a.LogAsync(It.IsAny<CreateActivityLogDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActivityLogDto());

        _service = new CouponService(_context, _mockMapper.Object, _mockActivityLog.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_EmptyCode_ThrowsBadRequest()
    {
        var dto = CreateValidDto();
        dto.Code = "";
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*code*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ThrowsBadRequest()
    {
        _context.Coupons.Add(new Coupon { CouponId = Guid.NewGuid(), Code = "SALE10", DiscountType = "percentage", DiscountValue = 10 });
        await _context.SaveChangesAsync();

        var dto = CreateValidDto();
        dto.Code = "sale10"; // Case insensitive
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateAsync_InvalidDiscountType_ThrowsBadRequest()
    {
        var dto = CreateValidDto();
        dto.DiscountType = "invalid";
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*percentage*fixed*");
    }

    [Fact]
    public async Task CreateAsync_PercentageOver100_ThrowsBadRequest()
    {
        var dto = CreateValidDto();
        dto.DiscountType = "percentage";
        dto.DiscountValue = 150;
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*exceed 100*");
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_ThrowsBadRequest()
    {
        var dto = CreateValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(5);
        dto.EndDate = DateTime.UtcNow.AddDays(1);
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*EndDate*");
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesCoupon()
    {
        var dto = CreateValidDto();
        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        var saved = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == "NEWCODE");
        saved.Should().NotBeNull();
    }

    // ============================================================
    // ValidateAsync
    // ============================================================

    [Fact]
    public async Task ValidateAsync_EmptyCode_ReturnsInvalid()
    {
        var result = await _service.ValidateAsync("", 100m, null);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("required");
    }

    [Fact]
    public async Task ValidateAsync_CouponNotFound_ReturnsInvalid()
    {
        var result = await _service.ValidateAsync("NOTEXIST", 100m, null);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ValidateAsync_InactiveCoupon_ReturnsInvalid()
    {
        _context.Coupons.Add(new Coupon
        {
            CouponId = Guid.NewGuid(), Code = "INACTIVE", DiscountType = "fixed", DiscountValue = 10,
            IsActive = false, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        });
        await _context.SaveChangesAsync();

        var result = await _service.ValidateAsync("INACTIVE", 100m, null);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("inactive");
    }

    [Fact]
    public async Task ValidateAsync_BelowMinAmount_ReturnsInvalid()
    {
        _context.Coupons.Add(new Coupon
        {
            CouponId = Guid.NewGuid(), Code = "MINAMOUNT", DiscountType = "fixed", DiscountValue = 10,
            MinOrderAmount = 200, IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        });
        await _context.SaveChangesAsync();

        var result = await _service.ValidateAsync("MINAMOUNT", 100m, null);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("minimum");
    }

    [Fact]
    public async Task ValidateAsync_ValidCoupon_ReturnsValid()
    {
        _context.Coupons.Add(new Coupon
        {
            CouponId = Guid.NewGuid(), Code = "VALID10", DiscountType = "percentage", DiscountValue = 10,
            MinOrderAmount = 0, IsActive = true, PerUserLimit = 5,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        });
        await _context.SaveChangesAsync();

        var result = await _service.ValidateAsync("VALID10", 100m, null);
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(10m); // 10% of 100
        result.FinalAmount.Should().Be(90m);
    }

    [Fact]
    public async Task ValidateAsync_PercentageWithMaxDiscount_CapsDiscount()
    {
        _context.Coupons.Add(new Coupon
        {
            CouponId = Guid.NewGuid(), Code = "CAPPED", DiscountType = "percentage", DiscountValue = 50,
            MaxDiscount = 20, MinOrderAmount = 0, IsActive = true, PerUserLimit = 5,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        });
        await _context.SaveChangesAsync();

        var result = await _service.ValidateAsync("CAPPED", 100m, null);
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(20m); // 50% of 100 = 50, capped at 20
    }

    // ============================================================
    // ApplyAsync
    // ============================================================

    [Fact]
    public async Task ApplyAsync_CouponNotFound_ThrowsNotFound()
    {
        var act = () => _service.ApplyAsync(Guid.NewGuid(), Guid.NewGuid(), null);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ApplyAsync_OrderAlreadyHasCoupon_ThrowsBadRequest()
    {
        var coupon = new Coupon { CouponId = Guid.NewGuid(), Code = "APPLY1", DiscountType = "fixed", DiscountValue = 10, IsActive = true, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), PerUserLimit = 5 };
        var order = new Order { OrderId = Guid.NewGuid(), UserId = Guid.NewGuid(), TotalAmount = 100, CouponId = Guid.NewGuid(), OrderCode = "ORD-1" };
        _context.Coupons.Add(coupon);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var act = () => _service.ApplyAsync(coupon.CouponId, order.OrderId, null);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already*");
    }

    // ============================================================
    // DeactivateAsync
    // ============================================================

    [Fact]
    public async Task DeactivateAsync_NotFound_ThrowsNotFound()
    {
        var act = () => _service.DeactivateAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeactivateAsync_ActiveCoupon_DeactivatesIt()
    {
        var coupon = new Coupon { CouponId = Guid.NewGuid(), Code = "DEACT", DiscountType = "fixed", DiscountValue = 10, IsActive = true };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var result = await _service.DeactivateAsync(coupon.CouponId);
        result.Should().NotBeNull();

        var saved = await _context.Coupons.FirstAsync(c => c.CouponId == coupon.CouponId);
        saved.IsActive.Should().BeFalse();
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFound()
    {
        var act = () => _service.UpdateAsync(Guid.NewGuid(), new UpdateCouponDto());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesCoupon()
    {
        var coupon = new Coupon { CouponId = Guid.NewGuid(), Code = "UPD", DiscountType = "fixed", DiscountValue = 10, IsActive = true };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var dto = new UpdateCouponDto { DiscountValue = 20 };
        var result = await _service.UpdateAsync(coupon.CouponId, dto);

        result.Should().NotBeNull();
        var saved = await _context.Coupons.FirstAsync(c => c.CouponId == coupon.CouponId);
        saved.DiscountValue.Should().Be(20);
    }

    // ============================================================
    // GetAllAsync
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        for (int i = 0; i < 15; i++)
        {
            _context.Coupons.Add(new Coupon
            {
                CouponId = Guid.NewGuid(), Code = $"C{i:D3}", DiscountType = "fixed",
                DiscountValue = 10, IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(1, 10, null, null);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(10);
        result.Page.Should().Be(1);
    }

    // ============================================================
    // Helper
    // ============================================================

    private static CreateCouponDto CreateValidDto() => new()
    {
        Code = "NEWCODE",
        DiscountType = "percentage",
        DiscountValue = 10,
        MinOrderAmount = 0,
        PerUserLimit = 1,
        StartDate = DateTime.UtcNow.AddDays(-1),
        EndDate = DateTime.UtcNow.AddDays(30),
        IsActive = true
    };
}
