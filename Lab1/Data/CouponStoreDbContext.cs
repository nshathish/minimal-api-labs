using Lab1.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Data;

public class CouponStoreDbContext : DbContext
{
    public DbSet<Coupon> Coupons { get; set; } = default!;

    public CouponStoreDbContext(DbContextOptions<CouponStoreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Coupon>().HasData(
            new Coupon
            {
                Id = 1,
                Code = "LAB1",
                Discount = 0.1m,
                Total = 100,
                Claimed = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = 2,
                Code = "LAB2",
                Discount = 0.2m,
                Total = 100,
                Claimed = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}