using Microsoft.EntityFrameworkCore;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
}