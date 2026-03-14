using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;

namespace Product.Infrastructure.Persistence;

public class ProductDbContext : DbContext, IApplicationDbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<global::Product.Domain.Entities.Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<global::Product.Domain.Entities.Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
}

