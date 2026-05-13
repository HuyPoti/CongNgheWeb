using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class EfCoreTests
{
    [Fact]
    public async Task InMemory_ShouldWork()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AppDbContext(options);
        
        var userId = Guid.NewGuid();
        context.Wishlists.Add(new Wishlist { WishlistId = Guid.NewGuid(), UserId = userId, ProductId = Guid.NewGuid() });
        await context.SaveChangesAsync();
        
        var count = await context.Wishlists.CountAsync(w => w.UserId == userId);
        count.Should().Be(1);
    }
}
